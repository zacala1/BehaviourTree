using BehaviourTree.Demo.Components;
using Xunit;

namespace BehaviourTree.Demo.SystemTests.Components
{
    public class StaminaComponentTests
    {
        [Fact]
        public void Constructor_InitializesStaminaToMaxStamina()
        {
            // Arrange & Act
            var component = new StaminaComponent(100);

            // Assert
            Assert.Equal(100, component.MaxStamina);
            Assert.Equal(100, component.Stamina);
        }

        [Fact]
        public void ReduceBy_DecreasesStamina()
        {
            // Arrange
            var component = new StaminaComponent(100);

            // Act
            component.ReduceBy(30);

            // Assert
            Assert.Equal(70, component.Stamina);
        }

        [Fact]
        public void ReduceBy_DoesNotGoBelowZero()
        {
            // Arrange
            var component = new StaminaComponent(100);

            // Act
            component.ReduceBy(150);

            // Assert
            Assert.Equal(0, component.Stamina);
        }

        [Fact]
        public void IncreaseBy_IncreasesStamina()
        {
            // Arrange
            var component = new StaminaComponent(100);
            component.ReduceBy(50);

            // Act
            component.IncreaseBy(20);

            // Assert
            Assert.Equal(70, component.Stamina);
        }

        [Fact]
        public void IncreaseBy_DoesNotExceedMaxStamina()
        {
            // Arrange
            var component = new StaminaComponent(100);
            component.ReduceBy(30);

            // Act
            component.IncreaseBy(50);

            // Assert
            Assert.Equal(100, component.Stamina);
        }

        [Fact]
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
            Assert.Equal(35, component.Stamina);
        }
    }
}
