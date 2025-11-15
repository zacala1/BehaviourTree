using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using BehaviourTree.Demo.Nodes;
using BehaviourTree.Demo.Systems;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.Systems
{
    public class StaminaSystemTests
    {
        [Test]
        public void Constructor_InitializesSystem()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var staminaSystem = new StaminaSystem(engine);

            // Assert
            Assert.IsNotNull(staminaSystem);
        }

        [Test]
        public void Update_BeforeFrequencyThreshold_DoesNotIncreaseStamina()
        {
            // Arrange
            var engine = new Engine();
            var staminaSystem = new StaminaSystem(engine);
            var entity = engine.NewEntity();
            var staminaComponent = new StaminaComponent(100);
            staminaComponent.ReduceBy(50); // Set to 50
            entity.AddComponent(staminaComponent);

            engine.AddSystem(staminaSystem);

            // Act - Update with less than 50ms (frequency threshold)
            engine.Update(0);
            engine.Update(40);

            // Assert - Stamina should still be 50 (no increase yet)
            var stamina = entity.GetComponent<StaminaComponent>();
            Assert.AreEqual(50, stamina!.Stamina);
        }

        [Test]
        public void Update_AfterFrequencyThreshold_IncreasesStamina()
        {
            // Arrange
            var engine = new Engine();
            var staminaSystem = new StaminaSystem(engine);
            var entity = engine.NewEntity();
            var staminaComponent = new StaminaComponent(100);
            staminaComponent.ReduceBy(50); // Set to 50
            entity.AddComponent(staminaComponent);

            engine.AddSystem(staminaSystem);

            // Act - First update to set initial timestamp
            engine.Update(0);
            // Second update after 50ms threshold
            engine.Update(50);

            // Assert - Stamina should be increased
            var stamina = entity.GetComponent<StaminaComponent>();
            Assert.IsTrue(stamina!.Stamina > 50);
        }

        [Test]
        public void Update_CalculatesDeltaCorrectly()
        {
            // Arrange
            var engine = new Engine();
            var staminaSystem = new StaminaSystem(engine);
            var entity = engine.NewEntity();
            var staminaComponent = new StaminaComponent(100);
            staminaComponent.ReduceBy(50); // Set to 50
            entity.AddComponent(staminaComponent);

            engine.AddSystem(staminaSystem);

            // Act
            engine.Update(0);
            engine.Update(100); // Delta = 100ms, should increase by 2 (100/50)

            // Assert
            var stamina = entity.GetComponent<StaminaComponent>();
            Assert.AreEqual(52, stamina!.Stamina);
        }

        [Test]
        public void Update_DoesNotExceedMaxStamina()
        {
            // Arrange
            var engine = new Engine();
            var staminaSystem = new StaminaSystem(engine);
            var entity = engine.NewEntity();
            var staminaComponent = new StaminaComponent(100);
            staminaComponent.ReduceBy(1); // Set to 99
            entity.AddComponent(staminaComponent);

            engine.AddSystem(staminaSystem);

            // Act
            engine.Update(0);
            engine.Update(200); // Delta = 200ms, would try to increase by 4

            // Assert - Should be capped at max (100)
            var stamina = entity.GetComponent<StaminaComponent>();
            Assert.AreEqual(100, stamina!.Stamina);
        }

        [Test]
        public void Update_MultipleEntities_IncreasesAllStamina()
        {
            // Arrange
            var engine = new Engine();
            var staminaSystem = new StaminaSystem(engine);

            var entity1 = engine.NewEntity();
            var stamina1 = new StaminaComponent(100);
            stamina1.ReduceBy(50);
            entity1.AddComponent(stamina1);

            var entity2 = engine.NewEntity();
            var stamina2 = new StaminaComponent(100);
            stamina2.ReduceBy(50);
            entity2.AddComponent(stamina2);

            var entity3 = engine.NewEntity();
            var stamina3 = new StaminaComponent(100);
            stamina3.ReduceBy(50);
            entity3.AddComponent(stamina3);

            engine.AddSystem(staminaSystem);

            // Act
            engine.Update(0);
            engine.Update(100); // Delta = 100ms

            // Assert
            Assert.AreEqual(52, entity1.GetComponent<StaminaComponent>()!.Stamina);
            Assert.AreEqual(52, entity2.GetComponent<StaminaComponent>()!.Stamina);
            Assert.AreEqual(52, entity3.GetComponent<StaminaComponent>()!.Stamina);
        }

        [Test]
        public void Update_WithNoStaminaEntities_DoesNotThrow()
        {
            // Arrange
            var engine = new Engine();
            var staminaSystem = new StaminaSystem(engine);
            engine.AddSystem(staminaSystem);

            // Act & Assert - Should not throw
            engine.Update(0);
            engine.Update(50);
        }
    }
}
