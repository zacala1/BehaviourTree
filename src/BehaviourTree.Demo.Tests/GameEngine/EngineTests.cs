using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.GameEngine
{
    public class EngineTests
    {
        [Test]
        public void NewEntity_CreatesEntity()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var entity = engine.NewEntity();

            // Assert
            Assert.IsNotNull(entity);
            Assert.IsTrue(entity.Id > 0);
        }

        [Test]
        public void NewEntity_CreatesMultipleEntitiesWithUniqueIds()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var entity1 = engine.NewEntity();
            var entity2 = engine.NewEntity();
            var entity3 = engine.NewEntity();

            // Assert
            Assert.AreNotEqual(entity1.Id, entity2.Id);
            Assert.AreNotEqual(entity1.Id, entity3.Id);
            Assert.AreNotEqual(entity2.Id, entity3.Id);
        }

        [Test]
        public void GetEntityById_ReturnsCorrectEntity()
        {
            // Arrange
            var engine = new Engine();
            var entity = engine.NewEntity();

            // Act
            var retrievedEntity = engine.GetEntityById(entity.Id);

            // Assert
            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual(entity.Id, retrievedEntity.Id);
        }

        [Test]
        public void GetEntityById_NonExistentId_ReturnsNull()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var entity = engine.GetEntityById(999);

            // Assert
            Assert.IsNull(entity);
        }

        [Test]
        public void RemoveEntity_RemovesEntityFromEngine()
        {
            // Arrange
            var engine = new Engine();
            var entity = engine.NewEntity();

            // Act
            engine.RemoveEntity(entity.Id);
            var retrievedEntity = engine.GetEntityById(entity.Id);

            // Assert
            Assert.IsNull(retrievedEntity);
        }

        [Test]
        public void Update_CallsSystemUpdate()
        {
            // Arrange
            var engine = new Engine();
            var testSystem = new TestSystem();
            engine.AddSystem(testSystem);

            // Act
            engine.Update(100);

            // Assert
            Assert.IsTrue(testSystem.UpdateCalled);
            Assert.AreEqual(100, testSystem.LastEllapsedMilliseconds);
        }

        [Test]
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
            Assert.IsTrue(testSystem1.UpdateCalled);
            Assert.IsTrue(testSystem2.UpdateCalled);
        }

        [Test]
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
            Assert.IsFalse(testSystem.UpdateCalled);
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
