using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using BehaviourTree.Demo.Nodes;
using BehaviourTree.Demo.Systems;
using System.Drawing;
using System.Numerics;
using Xunit;

namespace BehaviourTree.Demo.SystemTests.Systems
{
    public class MoveSystemTests
    {
        [Fact]
        public void Constructor_InitializesSystem()
        {
            // Arrange
            var engine = new Engine();
            var boardSize = new Size(800, 600);

            // Act
            var moveSystem = new MoveSystem(engine, boardSize);

            // Assert
            Assert.NotNull(moveSystem);
        }

        [Fact]
        public void Update_AppliesVelocityToPosition()
        {
            // Arrange
            var engine = new Engine();
            var boardSize = new Size(800, 600);
            var moveSystem = new MoveSystem(engine, boardSize);
            var entity = engine.NewEntity();

            entity.AddComponent(new PositionComponent(new Vector2(100, 100)));
            entity.AddComponent(new MovementComponent { Velocity = new Vector2(10, 5) });

            engine.AddSystem(moveSystem);

            // Act
            engine.Update(100);

            // Assert
            var position = entity.GetComponent<PositionComponent>();
            Assert.Equal(110, position.Position.X);
            Assert.Equal(105, position.Position.Y);
        }

        [Fact]
        public void Update_StoresPreviousPosition()
        {
            // Arrange
            var engine = new Engine();
            var boardSize = new Size(800, 600);
            var moveSystem = new MoveSystem(engine, boardSize);
            var entity = engine.NewEntity();

            entity.AddComponent(new PositionComponent(new Vector2(100, 100)));
            entity.AddComponent(new MovementComponent { Velocity = new Vector2(10, 5) });

            engine.AddSystem(moveSystem);

            // Act
            engine.Update(100);

            // Assert
            var position = entity.GetComponent<PositionComponent>();
            Assert.Equal(100, position.PreviousPosition.X);
            Assert.Equal(100, position.PreviousPosition.Y);
        }

        [Fact]
        public void Update_ClampsToBoardBoundaries_MinimumX()
        {
            // Arrange
            var engine = new Engine();
            var boardSize = new Size(800, 600);
            var moveSystem = new MoveSystem(engine, boardSize);
            var entity = engine.NewEntity();

            entity.AddComponent(new PositionComponent(new Vector2(5, 100)));
            entity.AddComponent(new MovementComponent { Velocity = new Vector2(-10, 0) });

            engine.AddSystem(moveSystem);

            // Act
            engine.Update(100);

            // Assert
            var position = entity.GetComponent<PositionComponent>();
            Assert.Equal(0, position.Position.X); // Clamped to 0
        }

        [Fact]
        public void Update_ClampsToBoardBoundaries_MinimumY()
        {
            // Arrange
            var engine = new Engine();
            var boardSize = new Size(800, 600);
            var moveSystem = new MoveSystem(engine, boardSize);
            var entity = engine.NewEntity();

            entity.AddComponent(new PositionComponent(new Vector2(100, 5)));
            entity.AddComponent(new MovementComponent { Velocity = new Vector2(0, -10) });

            engine.AddSystem(moveSystem);

            // Act
            engine.Update(100);

            // Assert
            var position = entity.GetComponent<PositionComponent>();
            Assert.Equal(0, position.Position.Y); // Clamped to 0
        }

        [Fact]
        public void Update_ClampsToBoardBoundaries_MaximumX()
        {
            // Arrange
            var engine = new Engine();
            var boardSize = new Size(800, 600);
            var moveSystem = new MoveSystem(engine, boardSize);
            var entity = engine.NewEntity();

            entity.AddComponent(new PositionComponent(new Vector2(795, 100)));
            entity.AddComponent(new MovementComponent { Velocity = new Vector2(10, 0) });

            engine.AddSystem(moveSystem);

            // Act
            engine.Update(100);

            // Assert
            var position = entity.GetComponent<PositionComponent>();
            Assert.Equal(800, position.Position.X); // Clamped to board width
        }

        [Fact]
        public void Update_ClampsToBoardBoundaries_MaximumY()
        {
            // Arrange
            var engine = new Engine();
            var boardSize = new Size(800, 600);
            var moveSystem = new MoveSystem(engine, boardSize);
            var entity = engine.NewEntity();

            entity.AddComponent(new PositionComponent(new Vector2(100, 595)));
            entity.AddComponent(new MovementComponent { Velocity = new Vector2(0, 10) });

            engine.AddSystem(moveSystem);

            // Act
            engine.Update(100);

            // Assert
            var position = entity.GetComponent<PositionComponent>();
            Assert.Equal(600, position.Position.Y); // Clamped to board height
        }

        [Fact]
        public void Update_MultipleEntities_MovesAll()
        {
            // Arrange
            var engine = new Engine();
            var boardSize = new Size(800, 600);
            var moveSystem = new MoveSystem(engine, boardSize);

            var entity1 = engine.NewEntity();
            entity1.AddComponent(new PositionComponent(new Vector2(100, 100)));
            entity1.AddComponent(new MovementComponent { Velocity = new Vector2(5, 5) });

            var entity2 = engine.NewEntity();
            entity2.AddComponent(new PositionComponent(new Vector2(200, 200)));
            entity2.AddComponent(new MovementComponent { Velocity = new Vector2(-5, -5) });

            var entity3 = engine.NewEntity();
            entity3.AddComponent(new PositionComponent(new Vector2(300, 300)));
            entity3.AddComponent(new MovementComponent { Velocity = new Vector2(10, -10) });

            engine.AddSystem(moveSystem);

            // Act
            engine.Update(100);

            // Assert
            var pos1 = entity1.GetComponent<PositionComponent>();
            Assert.Equal(105, pos1.Position.X);
            Assert.Equal(105, pos1.Position.Y);

            var pos2 = entity2.GetComponent<PositionComponent>();
            Assert.Equal(195, pos2.Position.X);
            Assert.Equal(195, pos2.Position.Y);

            var pos3 = entity3.GetComponent<PositionComponent>();
            Assert.Equal(310, pos3.Position.X);
            Assert.Equal(290, pos3.Position.Y);
        }

        [Fact]
        public void Update_WithNoMoveEntities_DoesNotThrow()
        {
            // Arrange
            var engine = new Engine();
            var boardSize = new Size(800, 600);
            var moveSystem = new MoveSystem(engine, boardSize);
            engine.AddSystem(moveSystem);

            // Act & Assert - Should not throw
            engine.Update(100);
        }
    }
}
