using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using BehaviourTree.Demo.Nodes;
using BehaviourTree.Demo.Systems;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.Systems
{
    public class LootableSystemTests
    {
        [Test]
        public void Constructor_InitializesSystem()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var lootableSystem = new LootableSystem(engine);

            // Assert
            Assert.IsNotNull(lootableSystem);
        }

        [Test]
        public void Update_WhenQuantityIsZero_RemovesEntity()
        {
            // Arrange
            var engine = new Engine();
            var lootableSystem = new LootableSystem(engine);
            var entity = engine.NewEntity();
            var lootableComponent = new LootableComponent(1);
            lootableComponent.Loot(1); // Reduce quantity to 0
            entity.AddComponent(lootableComponent);

            engine.AddSystem(lootableSystem);

            // Act
            engine.Update(100);

            // Assert
            var retrievedEntity = engine.GetEntityById(entity.Id);
            Assert.IsNull(retrievedEntity);
        }

        [Test]
        public void Update_WhenQuantityIsNotZero_DoesNotRemoveEntity()
        {
            // Arrange
            var engine = new Engine();
            var lootableSystem = new LootableSystem(engine);
            var entity = engine.NewEntity();
            var lootableComponent = new LootableComponent(10);
            lootableComponent.Loot(5); // Reduce quantity to 5
            entity.AddComponent(lootableComponent);

            engine.AddSystem(lootableSystem);

            // Act
            engine.Update(100);

            // Assert
            var retrievedEntity = engine.GetEntityById(entity.Id);
            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual(5, retrievedEntity!.GetComponent<LootableComponent>()!.Quantity);
        }

        [Test]
        public void Update_MultipleEntities_RemovesOnlyEmptyOnes()
        {
            // Arrange
            var engine = new Engine();
            var lootableSystem = new LootableSystem(engine);

            var entity1 = engine.NewEntity();
            var lootable1 = new LootableComponent(1);
            lootable1.LootAll(); // Empty
            entity1.AddComponent(lootable1);

            var entity2 = engine.NewEntity();
            entity2.AddComponent(new LootableComponent(10)); // Not empty

            var entity3 = engine.NewEntity();
            var lootable3 = new LootableComponent(5);
            lootable3.Loot(5); // Empty
            entity3.AddComponent(lootable3);

            engine.AddSystem(lootableSystem);

            // Act
            engine.Update(100);

            // Assert
            Assert.IsNull(engine.GetEntityById(entity1.Id)); // Removed
            Assert.IsNotNull(engine.GetEntityById(entity2.Id)); // Still exists
            Assert.IsNull(engine.GetEntityById(entity3.Id)); // Removed
        }

        [Test]
        public void Update_WithNoLootableEntities_DoesNotThrow()
        {
            // Arrange
            var engine = new Engine();
            var lootableSystem = new LootableSystem(engine);
            engine.AddSystem(lootableSystem);

            // Act & Assert - Should not throw
            engine.Update(100);
        }
    }
}
