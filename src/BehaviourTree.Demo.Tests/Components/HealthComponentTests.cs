using BehaviourTree.Demo.Components;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.Components
{
    public class HealthComponentTests
    {
        [Test]
        public void Constructor_InitializesHealthToMaxHealth()
        {
            // Arrange & Act
            var component = new HealthComponent(100);

            // Assert
            Assert.AreEqual(100, component.MaxHealth);
            Assert.AreEqual(100, component.Health);
        }

        [Test]
        public void ReduceBy_DecreasesHealth()
        {
            // Arrange
            var component = new HealthComponent(100);

            // Act
            component.ReduceBy(30);

            // Assert
            Assert.AreEqual(70, component.Health);
        }

        [Test]
        public void ReduceBy_DoesNotGoBelowZero()
        {
            // Arrange
            var component = new HealthComponent(100);

            // Act
            component.ReduceBy(150);

            // Assert
            Assert.AreEqual(0, component.Health);
        }

        [Test]
        public void IncreaseBy_IncreasesHealth()
        {
            // Arrange
            var component = new HealthComponent(100);
            component.ReduceBy(50);

            // Act
            component.IncreaseBy(20);

            // Assert
            Assert.AreEqual(70, component.Health);
        }

        [Test]
        public void IncreaseBy_DoesNotExceedMaxHealth()
        {
            // Arrange
            var component = new HealthComponent(100);
            component.ReduceBy(30);

            // Act
            component.IncreaseBy(50);

            // Assert
            Assert.AreEqual(100, component.Health);
        }

        [Test]
        public void MultipleOperations_MaintainCorrectHealth()
        {
            // Arrange
            var component = new HealthComponent(100);

            // Act
            component.ReduceBy(40);
            component.ReduceBy(20);
            component.IncreaseBy(10);
            component.ReduceBy(15);

            // Assert
            Assert.AreEqual(35, component.Health);
        }
    }
}
