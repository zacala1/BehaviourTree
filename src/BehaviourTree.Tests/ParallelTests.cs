using BehaviourTree.Composites;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    /// <summary>
    /// Comprehensive tests for Parallel composite node with flexible policies.
    /// </summary>
    [TestFixture]
    internal sealed class ParallelTests
    {
        [Test]
        public void Parallel_RequireAll_SucceedsWhenAllChildrenSucceed()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var parallel = new Parallel<MockContext>("Test", ParallelPolicy.RequireAll, mock1, mock2, mock3);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock3.UpdateCallCount, Is.EqualTo(1), "All children should execute");
        }

        [Test]
        public void Parallel_RequireAll_FailsWhenAnyChildFails()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Failed);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var parallel = new Parallel<MockContext>("Test", ParallelPolicy.RequireAll, mock1, mock2, mock3);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock3.UpdateCallCount, Is.EqualTo(1), "Should execute all even if one fails");
        }

        [Test]
        public void Parallel_RequireOne_SucceedsWhenOneChildSucceeds()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Failed);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock3 = new MockBehaviour(BehaviourStatus.Failed);
            var parallel = new Parallel<MockContext>("Test", ParallelPolicy.RequireOne, mock1, mock2, mock3);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void Parallel_RequireOne_FailsWhenAllChildrenFail()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Failed);
            var mock2 = new MockBehaviour(BehaviourStatus.Failed);
            var mock3 = new MockBehaviour(BehaviourStatus.Failed);
            var parallel = new Parallel<MockContext>("Test", ParallelPolicy.RequireOne, mock1, mock2, mock3);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
        }

        [Test]
        public void Parallel_RequireN_SucceedsWhenNChildrenSucceed()
        {
            // Arrange - Require 2 out of 4 to succeed
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Failed);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock4 = new MockBehaviour(BehaviourStatus.Failed);
            var parallel = new Parallel<MockContext>("Test", 2, mock1, mock2, mock3, mock4);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded), "Should succeed with exactly 2 successes");
        }

        [Test]
        public void Parallel_RequireN_FailsWhenLessThanNSucceed()
        {
            // Arrange - Require 3 out of 4 to succeed, but only 2 succeed
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Failed);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock4 = new MockBehaviour(BehaviourStatus.Failed);
            var parallel = new Parallel<MockContext>("Test", 3, mock1, mock2, mock3, mock4);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed), "Should fail with only 2 successes when 3 required");
        }

        [Test]
        public void Parallel_ReturnsRunning_WhenChildrenAreRunning()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Running);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var parallel = new Parallel<MockContext>("Test", ParallelPolicy.RequireAll, mock1, mock2);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Running));
        }

        [Test]
        public void Parallel_ContinuesRunning_UntilPolicyMet()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Running);
            var mock2 = new MockBehaviour(BehaviourStatus.Running);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var parallel = new Parallel<MockContext>("Test", ParallelPolicy.RequireOne, mock1, mock2, mock3);

            // Act - First tick, one succeeds immediately
            var result1 = parallel.Tick(new MockContext());

            // Assert - Should succeed immediately with RequireOne policy
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void Parallel_ExecutesAllChildren_InParallel()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var parallel = new Parallel<MockContext>("Test", ParallelPolicy.RequireAll, mock1, mock2, mock3);

            // Act
            parallel.Tick(new MockContext());

            // Assert - All children should be ticked
            Assert.That(mock1.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock2.UpdateCallCount, Is.EqualTo(1));
            Assert.That(mock3.UpdateCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Parallel_ResetsProperly()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Failed);
            var parallel = new Parallel<MockContext>("Test", ParallelPolicy.RequireAll, mock1, mock2);

            // Act
            parallel.Tick(new MockContext());
            parallel.Reset();

            // Assert
            Assert.That(parallel.Status, Is.EqualTo(BehaviourStatus.Ready));
            Assert.That(mock1.Status, Is.EqualTo(BehaviourStatus.Ready));
            Assert.That(mock2.Status, Is.EqualTo(BehaviourStatus.Ready));
        }

        [Test]
        public void Parallel_HandlesEmptyChildren_RequireAll()
        {
            // Arrange
            var parallel = new Parallel<MockContext>("Empty", ParallelPolicy.RequireAll);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded), "Empty parallel with RequireAll should succeed");
        }

        [Test]
        public void Parallel_HandlesEmptyChildren_RequireOne()
        {
            // Arrange
            var parallel = new Parallel<MockContext>("Empty", ParallelPolicy.RequireOne);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed), "Empty parallel with RequireOne should fail");
        }

        [Test]
        public void Parallel_RequireN_ThrowsException_WhenNGreaterThanChildCount()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);

            // Act & Assert
            Assert.Throws<System.ArgumentException>(() =>
            {
                var parallel = new Parallel<MockContext>("Test", 5, mock1, mock2); // Require 5 but only 2 children
            });
        }

        [Test]
        public void Parallel_RequireN_WithZero_BehavesLikeRequireOne()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Failed);
            var mock2 = new MockBehaviour(BehaviourStatus.Succeeded);

            // Creating with 0 should default to RequireOne behavior
            var parallel = new Parallel<MockContext>("Test", 0, mock1, mock2);

            // Act
            var result = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void Parallel_MixedResults_WithRequireAll()
        {
            // Arrange
            var mock1 = new MockBehaviour(BehaviourStatus.Succeeded);
            var mock2 = new MockBehaviour(BehaviourStatus.Running);
            var mock3 = new MockBehaviour(BehaviourStatus.Succeeded);
            var parallel = new Parallel<MockContext>("Test", ParallelPolicy.RequireAll, mock1, mock2, mock3);

            // Act - First tick
            var result1 = parallel.Tick(new MockContext());
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));

            // Second tick - middle child completes
            mock2.SetStatus(BehaviourStatus.Succeeded);
            var result2 = parallel.Tick(new MockContext());

            // Assert
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void Parallel_PropertyReflectsSuccessRequired()
        {
            // Arrange & Act
            var parallel1 = new Parallel<MockContext>("Test1", ParallelPolicy.RequireAll,
                new MockBehaviour(BehaviourStatus.Succeeded),
                new MockBehaviour(BehaviourStatus.Succeeded),
                new MockBehaviour(BehaviourStatus.Succeeded));

            var parallel2 = new Parallel<MockContext>("Test2", 2,
                new MockBehaviour(BehaviourStatus.Succeeded),
                new MockBehaviour(BehaviourStatus.Succeeded),
                new MockBehaviour(BehaviourStatus.Succeeded));

            // Assert
            Assert.That(parallel1.SuccessRequired, Is.EqualTo(3), "RequireAll should equal child count");
            Assert.That(parallel2.SuccessRequired, Is.EqualTo(2), "Should match specified success count");
        }
    }
}
