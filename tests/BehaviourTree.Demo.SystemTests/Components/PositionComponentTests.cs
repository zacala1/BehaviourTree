using BehaviourTree.Demo.Components;
using System.Numerics;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.Components
{
    public class PositionComponentTests
    {
        [Test]
        public void Constructor_InitializesPositionAndPreviousPosition()
        {
            // Arrange
            var initialPosition = new Vector2(100, 200);

            // Act
            var component = new PositionComponent(initialPosition);

            // Assert
            Assert.AreEqual(100, component.Position.X);
            Assert.AreEqual(200, component.Position.Y);
            Assert.AreEqual(100, component.PreviousPosition.X);
            Assert.AreEqual(200, component.PreviousPosition.Y);
        }

        [Test]
        public void GetInterpolatedPosition_AtZeroInterpolation_ReturnsPreviousPosition()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(100, 100));
            component.PreviousPosition = new Vector2(100, 100);
            component.Position = new Vector2(200, 200);

            // Act
            var interpolated = component.GetInterpolatedPosition(0f);

            // Assert
            Assert.AreEqual(100, interpolated.X);
            Assert.AreEqual(100, interpolated.Y);
        }

        [Test]
        public void GetInterpolatedPosition_AtOneInterpolation_ReturnsCurrentPosition()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(100, 100));
            component.PreviousPosition = new Vector2(100, 100);
            component.Position = new Vector2(200, 200);

            // Act
            var interpolated = component.GetInterpolatedPosition(1f);

            // Assert
            Assert.AreEqual(200, interpolated.X);
            Assert.AreEqual(200, interpolated.Y);
        }

        [Test]
        public void GetInterpolatedPosition_AtHalfInterpolation_ReturnsMidpoint()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(100, 100));
            component.PreviousPosition = new Vector2(100, 100);
            component.Position = new Vector2(200, 200);

            // Act
            var interpolated = component.GetInterpolatedPosition(0.5f);

            // Assert
            Assert.AreEqual(150, interpolated.X);
            Assert.AreEqual(150, interpolated.Y);
        }

        [Test]
        public void GetInterpolatedPosition_CustomInterpolation_CalculatesCorrectly()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(0, 0));
            component.PreviousPosition = new Vector2(0, 0);
            component.Position = new Vector2(100, 50);

            // Act
            var interpolated = component.GetInterpolatedPosition(0.25f);

            // Assert
            Assert.AreEqual(25, interpolated.X);
            Assert.AreEqual(12.5f, interpolated.Y);
        }

        [Test]
        public void GetInterpolatedPosition_NegativeMovement_CalculatesCorrectly()
        {
            // Arrange
            var component = new PositionComponent(new Vector2(200, 200));
            component.PreviousPosition = new Vector2(200, 200);
            component.Position = new Vector2(100, 150);

            // Act
            var interpolated = component.GetInterpolatedPosition(0.5f);

            // Assert
            Assert.AreEqual(150, interpolated.X);
            Assert.AreEqual(175, interpolated.Y);
        }
    }
}
