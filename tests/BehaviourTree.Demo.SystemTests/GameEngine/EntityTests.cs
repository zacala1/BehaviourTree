using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using System.Linq;
using Xunit;

namespace BehaviourTree.Demo.SystemTests.GameEngine
{
    public class EntityTests
    {
        [Fact]
        public void Constructor_SetsId()
        {
            // Arrange & Act
            var entity = new Entity(42);

            // Assert
            Assert.Equal(42, entity.Id);
        }

        [Fact]
        public void AddComponent_AddsComponentToEntity()
        {
            // Arrange
            var entity = new Entity(1);
            var healthComponent = new HealthComponent(100);

            // Act
            entity.AddComponent(healthComponent);

            // Assert
            Assert.True(entity.HasComponent<HealthComponent>());
        }

        [Fact]
        public void AddComponent_ReturnsEntity_ForChaining()
        {
            // Arrange
            var entity = new Entity(1);

            // Act
            var result = entity.AddComponent(new HealthComponent(100));

            // Assert
            Assert.Same(entity, result);
        }

        [Fact]
        public void AddComponent_MultipleComponents_AllAdded()
        {
            // Arrange
            var entity = new Entity(1);

            // Act
            entity.AddComponent(new HealthComponent(100));
            entity.AddComponent(new StaminaComponent(50));
            entity.AddComponent(new InventoryComponent());

            // Assert
            Assert.True(entity.HasComponent<HealthComponent>());
            Assert.True(entity.HasComponent<StaminaComponent>());
            Assert.True(entity.HasComponent<InventoryComponent>());
        }

        [Fact]
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
            Assert.Equal(200, component.MaxHealth);
        }

        [Fact]
        public void GetComponent_ReturnsCorrectComponent()
        {
            // Arrange
            var entity = new Entity(1);
            var healthComponent = new HealthComponent(100);
            entity.AddComponent(healthComponent);

            // Act
            var retrieved = entity.GetComponent<HealthComponent>();

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(100, retrieved.MaxHealth);
        }

        [Fact]
        public void GetComponent_NonExistentComponent_ReturnsNull()
        {
            // Arrange
            var entity = new Entity(1);

            // Act
            var component = entity.GetComponent<HealthComponent>();

            // Assert
            Assert.Null(component);
        }

        [Fact]
        public void HasComponent_ReturnsTrueWhenComponentExists()
        {
            // Arrange
            var entity = new Entity(1);
            entity.AddComponent(new HealthComponent(100));

            // Act & Assert
            Assert.True(entity.HasComponent<HealthComponent>());
        }

        [Fact]
        public void HasComponent_ReturnsFalseWhenComponentDoesNotExist()
        {
            // Arrange
            var entity = new Entity(1);

            // Act & Assert
            Assert.False(entity.HasComponent<HealthComponent>());
        }

        [Fact]
        public void RemoveComponent_RemovesComponent()
        {
            // Arrange
            var entity = new Entity(1);
            entity.AddComponent(new HealthComponent(100));

            // Act
            entity.RemoveComponent<HealthComponent>();

            // Assert
            Assert.False(entity.HasComponent<HealthComponent>());
        }

        [Fact]
        public void RemoveComponent_ReturnsEntity_ForChaining()
        {
            // Arrange
            var entity = new Entity(1);
            entity.AddComponent(new HealthComponent(100));

            // Act
            var result = entity.RemoveComponent<HealthComponent>();

            // Assert
            Assert.Same(entity, result);
        }

        [Fact]
        public void RemoveComponent_NonExistentComponent_DoesNotThrow()
        {
            // Arrange
            var entity = new Entity(1);

            // Act & Assert (should not throw)
            entity.RemoveComponent<HealthComponent>();
        }

        [Fact]
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
            Assert.Equal(3, components.Count);
        }

        [Fact]
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
            Assert.NotNull(addedComponent);
            Assert.IsType<HealthComponent>(addedComponent);
        }

        [Fact]
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
            Assert.NotNull(removedComponent);
            Assert.IsType<HealthComponent>(removedComponent);
        }

        [Fact]
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
            Assert.True(entity.HasComponent<HealthComponent>());
            Assert.True(entity.HasComponent<StaminaComponent>());
            Assert.True(entity.HasComponent<InventoryComponent>());
        }
    }
}
