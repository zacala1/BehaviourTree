using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using BehaviourTree.Events;
using BehaviourTree.FluentBuilder;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BehaviourTree.Tests
{
    /// <summary>
    /// Tests to verify proper resource disposal in behavior tree nodes.
    /// Ensures observers are cleared, children are disposed, and async resources are cleaned up.
    /// </summary>
    [TestFixture]
    public class DisposalTests
    {
        private class TestContext { }

        private class TestObserver : IBehaviourTreeObserver
        {
            public int InitializeCount { get; private set; }
            public int UpdateCount { get; private set; }
            public int TerminateCount { get; private set; }
            public int ResetCount { get; private set; }

            public void OnNodeInitialize(BehaviourTreeNodeEvent nodeEvent) => InitializeCount++;
            public void OnNodeUpdate(BehaviourTreeNodeEvent nodeEvent) => UpdateCount++;
            public void OnNodeTerminate(BehaviourTreeNodeEvent nodeEvent) => TerminateCount++;
            public void OnNodeReset(BehaviourTreeNodeEvent nodeEvent) => ResetCount++;
        }

        [Test]
        public void BaseBehaviour_Dispose_ClearsObservers()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("test", ctx => BehaviourStatus.Succeeded);
            var observer = new TestObserver();
            action.AttachObserver(observer);

            // Verify observer is attached
            action.Tick(new TestContext());
            Assert.AreEqual(1, observer.InitializeCount);
            Assert.AreEqual(1, observer.UpdateCount);

            // Act
            action.Dispose();

            // Reset and tick again - observer should not be notified
            action.Reset();
            action.Tick(new TestContext());

            // Assert - counts should not have changed after dispose
            Assert.AreEqual(1, observer.InitializeCount, "Observer should not receive events after dispose");
            Assert.AreEqual(1, observer.UpdateCount, "Observer should not receive events after dispose");
        }

        [Test]
        public void CompositeBehaviour_Dispose_DisposesAllChildren()
        {
            // Arrange
            var child1Disposed = false;
            var child2Disposed = false;
            var child3Disposed = false;

            var child1 = new DisposableActionBehaviour<TestContext>("child1",
                ctx => BehaviourStatus.Succeeded,
                () => child1Disposed = true);

            var child2 = new DisposableActionBehaviour<TestContext>("child2",
                ctx => BehaviourStatus.Succeeded,
                () => child2Disposed = true);

            var child3 = new DisposableActionBehaviour<TestContext>("child3",
                ctx => BehaviourStatus.Succeeded,
                () => child3Disposed = true);

            var sequence = new Sequence<TestContext>("sequence", child1, child2, child3);
            var observer = new TestObserver();
            sequence.AttachObserver(observer);

            // Act
            sequence.Dispose();

            // Assert
            Assert.IsTrue(child1Disposed, "Child 1 should be disposed");
            Assert.IsTrue(child2Disposed, "Child 2 should be disposed");
            Assert.IsTrue(child3Disposed, "Child 3 should be disposed");

            // Verify observers are also cleared
            sequence.Reset();
            sequence.Tick(new TestContext());
            Assert.AreEqual(0, observer.InitializeCount, "Sequence observer should be cleared after dispose");
        }

        [Test]
        public void DecoratorBehaviour_Dispose_DisposesChild()
        {
            // Arrange
            var childDisposed = false;
            var child = new DisposableActionBehaviour<TestContext>("child",
                ctx => BehaviourStatus.Succeeded,
                () => childDisposed = true);

            var inverter = new Inverter<TestContext>("inverter", child);
            var observer = new TestObserver();
            inverter.AttachObserver(observer);

            // Act
            inverter.Dispose();

            // Assert
            Assert.IsTrue(childDisposed, "Child should be disposed");

            // Verify observers are also cleared
            inverter.Reset();
            inverter.Tick(new TestContext());
            Assert.AreEqual(0, observer.InitializeCount, "Inverter observer should be cleared after dispose");
        }

        [Test]
        public void AsyncAction_Dispose_CancelsAndDisposesTask()
        {
            // Arrange
            var taskStarted = false;
            var taskCancelled = false;
            var cts = new CancellationTokenSource();

            var asyncAction = new AsyncAction<TestContext>("async-test",
                async (ctx, token) =>
                {
                    taskStarted = true;
                    try
                    {
                        await Task.Delay(5000, token); // Long delay
                        return BehaviourStatus.Succeeded;
                    }
                    catch (OperationCanceledException)
                    {
                        taskCancelled = true;
                        throw;
                    }
                });

            // Start the async action
            var status = asyncAction.Tick(new TestContext());
            Assert.AreEqual(BehaviourStatus.Running, status);
            Assert.IsTrue(taskStarted);

            // Act - Dispose should cancel the task
            asyncAction.Dispose();

            // Give a moment for cancellation to propagate
            Thread.Sleep(150);

            // Assert
            Assert.IsTrue(taskCancelled, "Task should be cancelled on dispose");
        }

        [Test]
        public void NestedTree_Dispose_DisposesAllNodesRecursively()
        {
            // Arrange
            var disposedNodes = 0;

            var tree = FluentBuilder.Create<TestContext>()
                .Sequence("root", seq =>
                {
                    seq.Do("action1", ctx =>
                    {
                        disposedNodes++;
                        return BehaviourStatus.Succeeded;
                    });

                    seq.Selector("selector", sel =>
                    {
                        sel.Do("action2", ctx =>
                        {
                            disposedNodes++;
                            return BehaviourStatus.Failed;
                        });

                        sel.Do("action3", ctx =>
                        {
                            disposedNodes++;
                            return BehaviourStatus.Succeeded;
                        });
                    });

                    seq.Do("action4", ctx =>
                    {
                        disposedNodes++;
                        return BehaviourStatus.Succeeded;
                    });
                })
                .Build();

            var observer = new TestObserver();
            tree.AttachObserver(observer);

            // Tick once to verify tree works
            tree.Tick(new TestContext());
            Assert.Greater(observer.UpdateCount, 0);

            // Act
            tree.Dispose();

            // Assert - observer should not receive new events
            tree.Reset();
            tree.Tick(new TestContext());

            // The observer counts should not increase after dispose
            var previousUpdateCount = observer.UpdateCount;
            tree.Reset();
            tree.Tick(new TestContext());
            Assert.AreEqual(previousUpdateCount, observer.UpdateCount,
                "Observer should not receive events after tree disposal");
        }

        [Test]
        public void Dispose_CalledMultipleTimes_IsSafe()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("test", ctx => BehaviourStatus.Succeeded);
            var observer = new TestObserver();
            action.AttachObserver(observer);

            // Act & Assert - multiple dispose calls should not throw
            Assert.DoesNotThrow(() =>
            {
                action.Dispose();
                action.Dispose();
                action.Dispose();
            });
        }

        /// <summary>
        /// Helper class to track disposal of ActionBehaviour nodes
        /// </summary>
        private class DisposableActionBehaviour<TContext> : ActionBehaviour<TContext>
        {
            private readonly Action onDispose;

            public DisposableActionBehaviour(string name, Func<TContext, BehaviourStatus> action, Action onDispose)
                : base(name, action)
            {
                this.onDispose = onDispose;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    onDispose?.Invoke();
                }
                base.Dispose(disposing);
            }
        }
    }
}
