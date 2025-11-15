using BehaviourTree.Composites;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    /// <summary>
    /// Comprehensive tests for ActiveSelector composite node.
    /// ActiveSelector re-evaluates from the beginning each tick (reactive behavior).
    /// </summary>
    [TestFixture]
    internal sealed class ActiveSelectorTests
    {
        [Test]
        public void ActiveSelector_ReturnsSuccess_WhenFirstChildSucceeds()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Failed);
            var selector = new ActiveSelector<MockContext>("TestSelector", mock1, mock2);

            // Act
            var result = selector.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(0), "Second child should not execute when first succeeds");
        }

        [Test]
        public void ActiveSelector_ReturnsSuccess_WhenSecondChildSucceeds()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Failed);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var selector = new ActiveSelector<MockContext>("TestSelector", mock1, mock2);

            // Act
            var result = selector.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(1));
        }

        [Test]
        public void ActiveSelector_ReturnsFailed_WhenAllChildrenFail()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Failed);
            var mock2 = new MockBehaviour(BehaviourStatus.Failed);
            var mock3 = new MockBehaviour(BehaviourStatus.Failed);
            var selector = new ActiveSelector<MockContext>("TestSelector", mock1, mock2, mock3);

            // Act
            var result = selector.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock3.UpdateCallCount, Is.EqualTo(1));
        }

        [Test]
        public void ActiveSelector_ReturnsRunning_WhenChildIsRunning()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Failed);
            var mock2 = new MockBehaviour(BehaviourStatus.Running);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var selector = new ActiveSelector<MockContext>("TestSelector", mock1, mock2, mock3);

            // Act
            var result = selector.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Running));
            Assert.That(mock3.UpdateCallCount, Is.EqualTo(0), "Should not execute children after running child");
        }

        [Test]
        public void ActiveSelector_ResetsNonRunningChildren_OnEachTick()
        {
            // Arrange - First child fails, second succeeds
            var mock1 = new MockBehaviour(BehaviourStatus.Failed);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var selector = new ActiveSelector<MockContext>("TestSelector", mock1, mock2);

            // Act - First tick
            selector.Tick(new MockContext());

            // Verify failed children maintain state (not reset) for reactive re-evaluation
            Assert.That(mock1.ResetCallCount, Is.EqualTo(0), "Failed child maintains state for next tick");
            Assert.That(mock2.ResetCallCount, Is.EqualTo(0), "Successful child doesn't need reset on first tick");

            // Act - Second tick (reactive behavior)
            selector.Reset();
            mock1.ResetCallCount = 0; // Clear counter
            mock2.ResetCallCount = 0;

            selector.Tick(new MockContext());

            // Assert - Children are re-evaluated from beginning
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(2));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(2));
        }

        [Test]
        public void ActiveSelector_ReEvaluatesHigherPriorityChildren_WhenChildIsRunning()
        {
            // Arrange
            var highPriority = new MockBehaviour(BehaviourStatus.Failed);
            var lowPriority = new MockBehaviour(BehaviourStatus.Running);
            var selector = new ActiveSelector<MockContext>("TestSelector", highPriority, lowPriority);

            // Act - First tick, low priority runs
            var result1 = selector.Tick(new MockContext());
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));
            Assert.That(highPriority.UpdateCallCount, Is.EqualTo(1));
            Assert.That(lowPriority.UpdateCallCount, Is.EqualTo(1));

            // Now high priority succeeds
            highPriority.SetStatus(BehaviourStatus.Succeeded);

            // Act - Second tick, high priority is re-checked
            var result2 = selector.Tick(new MockContext());

            // Assert - High priority child interrupts low priority
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Succeeded));
            Assert.That(highPriority.UpdateCallCount, Is.EqualTo(2), "High priority re-evaluated");
            Assert.That(lowPriority.UpdateCallCount, Is.EqualTo(1), "Low priority not executed when high priority succeeds");
            Assert.That(lowPriority.ResetCallCount, Is.GreaterThan(0), "Running child should be reset when interrupted");
        }

        [Test]
        public void ActiveSelector_HandlesEmptyChildren()
        {
            // Arrange
            var selector = new ActiveSelector<MockContext>("Empty");

            // Act
            var result = selector.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed), "Empty selector should fail");
        }

        [Test]
        public void ActiveSelector_ResetsProperly()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Failed);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var selector = new ActiveSelector<MockContext>("TestSelector", mock1, mock2);

            // Act
            selector.Tick(new MockContext());
            selector.Reset();

            // Assert
            Assert.That(selector.Status, Is.EqualTo(BehaviourStatus.Ready));
            Assert.That(mock1.Status, Is.EqualTo(BehaviourStatus.Ready));
            Assert.That(mock2.Status, Is.EqualTo(BehaviourStatus.Ready));
        }

        [Test]
        public void ActiveSelector_DifferentFromStandardSelector_InReactivity()
        {
            // Arrange
            var activeChild = new MockBehaviour(BehaviourStatus.Failed);
            var runningChild = new MockBehaviour(BehaviourStatus.Running);
            var activeSelector = new ActiveSelector<MockContext>("Active", activeChild, runningChild);
            var standardSelector = new Selector<MockContext>("Standard", activeChild, runningChild);

            // Act - First tick
            activeSelector.Tick(new MockContext());
            standardSelector.Tick(new MockContext());

            // Change first child to succeed
            activeChild.SetStatus(BehaviourStatus.Succeeded);

            // Act - Second tick
            var activeResult = activeSelector.Tick(new MockContext());
            var standardResult = standardSelector.Tick(new MockContext());

            // Assert
            Assert.That(activeResult, Is.EqualTo(BehaviourStatus.Succeeded),
                "ActiveSelector should re-evaluate and succeed");
            Assert.That(standardResult, Is.EqualTo(BehaviourStatus.Running),
                "Standard Selector should continue with running child");
        }
    }
}
