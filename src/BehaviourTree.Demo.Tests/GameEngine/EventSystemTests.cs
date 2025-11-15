using BehaviourTree.Demo.Events;
using BehaviourTree.Demo.GameEngine;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.GameEngine
{
    public class EventSystemTests
    {
        [Test]
        public void PublishEvent_NotifiesSubscribedListener()
        {
            // Arrange
            var engine = new Engine();
            var listener = new TestEventListener();
            engine.SubscribeToEvent(listener);

            // Act
            engine.PublishEvent(new HealthReachedZero(42));

            // Assert
            Assert.IsTrue(listener.EventReceived);
            Assert.AreEqual(42, listener.LastEntityId);
        }

        [Test]
        public void PublishEvent_MultipleListeners_AllNotified()
        {
            // Arrange
            var engine = new Engine();
            var listener1 = new TestEventListener();
            var listener2 = new TestEventListener();
            engine.SubscribeToEvent(listener1);
            engine.SubscribeToEvent(listener2);

            // Act
            engine.PublishEvent(new HealthReachedZero(42));

            // Assert
            Assert.IsTrue(listener1.EventReceived);
            Assert.IsTrue(listener2.EventReceived);
        }

        [Test]
        public void UnsubscribeFromEvent_ListenerNoLongerNotified()
        {
            // Arrange
            var engine = new Engine();
            var listener = new TestEventListener();
            engine.SubscribeToEvent(listener);
            engine.UnsubscribeFromEvent(listener);

            // Act
            engine.PublishEvent(new HealthReachedZero(42));

            // Assert
            Assert.IsFalse(listener.EventReceived);
        }

        [Test]
        public void PublishEvent_EntityAdded_AutomaticallyPublished()
        {
            // Arrange
            var engine = new Engine();
            var listener = new EntityAddedListener();
            engine.SubscribeToEvent(listener);

            // Act
            var entity = engine.NewEntity();

            // Assert
            Assert.IsTrue(listener.EventReceived);
            Assert.AreEqual(entity.Id, listener.LastEntityId);
        }

        [Test]
        public void PublishEvent_EntityRemoved_AutomaticallyPublished()
        {
            // Arrange
            var engine = new Engine();
            var listener = new EntityRemovedListener();
            engine.SubscribeToEvent(listener);
            var entity = engine.NewEntity();

            // Act
            engine.RemoveEntity(entity.Id);

            // Assert
            Assert.IsTrue(listener.EventReceived);
            Assert.AreEqual(entity.Id, listener.LastEntityId);
        }

        [Test]
        public void PublishEvent_DifferentEventTypes_OnlyRelevantListenersNotified()
        {
            // Arrange
            var engine = new Engine();
            var healthListener = new TestEventListener();
            var entityAddedListener = new EntityAddedListener();

            engine.SubscribeToEvent(healthListener);
            engine.SubscribeToEvent(entityAddedListener);

            // Act
            engine.PublishEvent(new HealthReachedZero(42));

            // Assert
            Assert.IsTrue(healthListener.EventReceived);
            Assert.IsFalse(entityAddedListener.EventReceived);
        }

        // Test helper classes
        private class TestEventListener : IEventListener<HealthReachedZero>
        {
            public bool EventReceived { get; private set; }
            public int LastEntityId { get; private set; }

            public void Handle(Engine engine, HealthReachedZero @event)
            {
                EventReceived = true;
                LastEntityId = @event.EntityId;
            }
        }

        private class EntityAddedListener : IEventListener<EntityAdded>
        {
            public bool EventReceived { get; private set; }
            public int LastEntityId { get; private set; }

            public void Handle(Engine engine, EntityAdded @event)
            {
                EventReceived = true;
                LastEntityId = @event.EntityId;
            }
        }

        private class EntityRemovedListener : IEventListener<EntityRemoved>
        {
            public bool EventReceived { get; private set; }
            public int LastEntityId { get; private set; }

            public void Handle(Engine engine, EntityRemoved @event)
            {
                EventReceived = true;
                LastEntityId = @event.EntityId;
            }
        }
    }
}
