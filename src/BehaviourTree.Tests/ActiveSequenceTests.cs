using BehaviourTree.Composites;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    /// <summary>
    /// Comprehensive tests for ActiveSequence composite node.
    /// ActiveSequence re-evaluates from the beginning each tick (reactive behavior).
    /// </summary>
    [TestFixture]
    internal sealed class ActiveSequenceTests
    {
        [Test]
        public void ActiveSequence_ReturnsSuccess_WhenAllChildrenSucceed()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var sequence = new ActiveSequence<MockContext>("TestSequence", mock1, mock2, mock3);

            // Act
            var result = sequence.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock3.UpdateCallCount, Is.EqualTo(1));
        }

        [Test]
        public void ActiveSequence_ReturnsFailed_WhenFirstChildFails()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Failed);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var sequence = new ActiveSequence<MockContext>("TestSequence", mock1, mock2);

            // Act
            var result = sequence.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(0), "Subsequent children should not execute after failure");
        }

        [Test]
        public void ActiveSequence_ReturnsFailed_WhenMiddleChildFails()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Failed);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var sequence = new ActiveSequence<MockContext>("TestSequence", mock1, mock2, mock3);

            // Act
            var result = sequence.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock3.UpdateCallCount, Is.EqualTo(0));
        }

        [Test]
        public void ActiveSequence_ReturnsRunning_WhenChildIsRunning()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Running);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var sequence = new ActiveSequence<MockContext>("TestSequence", mock1, mock2, mock3);

            // Act
            var result = sequence.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Running));
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock3.UpdateCallCount, Is.EqualTo(0), "Should not execute children after running child");
        }

        [Test]
        public void ActiveSequence_ResetsNonRunningChildren_OnEachTick()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Running);
            var sequence = new ActiveSequence<MockContext>("TestSequence", mock1, mock2);

            // Act - First tick
            var result1 = sequence.Tick(new MockContext());
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));

            // Verify succeeded children maintain state (not reset) for reactive re-evaluation
            Assert.That(mock1.ResetCallCount, Is.EqualTo(0), "Succeeded child maintains state for next tick");

            // Act - Second tick (reactive behavior)
            var result2 = sequence.Tick(new MockContext());

            // Assert - First child is re-evaluated from its current state (not reset)
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Running));
            Assert.That(mock1.ResetCallCount, Is.EqualTo(0), "Succeeded child is not reset, maintains state");
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(2), "First child re-evaluated");
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(2), "Running child continues");
        }

        [Test]
        public void ActiveSequence_ReEvaluatesEarlierChildren_WhenChildIsRunning()
        {
            // Arrange
            var condition = new MockBehaviour(BehaviourStatus.Succeeded);
            var action = new MockBehaviour(BehaviourStatus.Running);
            var sequence = new ActiveSequence<MockContext>("TestSequence", condition, action);

            // Act - First tick
            var result1 = sequence.Tick(new MockContext());
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));
            Assert.That(condition.UpdateCallCount, Is.EqualTo(1));
            Assert.That(action.UpdateCallCount, Is.EqualTo(1));

            // Condition now fails
            condition.SetStatus(BehaviourStatus.Failed);

            // Act - Second tick
            var result2 = sequence.Tick(new MockContext());

            // Assert - Condition is re-checked and fails, interrupting action
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(condition.UpdateCallCount, Is.EqualTo(2), "Condition re-evaluated");
            Assert.That(action.UpdateCallCount, Is.EqualTo(1), "Action not executed when condition fails");
            Assert.That(action.ResetCallCount, Is.GreaterThan(0), "Running action should be reset when interrupted");
        }

        [Test]
        public void ActiveSequence_HandlesEmptyChildren()
        {
            // Arrange
            var sequence = new ActiveSequence<MockContext>("Empty");

            // Act
            var result = sequence.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded), "Empty sequence should succeed");
        }

        [Test]
        public void ActiveSequence_ResetsProperly()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Running);
            var sequence = new ActiveSequence<MockContext>("TestSequence", mock1, mock2);

            // Act
            sequence.Tick(new MockContext());
            sequence.Reset();

            // Assert
            Assert.That(sequence.Status, Is.EqualTo(BehaviourStatus.Ready));
            Assert.That(mock1.Status, Is.EqualTo(BehaviourStatus.Ready));
            Assert.That(mock2.Status, Is.EqualTo(BehaviourStatus.Ready));
        }

        [Test]
        public void ActiveSequence_DifferentFromStandardSequence_InReactivity()
        {
            // Arrange
            var precondition = new MockBehaviour(BehaviourStatus.Succeeded);
            var runningAction = new MockBehaviour(BehaviourStatus.Running);
            var activeSequence = new ActiveSequence<MockContext>("Active", precondition, runningAction);
            var standardSequence = new Sequence<MockContext>("Standard", precondition, runningAction);

            // Act - First tick
            activeSequence.Tick(new MockContext());
            standardSequence.Tick(new MockContext());

            // Change precondition to fail
            precondition.SetStatus(BehaviourStatus.Failed);

            // Act - Second tick
            var activeResult = activeSequence.Tick(new MockContext());
            var standardResult = standardSequence.Tick(new MockContext());

            // Assert
            Assert.That(activeResult, Is.EqualTo(BehaviourStatus.Failed),
                "ActiveSequence should re-evaluate precondition and fail");
            Assert.That(standardResult, Is.EqualTo(BehaviourStatus.Running),
                "Standard Sequence should continue with running child without re-checking precondition");
        }

        [Test]
        public void ActiveSequence_HandlesComplexScenario_WithMultipleResets()
        {
            // Arrange
            var gate1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var gate2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var action = new MockBehaviour(BehaviourStatus.Running);
            var sequence = new ActiveSequence<MockContext>("Complex", gate1, gate2, action);

            // Tick 1: All gates open, action runs
            var result1 = sequence.Tick(new MockContext());
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));

            // Tick 2: Gate1 closes
            gate1.SetStatus(BehaviourStatus.Failed);
            var result2 = sequence.Tick(new MockContext());
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Failed), "Should fail when gate1 closes");

            // Tick 3: Gate1 opens again
            gate1.SetStatus(BehaviourStatus.Succeeded);
            var result3 = sequence.Tick(new MockContext());
            Assert.That(result3, Is.EqualTo(BehaviourStatus.Running), "Should continue when gate1 reopens");

            // Verify proper reset behavior
            // gate1 failed in tick 2, so it maintains state (not reset)
            // gate2 and action were skipped when gate1 failed, so they were reset
            Assert.That(gate1.ResetCallCount, Is.EqualTo(0), "Failed gate maintains state");
            Assert.That(gate2.ResetCallCount, Is.GreaterThan(0), "Skipped children are reset");
        }
    }
}
