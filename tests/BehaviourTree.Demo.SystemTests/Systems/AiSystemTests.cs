using BehaviourTree.Demo.Ai.BT;
using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using BehaviourTree.Demo.Nodes;
using BehaviourTree.Demo.Systems;
using Xunit;

namespace BehaviourTree.Demo.SystemTests.Systems
{
    public class AiSystemTests
    {
        [Fact]
        public void Constructor_InitializesSystem()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var aiSystem = new AiSystem(engine);

            // Assert
            Assert.NotNull(aiSystem);
        }

        [Fact]
        public void Update_ExecutesBehaviourTreeForEntity()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);
            var entity = engine.NewEntity();

            var testBehaviour = new TestBehaviour();
            entity.AddComponent(new BTBehaviourComponent(testBehaviour));

            // Act
            engine.AddSystem(aiSystem);
            engine.Update(100);

            // Assert
            Assert.True(testBehaviour.WasExecuted);
            Assert.Equal(100, testBehaviour.LastTimestamp);
        }

        [Fact]
        public void Update_ExecutesBehaviourTreeForMultipleEntities()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);

            var entity1 = engine.NewEntity();
            var testBehaviour1 = new TestBehaviour();
            entity1.AddComponent(new BTBehaviourComponent(testBehaviour1));

            var entity2 = engine.NewEntity();
            var testBehaviour2 = new TestBehaviour();
            entity2.AddComponent(new BTBehaviourComponent(testBehaviour2));

            var entity3 = engine.NewEntity();
            var testBehaviour3 = new TestBehaviour();
            entity3.AddComponent(new BTBehaviourComponent(testBehaviour3));

            // Act
            engine.AddSystem(aiSystem);
            engine.Update(250);

            // Assert
            Assert.True(testBehaviour1.WasExecuted);
            Assert.True(testBehaviour2.WasExecuted);
            Assert.True(testBehaviour3.WasExecuted);
            Assert.Equal(250, testBehaviour1.LastTimestamp);
            Assert.Equal(250, testBehaviour2.LastTimestamp);
            Assert.Equal(250, testBehaviour3.LastTimestamp);
        }

        [Fact]
        public void Update_PassesCorrectContextToBehaviourTree()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);
            var entity = engine.NewEntity();

            var testBehaviour = new TestBehaviour();
            entity.AddComponent(new BTBehaviourComponent(testBehaviour));

            // Act
            engine.AddSystem(aiSystem);
            engine.Update(500);

            // Assert
            Assert.NotNull(testBehaviour.LastContext);
            Assert.Equal(entity, testBehaviour.LastContext.Agent);
            Assert.Equal(engine, testBehaviour.LastContext.Engine);
            Assert.Equal(500, testBehaviour.LastContext.TimeStampInMilliseconds);
        }

        [Fact]
        public void Update_MultipleUpdates_UsesContextPool()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);
            var entity = engine.NewEntity();

            var testBehaviour = new TestBehaviour();
            entity.AddComponent(new BTBehaviourComponent(testBehaviour));
            engine.AddSystem(aiSystem);

            // Act - Multiple updates should reuse contexts from pool
            engine.Update(100);
            var context1 = testBehaviour.LastContext;

            engine.Update(200);
            var context2 = testBehaviour.LastContext;

            engine.Update(300);
            var context3 = testBehaviour.LastContext;

            // Assert - Contexts should be different instances but timestamps should be updated
            Assert.NotNull(context1);
            Assert.NotNull(context2);
            Assert.NotNull(context3);

            // Each update should have correct timestamp
            Assert.Equal(300, context3.TimeStampInMilliseconds);
        }

        [Fact]
        public void Update_WithSuccessfulBehaviour_ReturnsSuccess()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);
            var entity = engine.NewEntity();

            var successBehaviour = new TestBehaviour(BehaviourStatus.Success);
            entity.AddComponent(new BTBehaviourComponent(successBehaviour));

            // Act
            engine.AddSystem(aiSystem);
            engine.Update(100);

            // Assert
            Assert.Equal(BehaviourStatus.Success, successBehaviour.LastStatus);
        }

        [Fact]
        public void Update_WithFailedBehaviour_ReturnsFailure()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);
            var entity = engine.NewEntity();

            var failedBehaviour = new TestBehaviour(BehaviourStatus.Failure);
            entity.AddComponent(new BTBehaviourComponent(failedBehaviour));

            // Act
            engine.AddSystem(aiSystem);
            engine.Update(100);

            // Assert
            Assert.Equal(BehaviourStatus.Failure, failedBehaviour.LastStatus);
        }

        [Fact]
        public void Update_WithRunningBehaviour_ReturnsRunning()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);
            var entity = engine.NewEntity();

            var runningBehaviour = new TestBehaviour(BehaviourStatus.Running);
            entity.AddComponent(new BTBehaviourComponent(runningBehaviour));

            // Act
            engine.AddSystem(aiSystem);
            engine.Update(100);

            // Assert
            Assert.Equal(BehaviourStatus.Running, runningBehaviour.LastStatus);
        }

        [Fact]
        public void Update_WithNoEntities_DoesNotThrow()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);

            // Act
            engine.AddSystem(aiSystem);

            // Assert - Should not throw
            engine.Update(100);
        }

        [Fact]
        public void Update_EntityWithoutBTBehaviourComponent_IsNotProcessed()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);

            // Create entity without BTBehaviourComponent
            var entity = engine.NewEntity();
            entity.AddComponent(new HealthComponent(100));

            // Act & Assert - Should not throw
            engine.AddSystem(aiSystem);
            engine.Update(100);
        }

        [Fact]
        public void Update_LargeNumberOfEntities_ProcessesAll()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);
            var behaviours = new TestBehaviour[100];

            for (int i = 0; i < 100; i++)
            {
                var entity = engine.NewEntity();
                behaviours[i] = new TestBehaviour();
                entity.AddComponent(new BTBehaviourComponent(behaviours[i]));
            }

            // Act
            engine.AddSystem(aiSystem);
            engine.Update(100);

            // Assert - All entities should have been processed
            foreach (var behaviour in behaviours)
            {
                Assert.True(behaviour.WasExecuted);
                Assert.Equal(100, behaviour.LastTimestamp);
            }
        }

        // Test helper class
        private class TestBehaviour : IBehaviour<BtContext>
        {
            private readonly BehaviourStatus _returnStatus;

            public TestBehaviour(BehaviourStatus returnStatus = BehaviourStatus.Success)
            {
                _returnStatus = returnStatus;
            }

            public bool WasExecuted { get; private set; }
            public BtContext? LastContext { get; private set; }
            public long LastTimestamp { get; private set; }
            public BehaviourStatus LastStatus { get; private set; }

            public BehaviourStatus Tick(BtContext context)
            {
                WasExecuted = true;
                LastContext = context;
                LastTimestamp = context.TimeStampInMilliseconds;
                LastStatus = _returnStatus;
                return _returnStatus;
            }
        }
    }
}
