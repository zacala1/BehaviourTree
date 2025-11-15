using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.Events;
using BehaviourTree.Demo.GameEngine;
using BehaviourTree.Demo.Nodes;
using BehaviourTree.Demo.Systems;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.Systems
{
    public class HealthSystemTests
    {
        [Test]
        public void Constructor_InitializesSystem()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var healthSystem = new HealthSystem(engine);

            // Assert
            Assert.IsNotNull(healthSystem);
        }

        [Test]
        public void Update_BeforeFrequencyThreshold_DoesNotReduceHealth()
        {
            // Arrange
            var engine = new Engine();
            var healthSystem = new HealthSystem(engine);
            var entity = engine.NewEntity();
            entity.AddComponent(new HealthComponent(100));

            engine.AddSystem(healthSystem);

            // Act - Update with less than 300ms (frequency threshold)
            engine.Update(100);
            engine.Update(200);

            // Assert - Health should still be 100 (no reduction yet)
            var health = entity.GetComponent<HealthComponent>();
            Assert.AreEqual(100, health!.Health);
        }

        [Test]
        public void Update_AfterFrequencyThreshold_ReducesHealth()
        {
            // Arrange
            var engine = new Engine();
            var healthSystem = new HealthSystem(engine);
            var entity = engine.NewEntity();
            entity.AddComponent(new HealthComponent(100));

            engine.AddSystem(healthSystem);

            // Act - First update to set initial timestamp
            engine.Update(0);
            // Second update after 300ms threshold
            engine.Update(300);

            // Assert - Health should be reduced
            var health = entity.GetComponent<HealthComponent>();
            Assert.IsTrue(health!.Health < 100);
        }

        [Test]
        public void Update_CalculatesDeltaCorrectly()
        {
            // Arrange
            var engine = new Engine();
            var healthSystem = new HealthSystem(engine);
            var entity = engine.NewEntity();
            entity.AddComponent(new HealthComponent(100));

            engine.AddSystem(healthSystem);

            // Act
            engine.Update(0);
            engine.Update(600); // Delta = 600ms, should reduce by 2 (600/300)

            // Assert
            var health = entity.GetComponent<HealthComponent>();
            Assert.AreEqual(98, health!.Health);
        }

        [Test]
        public void Update_WhenHealthReachesZero_PublishesEvent()
        {
            // Arrange
            var engine = new Engine();
            var healthSystem = new HealthSystem(engine);
            var entity = engine.NewEntity();
            entity.AddComponent(new HealthComponent(2)); // Low health

            var listener = new HealthReachedZeroListener();
            engine.SubscribeToEvent(listener);
            engine.AddSystem(healthSystem);

            // Act
            engine.Update(0);
            engine.Update(900); // Delta = 900ms, should reduce by 3 (900/300)

            // Assert
            Assert.IsTrue(listener.EventReceived);
            Assert.AreEqual(entity.Id, listener.EntityId);
        }

        [Test]
        public void Update_WhenHealthAlreadyZero_DoesNotPublishEvent()
        {
            // Arrange
            var engine = new Engine();
            var healthSystem = new HealthSystem(engine);
            var entity = engine.NewEntity();
            var healthComponent = new HealthComponent(100);
            healthComponent.ReduceBy(100); // Already at 0
            entity.AddComponent(healthComponent);

            var listener = new HealthReachedZeroListener();
            engine.SubscribeToEvent(listener);
            engine.AddSystem(healthSystem);

            // Act
            engine.Update(0);
            engine.Update(300);

            // Assert
            Assert.IsFalse(listener.EventReceived);
        }

        [Test]
        public void Update_MultipleEntities_ReducesAllHealth()
        {
            // Arrange
            var engine = new Engine();
            var healthSystem = new HealthSystem(engine);

            var entity1 = engine.NewEntity();
            entity1.AddComponent(new HealthComponent(100));

            var entity2 = engine.NewEntity();
            entity2.AddComponent(new HealthComponent(100));

            var entity3 = engine.NewEntity();
            entity3.AddComponent(new HealthComponent(100));

            engine.AddSystem(healthSystem);

            // Act
            engine.Update(0);
            engine.Update(600); // Delta = 600ms

            // Assert
            Assert.AreEqual(98, entity1.GetComponent<HealthComponent>()!.Health);
            Assert.AreEqual(98, entity2.GetComponent<HealthComponent>()!.Health);
            Assert.AreEqual(98, entity3.GetComponent<HealthComponent>()!.Health);
        }

        [Test]
        public void Update_WithNoHealthEntities_DoesNotThrow()
        {
            // Arrange
            var engine = new Engine();
            var healthSystem = new HealthSystem(engine);
            engine.AddSystem(healthSystem);

            // Act & Assert - Should not throw
            engine.Update(0);
            engine.Update(300);
        }

        // Test helper class
        private class HealthReachedZeroListener : IEventListener<HealthReachedZero>
        {
            public bool EventReceived { get; private set; }
            public int EntityId { get; private set; }

            public void Handle(Engine engine, HealthReachedZero @event)
            {
                EventReceived = true;
                EntityId = @event.EntityId;
            }
        }
    }
}
