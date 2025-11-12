using BehaviourTree.Demo.Components;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.Components
{
    public class StaminaComponentTests
    {
        [Test]
        public void Constructor_InitializesStaminaToMaxStamina()
        {
            // Arrange & Act
            var component = new StaminaComponent(100);

            // Assert
            Assert.AreEqual(100, component.MaxStamina);
            Assert.AreEqual(100, component.Stamina);
        }

        [Test]
        public void ReduceBy_DecreasesStamina()
        {
            // Arrange
            var component = new StaminaComponent(100);

            // Act
            component.ReduceBy(30);

            // Assert
            Assert.AreEqual(70, component.Stamina);
        }

        [Test]
        public void ReduceBy_DoesNotGoBelowZero()
        {
            // Arrange
            var component = new StaminaComponent(100);

            // Act
            component.ReduceBy(150);

            // Assert
            Assert.AreEqual(0, component.Stamina);
        }

        [Test]
        public void IncreaseBy_IncreasesStamina()
        {
            // Arrange
            var component = new StaminaComponent(100);
            component.ReduceBy(50);

            // Act
            component.IncreaseBy(20);

            // Assert
            Assert.AreEqual(70, component.Stamina);
        }

        [Test]
        public void IncreaseBy_DoesNotExceedMaxStamina()
        {
            // Arrange
            var component = new StaminaComponent(100);
            component.ReduceBy(30);

            // Act
            component.IncreaseBy(50);

            // Assert
            Assert.AreEqual(100, component.Stamina);
        }

        [Test]
        public void MultipleOperations_MaintainCorrectStamina()
        {
            // Arrange
            var component = new StaminaComponent(100);

            // Act
            component.ReduceBy(40);
            component.ReduceBy(20);
            component.IncreaseBy(10);
            component.ReduceBy(15);

            // Assert
            Assert.AreEqual(35, component.Stamina);
        }
    }
}
