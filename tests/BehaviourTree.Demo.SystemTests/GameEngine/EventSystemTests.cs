using BehaviourTree.Demo.Events;
using BehaviourTree.Demo.GameEngine;
using Xunit;

namespace BehaviourTree.Demo.SystemTests.GameEngine
{
    public class EventSystemTests
    {
        [Fact]
        public void PublishEvent_NotifiesSubscribedListener()
        {
            // Arrange
            var engine = new Engine();
            var listener = new TestEventListener();
            engine.SubscribeToEvent(listener);

            // Act
            engine.PublishEvent(new HealthReachedZero(42));

            // Assert
            Assert.True(listener.EventReceived);
            Assert.Equal(42, listener.LastEntityId);
        }

        [Fact]
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
            Assert.True(listener1.EventReceived);
            Assert.True(listener2.EventReceived);
        }

        [Fact]
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
            Assert.False(listener.EventReceived);
        }

        [Fact]
        public void PublishEvent_EntityAdded_AutomaticallyPublished()
        {
            // Arrange
            var engine = new Engine();
            var listener = new EntityAddedListener();
            engine.SubscribeToEvent(listener);

            // Act
            var entity = engine.NewEntity();

            // Assert
            Assert.True(listener.EventReceived);
            Assert.Equal(entity.Id, listener.LastEntityId);
        }

        [Fact]
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
            Assert.True(listener.EventReceived);
            Assert.Equal(entity.Id, listener.LastEntityId);
        }

        [Fact]
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
            Assert.True(healthListener.EventReceived);
            Assert.False(entityAddedListener.EventReceived);
        }

        // Test helper classes
        private class TestEventListener : IEventListener<HealthReachedZero>
        {
            public bool EventReceived { get; private set; }
            public int LastEntityId { get; private set; }

            public void OnEvent(HealthReachedZero @event)
            {
                EventReceived = true;
                LastEntityId = @event.EntityId;
            }
        }

        private class EntityAddedListener : IEventListener<EntityAdded>
        {
            public bool EventReceived { get; private set; }
            public int LastEntityId { get; private set; }

            public void OnEvent(EntityAdded @event)
            {
                EventReceived = true;
                LastEntityId = @event.EntityId;
            }
        }

        private class EntityRemovedListener : IEventListener<EntityRemoved>
        {
            public bool EventReceived { get; private set; }
            public int LastEntityId { get; private set; }

            public void OnEvent(EntityRemoved @event)
            {
                EventReceived = true;
                LastEntityId = @event.EntityId;
            }
        }
    }
}
