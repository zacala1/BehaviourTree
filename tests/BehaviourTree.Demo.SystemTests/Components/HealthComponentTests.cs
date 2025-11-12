using BehaviourTree.Demo.Components;
using Xunit;

namespace BehaviourTree.Demo.SystemTests.Components
{
    public class HealthComponentTests
    {
        [Fact]
        public void Constructor_InitializesHealthToMaxHealth()
        {
            // Arrange & Act
            var component = new HealthComponent(100);

            // Assert
            Assert.Equal(100, component.MaxHealth);
            Assert.Equal(100, component.Health);
        }

        [Fact]
        public void ReduceBy_DecreasesHealth()
        {
            // Arrange
            var component = new HealthComponent(100);

            // Act
            component.ReduceBy(30);

            // Assert
            Assert.Equal(70, component.Health);
        }

        [Fact]
        public void ReduceBy_DoesNotGoBelowZero()
        {
            // Arrange
            var component = new HealthComponent(100);

            // Act
            component.ReduceBy(150);

            // Assert
            Assert.Equal(0, component.Health);
        }

        [Fact]
        public void IncreaseBy_IncreasesHealth()
        {
            // Arrange
            var component = new HealthComponent(100);
            component.ReduceBy(50);

            // Act
            component.IncreaseBy(20);

            // Assert
            Assert.Equal(70, component.Health);
        }

        [Fact]
        public void IncreaseBy_DoesNotExceedMaxHealth()
        {
            // Arrange
            var component = new HealthComponent(100);
            component.ReduceBy(30);

            // Act
            component.IncreaseBy(50);

            // Assert
            Assert.Equal(100, component.Health);
        }

        [Fact]
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
            Assert.Equal(35, component.Health);
        }
    }
}
