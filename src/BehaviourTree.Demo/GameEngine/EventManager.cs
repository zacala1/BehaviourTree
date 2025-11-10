using System;
using System.Collections.Generic;

namespace BehaviourTree.Demo.GameEngine
{
    public sealed class EventManager : IEventManager
    {
        private readonly Engine _engine;
        // Use object to store FastCollection<IEventListener<TEvent>> without boxing
        private readonly Dictionary<Type, object> _eventListeners = new Dictionary<Type, object>(32);

        public EventManager(Engine engine)
        {
            _engine = engine;
        }

        public void PublishEvent<TEvent>(TEvent @event)
        {
            if (!_eventListeners.TryGetValue(typeof(TEvent), out var listenersObj))
            {
                return;
            }

            // Cast to strongly-typed collection (no boxing)
            var listeners = (FastCollection<IEventListener<TEvent>>)listenersObj;

            // Use value-type enumerator to avoid allocation
            foreach (var listener in listeners)
            {
                listener.Handle(_engine, @event);
            }
        }

        public void SubscribeToEvent<TEvent>(IEventListener<TEvent> eventListener)
        {
            var eventType = typeof(TEvent);

            if (!_eventListeners.TryGetValue(eventType, out var listenersObj))
            {
                // Create strongly-typed FastCollection
                var listeners = new FastCollection<IEventListener<TEvent>>(8);
                _eventListeners[eventType] = listeners;
                listeners.Add(eventListener);
            }
            else
            {
                // Cast and add (no boxing)
                var listeners = (FastCollection<IEventListener<TEvent>>)listenersObj;
                listeners.Add(eventListener);
            }
        }

        public void UnsubscribeFromEvent<TEvent>(IEventListener<TEvent> eventListener)
        {
            var eventType = typeof(TEvent);

            if (_eventListeners.TryGetValue(eventType, out var listenersObj))
            {
                // Cast and remove (no boxing)
                var listeners = (FastCollection<IEventListener<TEvent>>)listenersObj;
                listeners.Remove(eventListener);
            }
        }
    }
}
