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
    internal sealed class DisposalTests
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
        public void WhenBehaviourDisposed_ClearsObservers()
        {
            var sut = new ActionBehaviour<TestContext>("test", ctx => BehaviourStatus.Succeeded);
            var observer = new TestObserver();
            sut.AttachObserver(observer);

            sut.Tick(new TestContext());
            var initialInitializeCount = observer.InitializeCount;
            var initialUpdateCount = observer.UpdateCount;

            sut.Dispose();

            sut.Reset();
            sut.Tick(new TestContext());

            Assert.That(observer.InitializeCount, Is.EqualTo(initialInitializeCount),
                "Observer should not receive events after dispose");
            Assert.That(observer.UpdateCount, Is.EqualTo(initialUpdateCount),
                "Observer should not receive events after dispose");
        }

        [Test]
        public void WhenCompositeDisposed_DisposesAllChildren()
        {
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

            var sut = new Sequence<TestContext>("sequence", child1, child2, child3);
            var observer = new TestObserver();
            sut.AttachObserver(observer);

            sut.Dispose();

            Assert.That(child1Disposed, Is.True, "Child 1 should be disposed");
            Assert.That(child2Disposed, Is.True, "Child 2 should be disposed");
            Assert.That(child3Disposed, Is.True, "Child 3 should be disposed");

            sut.Reset();
            sut.Tick(new TestContext());
            Assert.That(observer.InitializeCount, Is.EqualTo(0),
                "Composite observer should be cleared after dispose");
        }

        [Test]
        public void WhenDecoratorDisposed_DisposesChild()
        {
            var childDisposed = false;
            var child = new DisposableActionBehaviour<TestContext>("child",
                ctx => BehaviourStatus.Succeeded,
                () => childDisposed = true);

            var sut = new Inverter<TestContext>("inverter", child);
            var observer = new TestObserver();
            sut.AttachObserver(observer);

            sut.Dispose();

            Assert.That(childDisposed, Is.True, "Child should be disposed");

            sut.Reset();
            sut.Tick(new TestContext());
            Assert.That(observer.InitializeCount, Is.EqualTo(0),
                "Decorator observer should be cleared after dispose");
        }

        [Test]
        public void WhenAsyncActionDisposed_CancelsAndDisposesTask()
        {
            var taskStarted = false;
            var taskCancelled = false;

            var sut = new AsyncAction<TestContext>("async-test",
                async (ctx, token) =>
                {
                    taskStarted = true;
                    try
                    {
                        await Task.Delay(5000, token);
                        return BehaviourStatus.Succeeded;
                    }
                    catch (OperationCanceledException)
                    {
                        taskCancelled = true;
                        throw;
                    }
                });

            var status = sut.Tick(new TestContext());
            Assert.That(status, Is.EqualTo(BehaviourStatus.Running));
            Assert.That(taskStarted, Is.True);

            sut.Dispose();

            Thread.Sleep(150);

            Assert.That(taskCancelled, Is.True, "Task should be cancelled on dispose");
        }

        [Test]
        public void WhenNestedTreeDisposed_DisposesAllNodesRecursively()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("root", seq =>
                {
                    seq.Do("action1", ctx => BehaviourStatus.Succeeded);

                    seq.Selector("selector", sel =>
                    {
                        sel.Do("action2", ctx => BehaviourStatus.Failed);
                        sel.Do("action3", ctx => BehaviourStatus.Succeeded);
                    });

                    seq.Do("action4", ctx => BehaviourStatus.Succeeded);
                })
                .Build();

            var observer = new TestObserver();
            sut.AttachObserver(observer);

            sut.Tick(new TestContext());
            var updateCountBeforeDispose = observer.UpdateCount;
            Assert.That(updateCountBeforeDispose, Is.GreaterThan(0));

            sut.Dispose();

            sut.Reset();
            sut.Tick(new TestContext());

            Assert.That(observer.UpdateCount, Is.EqualTo(updateCountBeforeDispose),
                "Observer should not receive events after tree disposal");
        }

        [Test]
        public void WhenDisposeCalledMultipleTimes_DoesNotThrow()
        {
            var sut = new ActionBehaviour<TestContext>("test", ctx => BehaviourStatus.Succeeded);
            var observer = new TestObserver();
            sut.AttachObserver(observer);

            Assert.DoesNotThrow(() =>
            {
                sut.Dispose();
                sut.Dispose();
                sut.Dispose();
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
