using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using System.Linq;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.GameEngine
{
    public class EntityTests
    {
        [Test]
        public void Constructor_SetsId()
        {
            // Arrange & Act
            var entity = new Entity(42);

            // Assert
            Assert.AreEqual(42, entity.Id);
        }

        [Test]
        public void AddComponent_AddsComponentToEntity()
        {
            // Arrange
            var entity = new Entity(1);
            var healthComponent = new HealthComponent(100);

            // Act
            entity.AddComponent(healthComponent);

            // Assert
            Assert.IsTrue(entity.HasComponent<HealthComponent>());
        }

        [Test]
        public void AddComponent_ReturnsEntity_ForChaining()
        {
            // Arrange
            var entity = new Entity(1);

            // Act
            var result = entity.AddComponent(new HealthComponent(100));

            // Assert
            Assert.AreSame(entity, result);
        }

        [Test]
        public void AddComponent_MultipleComponents_AllAdded()
        {
            // Arrange
            var entity = new Entity(1);

            // Act
            entity.AddComponent(new HealthComponent(100));
            entity.AddComponent(new StaminaComponent(50));
            entity.AddComponent(new InventoryComponent());

            // Assert
            Assert.IsTrue(entity.HasComponent<HealthComponent>());
            Assert.IsTrue(entity.HasComponent<StaminaComponent>());
            Assert.IsTrue(entity.HasComponent<InventoryComponent>());
        }

        [Test]
        public void AddComponent_SameTypeMultipleTimes_ReplacesComponent()
        {
            // Arrange
            var entity = new Entity(1);
            var health1 = new HealthComponent(100);
            var health2 = new HealthComponent(200);

            // Act
            entity.AddComponent(health1);
            entity.AddComponent(health2);

            // Assert
            var component = entity.GetComponent<HealthComponent>();
            Assert.AreEqual(200, component.MaxHealth);
        }

        [Test]
        public void GetComponent_ReturnsCorrectComponent()
        {
            // Arrange
            var entity = new Entity(1);
            var healthComponent = new HealthComponent(100);
            entity.AddComponent(healthComponent);

            // Act
            var retrieved = entity.GetComponent<HealthComponent>();

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(100, retrieved.MaxHealth);
        }

        [Test]
        public void GetComponent_NonExistentComponent_ReturnsNull()
        {
            // Arrange
            var entity = new Entity(1);

            // Act
            var component = entity.GetComponent<HealthComponent>();

            // Assert
            Assert.IsNull(component);
        }

        [Test]
        public void HasComponent_ReturnsTrueWhenComponentExists()
        {
            // Arrange
            var entity = new Entity(1);
            entity.AddComponent(new HealthComponent(100));

            // Act & Assert
            Assert.IsTrue(entity.HasComponent<HealthComponent>());
        }

        [Test]
        public void HasComponent_ReturnsFalseWhenComponentDoesNotExist()
        {
            // Arrange
            var entity = new Entity(1);

            // Act & Assert
            Assert.IsFalse(entity.HasComponent<HealthComponent>());
        }

        [Test]
        public void RemoveComponent_RemovesComponent()
        {
            // Arrange
            var entity = new Entity(1);
            entity.AddComponent(new HealthComponent(100));

            // Act
            entity.RemoveComponent<HealthComponent>();

            // Assert
            Assert.IsFalse(entity.HasComponent<HealthComponent>());
        }

        [Test]
        public void RemoveComponent_ReturnsEntity_ForChaining()
        {
            // Arrange
            var entity = new Entity(1);
            entity.AddComponent(new HealthComponent(100));

            // Act
            var result = entity.RemoveComponent<HealthComponent>();

            // Assert
            Assert.AreSame(entity, result);
        }

        [Test]
        public void RemoveComponent_NonExistentComponent_DoesNotThrow()
        {
            // Arrange
            var entity = new Entity(1);

            // Act & Assert (should not throw)
            entity.RemoveComponent<HealthComponent>();
        }

        [Test]
        public void GetComponents_ReturnsAllComponents()
        {
            // Arrange
            var entity = new Entity(1);
            entity.AddComponent(new HealthComponent(100));
            entity.AddComponent(new StaminaComponent(50));
            entity.AddComponent(new InventoryComponent());

            // Act
            var components = entity.GetComponents().ToList();

            // Assert
            Assert.AreEqual(3, components.Count);
        }

        [Test]
        public void ComponentAdded_EventFired_WhenComponentAdded()
        {
            // Arrange
            var entity = new Entity(1);
            IComponent? addedComponent = null;
            entity.ComponentAdded += (sender, component) => addedComponent = component;

            // Act
            var healthComponent = new HealthComponent(100);
            entity.AddComponent(healthComponent);

            // Assert
            Assert.IsNotNull(addedComponent);
            Assert.IsType<HealthComponent>(addedComponent);
        }

        [Test]
        public void ComponentRemoved_EventFired_WhenComponentRemoved()
        {
            // Arrange
            var entity = new Entity(1);
            IComponent? removedComponent = null;
            entity.ComponentRemoved += (sender, component) => removedComponent = component;
            entity.AddComponent(new HealthComponent(100));

            // Act
            entity.RemoveComponent<HealthComponent>();

            // Assert
            Assert.IsNotNull(removedComponent);
            Assert.IsType<HealthComponent>(removedComponent);
        }

        [Test]
        public void ChainedOperations_WorkCorrectly()
        {
            // Arrange
            var entity = new Entity(1);

            // Act
            entity
                .AddComponent(new HealthComponent(100))
                .AddComponent(new StaminaComponent(50))
                .AddComponent(new InventoryComponent());

            // Assert
            Assert.IsTrue(entity.HasComponent<HealthComponent>());
            Assert.IsTrue(entity.HasComponent<StaminaComponent>());
            Assert.IsTrue(entity.HasComponent<InventoryComponent>());
        }
    }
}
