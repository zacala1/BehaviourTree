using BehaviourTree.Demo.Components;
using System.Numerics;
using Xunit;

namespace BehaviourTree.Demo.SystemTests.Components
{
    public class PositionComponentTests
    {
        [Fact]
        public void Constructor_InitializesPositionAndPreviousPosition()
        {
            // Arrange
            var initialPosition = new Vector2(100, 200);

            // Act
            var component = new PositionComponent(initialPosition);

            // Assert
            Assert.Equal(100, component.Position.X);
            Assert.Equal(200, component.Position.Y);
            Assert.Equal(100, component.PreviousPosition.X);
            Assert.Equal(200, component.PreviousPosition.Y);
        }

        [Fact]
        public void GetInterpolatedPosition_AtZeroInterpolation_ReturnsPreviousPosition()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(100, 100));
            component.PreviousPosition = new Vector2(100, 100);
            component.Position = new Vector2(200, 200);

            // Act
            var interpolated = component.GetInterpolatedPosition(0f);

            // Assert
            Assert.Equal(100, interpolated.X);
            Assert.Equal(100, interpolated.Y);
        }

        [Fact]
        public void GetInterpolatedPosition_AtOneInterpolation_ReturnsCurrentPosition()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(100, 100));
            component.PreviousPosition = new Vector2(100, 100);
            component.Position = new Vector2(200, 200);

            // Act
            var interpolated = component.GetInterpolatedPosition(1f);

            // Assert
            Assert.Equal(200, interpolated.X);
            Assert.Equal(200, interpolated.Y);
        }

        [Fact]
        public void GetInterpolatedPosition_AtHalfInterpolation_ReturnsMidpoint()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(100, 100));
            component.PreviousPosition = new Vector2(100, 100);
            component.Position = new Vector2(200, 200);

            // Act
            var interpolated = component.GetInterpolatedPosition(0.5f);

            // Assert
            Assert.Equal(150, interpolated.X);
            Assert.Equal(150, interpolated.Y);
        }

        [Fact]
        public void GetInterpolatedPosition_CustomInterpolation_CalculatesCorrectly()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(0, 0));
            component.PreviousPosition = new Vector2(0, 0);
            component.Position = new Vector2(100, 50);

            // Act
            var interpolated = component.GetInterpolatedPosition(0.25f);

            // Assert
            Assert.Equal(25, interpolated.X);
            Assert.Equal(12.5f, interpolated.Y);
        }

        [Fact]
        public void GetInterpolatedPosition_NegativeMovement_CalculatesCorrectly()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(200, 200));
            component.PreviousPosition = new Vector2(200, 200);
            component.Position = new Vector2(100, 150);

            // Act
            var interpolated = component.GetInterpolatedPosition(0.5f);

            // Assert
            Assert.Equal(150, interpolated.X);
            Assert.Equal(175, interpolated.Y);
        }
    }
}
