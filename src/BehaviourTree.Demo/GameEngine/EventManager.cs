using System;
using System.Collections.Generic;

namespace BehaviourTree.Demo.GameEngine
{
    public sealed class EventManager : IEventManager
    {
        private const int DefaultEventTypesCapacity = 32;
        private const int DefaultListenersPerEventCapacity = 8;

        private readonly Engine _engine;
        private readonly Dictionary<Type, object> _eventListeners = new Dictionary<Type, object>(DefaultEventTypesCapacity);

        public EventManager(Engine engine)
        {
            _engine = engine;
        }

        public void PublishEvent<TEvent>(TEvent eventData)
        {
            if (!_eventListeners.TryGetValue(typeof(TEvent), out var listenersObj))
            {
                return;
            }

            var listeners = (FastCollection<IEventListener<TEvent>>)listenersObj;

            foreach (var listener in listeners)
            {
                listener.Handle(_engine, eventData);
            }
        }

        public void SubscribeToEvent<TEvent>(IEventListener<TEvent> eventListener)
        {
            var eventType = typeof(TEvent);

            if (!_eventListeners.TryGetValue(eventType, out var listenersObj))
            {
                var listeners = new FastCollection<IEventListener<TEvent>>(DefaultListenersPerEventCapacity);
                _eventListeners[eventType] = listeners;
                listeners.Add(eventListener);
            }
            else
            {
                var listeners = (FastCollection<IEventListener<TEvent>>)listenersObj;
                listeners.Add(eventListener);
            }
        }

        public void UnsubscribeFromEvent<TEvent>(IEventListener<TEvent> eventListener)
        {
            if (_eventListeners.TryGetValue(typeof(TEvent), out var listenersObj))
            {
                var listeners = (FastCollection<IEventListener<TEvent>>)listenersObj;
                listeners.Remove(eventListener);
            }
        }
    }
}
