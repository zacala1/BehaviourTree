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
            // Arrange
            var taskStarted = false;
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                taskStarted = true;
                await Task.Delay(100, token);
                return BehaviourStatus.Succeeded;
            });

            // Act
            var result = asyncAction.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Running), "Should return Running while task executes");
            Assert.That(taskStarted, Is.True, "Task should start immediately");
        }

        [Test]
        public void AsyncAction_ReturnsSuccess_WhenTaskCompletes()
        {
            // Arrange
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                return BehaviourStatus.Succeeded;
            });

            // Act - First tick starts the task
            var result1 = asyncAction.Tick(new MockContext());
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));

            // Wait for task to complete
            Thread.Sleep(50);

            // Second tick should return completed status
            var result2 = asyncAction.Tick(new MockContext());

            // Assert
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void AsyncAction_ReturnsFailed_WhenTaskFails()
        {
            // Arrange
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                return BehaviourStatus.Failed;
            });

            // Act
            var result1 = asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            var result2 = asyncAction.Tick(new MockContext());

            // Assert
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Failed));
        }

        [Test]
        public void AsyncAction_CanBeExecutedMultipleTimes()
        {
            // Arrange
            var executionCount = 0;
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                executionCount++;
                await Task.Delay(10, token);
                return BehaviourStatus.Succeeded;
            });

            // Act - First execution
            asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            asyncAction.Tick(new MockContext());
            asyncAction.Reset();

            // Second execution
            asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            asyncAction.Tick(new MockContext());

            // Assert
            Assert.That(executionCount, Is.EqualTo(2), "Should execute twice");
        }

        [Test]
        public void AsyncAction_CancelsPreviousTask_WhenReset()
        {
            // Arrange
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

            // Act
            asyncAction.Tick(new MockContext());
            Thread.Sleep(10); // Give task time to start
            asyncAction.Reset(); // Should cancel the running task
            Thread.Sleep(50); // Give cancellation time to propagate

            // Assert
            Assert.That(taskCancelled, Is.True, "Task should be cancelled on reset");
            Assert.That(asyncAction.Status, Is.EqualTo(BehaviourStatus.Ready));
        }

        [Test]
        public void AsyncAction_HandlesExceptions_ReturnsFailure()
        {
            // Arrange
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                throw new System.Exception("Test exception");
            });

            // Act
            asyncAction.Tick(new MockContext());
            Thread.Sleep(50);
            var result = asyncAction.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed), "Should return Failed when exception occurs");
        }

        [Test]
        public void AsyncAction_PassesContext_ToAsyncFunction()
        {
            // Arrange
            MockContext? capturedContext = null;
            var expectedContext = new MockContext();
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                capturedContext = ctx;
                await Task.Delay(10, token);
                return BehaviourStatus.Succeeded;
            });

            // Act
            asyncAction.Tick(expectedContext);
            Thread.Sleep(50);

            // Assert
            Assert.That(capturedContext, Is.SameAs(expectedContext), "Context should be passed to async function");
        }

        [Test]
        public void AsyncAction_SupportsCancellationToken()
        {
            // Arrange
            var cancellationRequested = false;
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                // Simulate long-running operation checking cancellation
                for (int i = 0; i < 100; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        cancellationRequested = true;
                        token.ThrowIfCancellationRequested();
                    }
                    await Task.Delay(10, token);
                }
                return BehaviourStatus.Succeeded;
            });

            // Act
            asyncAction.Tick(new MockContext());
            Thread.Sleep(20); // Let it start
            asyncAction.Reset(); // Cancel
            Thread.Sleep(50);

            // Assert
            Assert.That(cancellationRequested, Is.True, "Cancellation token should be signaled");
        }

        [Test]
        public void AsyncAction_ReturnsRunning_WhileTaskExecutes()
        {
            // Arrange
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(200, token);
                return BehaviourStatus.Succeeded;
            });

            // Act & Assert - Multiple ticks while running
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
            // Arrange
            var startCount = 0;
            var asyncAction = new AsyncAction<MockContext>("Test", async (ctx, token) =>
            {
                Interlocked.Increment(ref startCount);
                await Task.Delay(100, token);
                return BehaviourStatus.Succeeded;
            });

            // Act - Multiple ticks
            asyncAction.Tick(new MockContext());
            asyncAction.Tick(new MockContext());
            asyncAction.Tick(new MockContext());
            Thread.Sleep(150);

            // Assert
            Assert.That(startCount, Is.EqualTo(1), "Should only start one task");
        }

        [Test]
        public void AsyncAction_NameProperty_IsSet()
        {
            // Arrange & Act
            var asyncAction = new AsyncAction<MockContext>("TestName", async (ctx, token) =>
            {
                await Task.CompletedTask;
                return BehaviourStatus.Succeeded;
            });

            // Assert
            Assert.That(asyncAction.Name, Is.EqualTo("TestName"));
        }

        [Test]
        public void AsyncAction_QuickCompletion_SucceedsImmediately()
        {
            // Arrange - Task that completes synchronously
            var asyncAction = new AsyncAction<MockContext>("Test", (ctx, token) =>
            {
                return Task.FromResult(BehaviourStatus.Succeeded);
            });

            // Act
            var result1 = asyncAction.Tick(new MockContext());

            // Small delay to ensure task completion is detected
            Thread.Sleep(10);
            var result2 = asyncAction.Tick(new MockContext());

            // Assert
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Succeeded));
        }
    }
}
