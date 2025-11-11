using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using Xunit;

namespace BehaviourTree.Demo.ComponentTests.GameEngine
{
    public class EngineTests
    {
        [Fact]
        public void NewEntity_CreatesEntity()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var entity = engine.NewEntity();

            // Assert
            Assert.NotNull(entity);
            Assert.True(entity.Id > 0);
        }

        [Fact]
        public void NewEntity_CreatesMultipleEntitiesWithUniqueIds()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var entity1 = engine.NewEntity();
            var entity2 = engine.NewEntity();
            var entity3 = engine.NewEntity();

            // Assert
            Assert.NotEqual(entity1.Id, entity2.Id);
            Assert.NotEqual(entity1.Id, entity3.Id);
            Assert.NotEqual(entity2.Id, entity3.Id);
        }

        [Fact]
        public void GetEntityById_ReturnsCorrectEntity()
        {
            // Arrange
            var engine = new Engine();
            var entity = engine.NewEntity();

            // Act
            var retrievedEntity = engine.GetEntityById(entity.Id);

            // Assert
            Assert.NotNull(retrievedEntity);
            Assert.Equal(entity.Id, retrievedEntity.Id);
        }

        [Fact]
        public void GetEntityById_NonExistentId_ReturnsNull()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var entity = engine.GetEntityById(999);

            // Assert
            Assert.Null(entity);
        }

        [Fact]
        public void RemoveEntity_RemovesEntityFromEngine()
        {
            // Arrange
            var engine = new Engine();
            var entity = engine.NewEntity();

            // Act
            engine.RemoveEntity(entity.Id);
            var retrievedEntity = engine.GetEntityById(entity.Id);

            // Assert
            Assert.Null(retrievedEntity);
        }

        [Fact]
        public void Update_CallsSystemUpdate()
        {
            // Arrange
            var engine = new Engine();
            var testSystem = new TestSystem();
            engine.AddSystem(testSystem);

            // Act
            engine.Update(100);

            // Assert
            Assert.True(testSystem.UpdateCalled);
            Assert.Equal(100, testSystem.LastEllapsedMilliseconds);
        }

        [Fact]
        public void AddSystem_MultipleSystems_AllGetUpdated()
        {
            // Arrange
            var engine = new Engine();
            var testSystem1 = new TestSystem();
            var testSystem2 = new TestSystem();
            engine.AddSystem(testSystem1);
            engine.AddSystem(testSystem2);

            // Act
            engine.Update(50);

            // Assert
            Assert.True(testSystem1.UpdateCalled);
            Assert.True(testSystem2.UpdateCalled);
        }

        [Fact]
        public void RemoveSystem_SystemNoLongerUpdated()
        {
            // Arrange
            var engine = new Engine();
            var testSystem = new TestSystem();
            engine.AddSystem(testSystem);
            engine.RemoveSystem(testSystem);

            // Act
            engine.Update(100);

            // Assert
            Assert.False(testSystem.UpdateCalled);
        }

        // Test helper system
        private class TestSystem : ISystem
        {
            public bool UpdateCalled { get; private set; }
            public long LastEllapsedMilliseconds { get; private set; }

            public void Update(long ellapsedMilliseconds)
            {
                UpdateCalled = true;
                LastEllapsedMilliseconds = ellapsedMilliseconds;
            }
        }
    }
}
