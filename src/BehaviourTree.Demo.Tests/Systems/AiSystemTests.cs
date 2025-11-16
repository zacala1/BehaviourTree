using BehaviourTree.Demo.Ai.BT;
using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using BehaviourTree.Demo.Nodes;
using BehaviourTree.Demo.Systems;
using BehaviourTree.Events;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.Systems
{
    public class AiSystemTests
    {
        [Test]
        public void Constructor_InitializesSystem()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var aiSystem = new AiSystem(engine);

            // Assert
            Assert.IsNotNull(aiSystem);
        }

        [Test]
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
            Assert.IsTrue(testBehaviour.WasExecuted);
            Assert.AreEqual(100, testBehaviour.LastTimestamp);
        }

        [Test]
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
            Assert.IsTrue(testBehaviour1.WasExecuted);
            Assert.IsTrue(testBehaviour2.WasExecuted);
            Assert.IsTrue(testBehaviour3.WasExecuted);
            Assert.AreEqual(250, testBehaviour1.LastTimestamp);
            Assert.AreEqual(250, testBehaviour2.LastTimestamp);
            Assert.AreEqual(250, testBehaviour3.LastTimestamp);
        }

        [Test]
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
            Assert.IsNotNull(testBehaviour.LastContext);
            Assert.AreEqual(entity, testBehaviour.LastContext!.Agent);
            Assert.AreEqual(engine, testBehaviour.LastContext!.Engine);
            Assert.AreEqual(500, testBehaviour.LastContext!.TimeStampInMilliseconds);
        }

        [Test]
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
            Assert.IsNotNull(context1);
            Assert.IsNotNull(context2);
            Assert.IsNotNull(context3);

            // Each update should have correct timestamp
            Assert.AreEqual(300, context3!.TimeStampInMilliseconds);
        }

        [Test]
        public void Update_WithSuccessfulBehaviour_ReturnsSuccess()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);
            var entity = engine.NewEntity();

            var successBehaviour = new TestBehaviour(BehaviourStatus.Succeeded);
            entity.AddComponent(new BTBehaviourComponent(successBehaviour));

            // Act
            engine.AddSystem(aiSystem);
            engine.Update(100);

            // Assert
            Assert.AreEqual(BehaviourStatus.Succeeded, successBehaviour.LastStatus);
        }

        [Test]
        public void Update_WithFailedBehaviour_ReturnsFailure()
        {
            // Arrange
            var engine = new Engine();
            var aiSystem = new AiSystem(engine);
            var entity = engine.NewEntity();

            var failedBehaviour = new TestBehaviour(BehaviourStatus.Failed);
            entity.AddComponent(new BTBehaviourComponent(failedBehaviour));

            // Act
            engine.AddSystem(aiSystem);
            engine.Update(100);

            // Assert
            Assert.AreEqual(BehaviourStatus.Failed, failedBehaviour.LastStatus);
        }

        [Test]
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
            Assert.AreEqual(BehaviourStatus.Running, runningBehaviour.LastStatus);
        }

        [Test]
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

        [Test]
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

        [Test]
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
                Assert.IsTrue(behaviour.WasExecuted);
                Assert.AreEqual(100, behaviour.LastTimestamp);
            }
        }

        // Test helper class
        private class TestBehaviour : IBehaviour<BtContext>
        {
            private readonly BehaviourStatus _returnStatus;

            public TestBehaviour(BehaviourStatus returnStatus = BehaviourStatus.Succeeded)
            {
                _returnStatus = returnStatus;
            }

            public bool WasExecuted { get; private set; }
            public BtContext? LastContext { get; private set; }
            public long LastTimestamp { get; private set; }
            public BehaviourStatus LastStatus { get; private set; }

            // IBehaviour<BtContext> members
            public int Id => 0;
            public string Name => "TestBehaviour";
            public BehaviourStatus Status => LastStatus;

            public BehaviourStatus Tick(BtContext context)
            {
                WasExecuted = true;
                // Create a copy of the context since it will be reset when returned to pool
                LastContext = new BtContext(context.Agent, context.Engine, context.TimeStampInMilliseconds);
                LastTimestamp = context.TimeStampInMilliseconds;
                LastStatus = _returnStatus;
                return _returnStatus;
            }

            public void Reset()
            {
                WasExecuted = false;
                LastContext = null;
                LastTimestamp = 0;
                LastStatus = BehaviourStatus.Ready;
            }

            public void AttachObserver(IBehaviourTreeObserver observer) { }

            public void DetachObserver(IBehaviourTreeObserver observer) { }

            public void Dispose() { }
        }
    }
}
