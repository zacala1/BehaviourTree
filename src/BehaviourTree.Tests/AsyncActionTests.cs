using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BehaviourTree.Behaviours;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    /// <summary>
    /// Comprehensive tests for AsyncAction behavior node.
    /// </summary>
    [TestFixture]
    internal sealed class AsyncActionTests
    {
        [Test]
        public void AsyncAction_ReturnsRunning_OnFirstTick()
        {
            var taskStarted = false;
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                taskStarted = true;
                await Task.Delay(100, token);
                return BehaviourStatus.Succeeded;
            });

            var result = asyncAction.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Running));
            Assert.That(taskStarted, Is.True);
        }

        [Test]
        public void AsyncAction_ReturnsSuccess_WhenTaskCompletes()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                return BehaviourStatus.Succeeded;
            });

            var result1 = asyncAction.Tick(new MockContext());
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));

            Thread.Sleep(50);
            var result2 = asyncAction.Tick(new MockContext());

            Assert.That(result2, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void AsyncAction_ReturnsFailed_WhenTaskFails()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                return BehaviourStatus.Failed;
            });

            asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            var result = asyncAction.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
        }

        [Test]
        public void AsyncAction_CanBeExecutedMultipleTimes()
        {
            var executionCount = 0;
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                executionCount++;
                await Task.Delay(10, token);
                return BehaviourStatus.Succeeded;
            });

            asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            asyncAction.Tick(new MockContext());
            asyncAction.Reset();

            asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            asyncAction.Tick(new MockContext());

            Assert.That(executionCount, Is.EqualTo(2));
        }

        [Test]
        public void AsyncAction_CancelsPreviousTask_WhenReset()
        {
            var taskCancelled = false;
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                try
                {
                    await Task.Delay(1000, token);
                    return BehaviourStatus.Succeeded;
                }
                catch (TaskCanceledException)
                {
                    taskCancelled = true;
                    throw;
                }
            });

            asyncAction.Tick(new MockContext());
            Thread.Sleep(10);
            asyncAction.Reset();
            Thread.Sleep(50);

            Assert.That(taskCancelled, Is.True);
            Assert.That(asyncAction.Status, Is.EqualTo(BehaviourStatus.Ready));
        }

        [Test]
        public void AsyncAction_HandlesExceptions_ReturnsFailure()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                throw new InvalidOperationException("Test exception");
            });

            asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            var result = asyncAction.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
        }

        [Test]
        public void AsyncAction_PassesContext_ToAsyncFunction()
        {
            MockContext? capturedContext = null;
            var expectedContext = new MockContext();
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                capturedContext = ctx;
                await Task.Delay(10, token);
                return BehaviourStatus.Succeeded;
            });

            asyncAction.Tick(expectedContext);
            Thread.Sleep(50);

            Assert.That(capturedContext, Is.SameAs(expectedContext));
        }

        [Test]
        public void AsyncAction_SupportsCancellationToken()
        {
            var cancellationRequested = false;
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                for (int i = 0; i < 100; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        cancellationRequested = true;
                        token.ThrowIfCancellationRequested();
                    }
                    await Task.Delay(10);
                    if (token.IsCancellationRequested)
                    {
                        cancellationRequested = true;
                        token.ThrowIfCancellationRequested();
                    }
                }
                return BehaviourStatus.Succeeded;
            });

            asyncAction.Tick(new MockContext());
            Thread.Sleep(20);
            asyncAction.Reset();
            Thread.Sleep(50);

            Assert.That(cancellationRequested, Is.True);
        }

        [Test]
        public void AsyncAction_ReturnsRunning_WhileTaskExecutes()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(200, token);
                return BehaviourStatus.Succeeded;
            });

            var result1 = asyncAction.Tick(new MockContext());
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));

            Thread.Sleep(50);
            var result2 = asyncAction.Tick(new MockContext());
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Running));

            Thread.Sleep(50);
            var result3 = asyncAction.Tick(new MockContext());
            Assert.That(result3, Is.EqualTo(BehaviourStatus.Running));

            Thread.Sleep(150);
            var result4 = asyncAction.Tick(new MockContext());
            Assert.That(result4, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void AsyncAction_DoesNotStartNewTask_WhileRunning()
        {
            var startCount = 0;
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                Interlocked.Increment(ref startCount);
                await Task.Delay(100, token);
                return BehaviourStatus.Succeeded;
            });

            asyncAction.Tick(new MockContext());
            asyncAction.Tick(new MockContext());
            asyncAction.Tick(new MockContext());
            Thread.Sleep(150);

            Assert.That(startCount, Is.EqualTo(1));
        }

        [Test]
        public void AsyncAction_NameProperty_IsSet()
        {
            var asyncAction = new AsyncAction<MockContext>("TestName", async (ctx, token) =>
            {
                await Task.CompletedTask;
                return BehaviourStatus.Succeeded;
            });

            Assert.That(asyncAction.Name, Is.EqualTo("TestName"));
        }

        [Test]
        public void AsyncAction_QuickCompletion_SucceedsOnFirstTick()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", (ctx, token) =>
            {
                return Task.FromResult(BehaviourStatus.Succeeded);
            });

            // Synchronously completed task should return result on the same tick
            var result = asyncAction.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void AsyncAction_QuickFailure_FailsOnFirstTick()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", (ctx, token) =>
            {
                return Task.FromResult(BehaviourStatus.Failed);
            });

            var result = asyncAction.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
        }

        [Test]
        public void AsyncAction_LastException_PopulatedOnFault()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                throw new InvalidOperationException("Test fault");
            });

            asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            asyncAction.Tick(new MockContext());

            Assert.That(asyncAction.LastException, Is.Not.Null);
            Assert.That(asyncAction.LastException, Is.TypeOf<InvalidOperationException>());
            Assert.That(asyncAction.LastException!.Message, Is.EqualTo("Test fault"));
        }

        [Test]
        public void AsyncAction_LastException_NullOnSuccess()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", (ctx, token) =>
            {
                return Task.FromResult(BehaviourStatus.Succeeded);
            });

            asyncAction.Tick(new MockContext());

            Assert.That(asyncAction.LastException, Is.Null);
        }

        [Test]
        public void AsyncAction_LastException_PopulatedOnSyncThrow()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", (Func<MockContext, CancellationToken, Task<BehaviourStatus>>)((ctx, token) =>
            {
                throw new ArgumentException("Sync throw");
            }));

            var result = asyncAction.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(asyncAction.LastException, Is.TypeOf<ArgumentException>());
        }

        [Test]
        public void AsyncAction_WasCancelled_TrueOnCancelCondition()
        {
            var shouldCancel = false;
            var asyncAction = new AsyncAction<MockContext>("Test",
                async (ctx, token) =>
                {
                    await Task.Delay(1000, token);
                    return BehaviourStatus.Succeeded;
                },
                ctx => shouldCancel);

            asyncAction.Tick(new MockContext());
            shouldCancel = true;
            var result = asyncAction.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(asyncAction.WasCancelled, Is.True);
        }

        [Test]
        public void AsyncAction_WasCancelled_TrueOnTimeout()
        {
            var asyncAction = new AsyncAction<MockContext>("Test",
                async (ctx, token) =>
                {
                    await Task.Delay(5000, token);
                    return BehaviourStatus.Succeeded;
                },
                TimeSpan.FromMilliseconds(20));

            asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            var result = asyncAction.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(asyncAction.WasCancelled, Is.True);
        }

        [Test]
        public void AsyncAction_WasCancelled_FalseOnNormalFailure()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", (ctx, token) =>
            {
                return Task.FromResult(BehaviourStatus.Failed);
            });

            asyncAction.Tick(new MockContext());

            Assert.That(asyncAction.WasCancelled, Is.False);
        }

        [Test]
        public void AsyncAction_WasCancelled_ResetOnReuse()
        {
            var shouldCancel = false;
            var asyncAction = new AsyncAction<MockContext>("Test",
                async (ctx, token) =>
                {
                    await Task.Delay(1000, token);
                    return BehaviourStatus.Succeeded;
                },
                ctx => shouldCancel);

            // First: cancel
            asyncAction.Tick(new MockContext());
            shouldCancel = true;
            asyncAction.Tick(new MockContext());
            Assert.That(asyncAction.WasCancelled, Is.True);

            // Reset and reuse - WasCancelled resets on next initialization (Tick)
            asyncAction.Reset();
            shouldCancel = false;
            asyncAction.Tick(new MockContext()); // OnInitialize resets WasCancelled
            Assert.That(asyncAction.WasCancelled, Is.False);
        }

        [Test]
        public void AsyncAction_ExternalToken_CancelsAction()
        {
            using var externalCts = new CancellationTokenSource();
            var asyncAction = new AsyncAction<MockContext>("Test",
                async (ctx, token) =>
                {
                    await Task.Delay(5000, token);
                    return BehaviourStatus.Succeeded;
                },
                null,
                default,
                externalCts.Token);

            asyncAction.Tick(new MockContext());
            externalCts.Cancel();
            Thread.Sleep(50);
            var result = asyncAction.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(asyncAction.WasCancelled, Is.True);
        }

        [Test]
        public void AsyncAction_CleanupDoesNotBlock()
        {
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10000, token);
                return BehaviourStatus.Succeeded;
            });

            asyncAction.Tick(new MockContext());
            Thread.Sleep(10);

            var sw = Stopwatch.StartNew();
            asyncAction.Reset();
            sw.Stop();

            // Reset should NOT block - must complete in under 50ms (was 100ms+ before)
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(50),
                "Reset should not block waiting for task completion");
        }
    }
}
