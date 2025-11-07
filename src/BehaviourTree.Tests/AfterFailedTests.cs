using BehaviourTree.Decorators;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    /// <summary>
    /// Comprehensive tests for AfterFailed decorator.
    /// AfterFailed executes a callback action after its child fails.
    /// </summary>
    [TestFixture]
    internal sealed class AfterFailedTests
    {
        [Test]
        public void AfterFailed_ExecutesCallback_WhenChildFails()
        {
            // Arrange
            var callbackExecuted = false;
            var child = new MockBehaviour(BehaviourStatus.Failed);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx => callbackExecuted = true);

            // Act
            var result = decorator.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(callbackExecuted, Is.True, "Callback should execute after child fails");
        }

        [Test]
        public void AfterFailed_DoesNotExecuteCallback_WhenChildSucceeds()
        {
            // Arrange
            var callbackExecuted = false;
            var child = new MockBehaviour(BehaviourStatus.Succeeded);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx => callbackExecuted = true);

            // Act
            var result = decorator.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
            Assert.That(callbackExecuted, Is.False, "Callback should not execute when child succeeds");
        }

        [Test]
        public void AfterFailed_DoesNotExecuteCallback_WhenChildIsRunning()
        {
            // Arrange
            var callbackExecuted = false;
            var child = new MockBehaviour(BehaviourStatus.Running);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx => callbackExecuted = true);

            // Act
            var result = decorator.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Running));
            Assert.That(callbackExecuted, Is.False, "Callback should not execute while child is running");
        }

        [Test]
        public void AfterFailed_PassesContext_ToCallback()
        {
            // Arrange
            MockContext? capturedContext = null;
            var expectedContext = new MockContext();
            var child = new MockBehaviour(BehaviourStatus.Failed);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx => capturedContext = ctx);

            // Act
            decorator.Tick(expectedContext);

            // Assert
            Assert.That(capturedContext, Is.SameAs(expectedContext), "Context should be passed to callback");
        }

        [Test]
        public void AfterFailed_ExecutesCallback_OnlyOnce_PerExecution()
        {
            // Arrange
            var callbackCount = 0;
            var child = new MockBehaviour(BehaviourStatus.Failed);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx => callbackCount++);

            // Act
            decorator.Tick(new MockContext());
            decorator.Tick(new MockContext()); // Should not execute again while failed

            // Assert
            Assert.That(callbackCount, Is.EqualTo(1), "Callback should execute only once per execution");
        }

        [Test]
        public void AfterFailed_ExecutesCallback_OnEachFailedExecution()
        {
            // Arrange
            var callbackCount = 0;
            var child = new MockBehaviour(BehaviourStatus.Failed);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx => callbackCount++);

            // Act - First execution
            decorator.Tick(new MockContext());
            decorator.Reset();

            // Second execution
            decorator.Tick(new MockContext());
            decorator.Reset();

            // Third execution
            decorator.Tick(new MockContext());

            // Assert
            Assert.That(callbackCount, Is.EqualTo(3), "Callback should execute on each failed execution");
        }

        [Test]
        public void AfterFailed_HandlesCallbackExceptions_Gracefully()
        {
            // Arrange
            var child = new MockBehaviour(BehaviourStatus.Failed);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx =>
            {
                throw new System.Exception("Callback exception");
            });

            // Act & Assert - Should not throw, exception should be caught internally
            Assert.DoesNotThrow(() =>
            {
                var result = decorator.Tick(new MockContext());
                Assert.That(result, Is.EqualTo(BehaviourStatus.Failed), "Should still return failure despite callback exception");
            });
        }

        [Test]
        public void AfterFailed_ResetsProperly()
        {
            // Arrange
            var child = new MockBehaviour(BehaviourStatus.Failed);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx => { });

            // Act
            decorator.Tick(new MockContext());
            decorator.Reset();

            // Assert
            Assert.That(decorator.Status, Is.EqualTo(BehaviourStatus.Ready));
            Assert.That(child.Status, Is.EqualTo(BehaviourStatus.Ready));
        }

        [Test]
        public void AfterFailed_WorksWithRunningChild()
        {
            // Arrange
            var callbackCount = 0;
            var child = new MockBehaviour(BehaviourStatus.Running);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx => callbackCount++);

            // Act - Tick while running
            var result1 = decorator.Tick(new MockContext());
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));
            Assert.That(callbackCount, Is.EqualTo(0));

            // Child fails
            child.SetStatus(BehaviourStatus.Failed);
            var result2 = decorator.Tick(new MockContext());

            // Assert
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(callbackCount, Is.EqualTo(1), "Callback should execute when running child fails");
        }

        [Test]
        public void AfterFailed_CallbackCanModifyContext()
        {
            // Arrange
            var context = new MockContext();
            context.Counter = 0;
            var child = new MockBehaviour(BehaviourStatus.Failed);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx => ctx.Counter = 99);

            // Act
            decorator.Tick(context);

            // Assert
            Assert.That(context.Counter, Is.EqualTo(99), "Callback should be able to modify context");
        }

        [Test]
        public void AfterFailed_UsefulForErrorHandling()
        {
            // Arrange - Simulate error logging
            var errorLogged = false;
            var child = new MockBehaviour(BehaviourStatus.Failed);
            var decorator = new AfterFailed<MockContext>("Test", child, ctx =>
            {
                // Log error or perform cleanup
                errorLogged = true;
            });

            // Act
            var result = decorator.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(errorLogged, Is.True, "Error handler should be called");
        }

        [Test]
        public void AfterFailed_NameProperty_IsSet()
        {
            // Arrange & Act
            var child = new MockBehaviour(BehaviourStatus.Failed);
            var decorator = new AfterFailed<MockContext>("TestName", child, ctx => { });

            // Assert
            Assert.That(decorator.Name, Is.EqualTo("TestName"));
        }
    }
}
