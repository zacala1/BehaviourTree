using System;
using System.Collections.Generic;

namespace BehaviourTree.Blackboard
{
    /// <summary>
    /// Default implementation of IBlackboard.
    /// Supports hierarchical scoping via optional parent blackboard.
    /// Child reads fall through to parent; writes stay local.
    /// </summary>
    public sealed class Blackboard : IBlackboard
    {
        private readonly Dictionary<string, object?> _data = new Dictionary<string, object?>(16);
        private readonly Dictionary<string, List<Action<string, object?, object?>>> _observers =
            new Dictionary<string, List<Action<string, object?, object?>>>(8);
        private readonly IBlackboard? _parent;

        /// <summary>
        /// Creates a standalone blackboard.
        /// </summary>
        public Blackboard()
        {
        }

        /// <summary>
        /// Creates a child blackboard that falls through reads to the parent.
        /// Writes are always local. Useful for subtree data isolation.
        /// </summary>
        /// <param name="parent">Parent blackboard for read fallthrough</param>
        public Blackboard(IBlackboard parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        /// <summary>
        /// The parent blackboard, if any.
        /// </summary>
        public IBlackboard? Parent => _parent;

        public void Set<T>(string key, T value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            object? oldValue = null;
            _data.TryGetValue(key, out oldValue);

            _data[key] = value;

            NotifyObservers(key, oldValue, value);
        }

        public T Get<T>(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (_data.TryGetValue(key, out var value))
            {
                return (T)value!;
            }

            if (_parent != null)
            {
                return _parent.Get<T>(key);
            }

            throw new KeyNotFoundException($"Blackboard key '{key}' not found");
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (_data.TryGetValue(key, out var obj))
            {
                value = (T)obj!;
                return true;
            }

            if (_parent != null)
            {
                return _parent.TryGet(key, out value);
            }

            value = default!;
            return false;
        }

        public bool HasKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _data.ContainsKey(key) || (_parent?.HasKey(key) ?? false);
        }

        public bool Remove(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (_data.TryGetValue(key, out var oldValue))
            {
                _data.Remove(key);
                NotifyObservers(key, oldValue, null);
                return true;
            }

            return false;
        }

        public IEnumerable<string> Keys => _data.Keys;

        public IDisposable Observe(string key, Action<string, object?, object?> callback)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            if (!_observers.TryGetValue(key, out var list))
            {
                list = new List<Action<string, object?, object?>>(4);
                _observers[key] = list;
            }

            list.Add(callback);

            return new ObserverHandle(this, key, callback);
        }

        public void Clear()
        {
            _data.Clear();
        }

        private void NotifyObservers(string key, object? oldValue, object? newValue)
        {
            if (_observers.TryGetValue(key, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Invoke(key, oldValue, newValue);
                }
            }
        }

        private sealed class ObserverHandle : IDisposable
        {
            private Blackboard? _blackboard;
            private readonly string _key;
            private readonly Action<string, object?, object?> _callback;

            public ObserverHandle(Blackboard blackboard, string key, Action<string, object?, object?> callback)
            {
                _blackboard = blackboard;
                _key = key;
                _callback = callback;
            }

            public void Dispose()
            {
                if (_blackboard != null && _blackboard._observers.TryGetValue(_key, out var list))
                {
                    list.Remove(_callback);
                }
                _blackboard = null;
            }
        }
    }
}
