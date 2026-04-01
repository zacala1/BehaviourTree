using System;
using System.Collections.Generic;

namespace BehaviourTree.Blackboard
{
    /// <summary>
    /// Key-value data store for sharing data between behavior tree nodes.
    /// Supports change notification for event-driven behavior.
    /// </summary>
    public interface IBlackboard
    {
        /// <summary>
        /// Sets a value in the blackboard.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">Key name</param>
        /// <param name="value">Value to store</param>
        void Set<T>(string key, T value);

        /// <summary>
        /// Gets a value from the blackboard.
        /// </summary>
        /// <typeparam name="T">Expected type of the value</typeparam>
        /// <param name="key">Key name</param>
        /// <returns>The stored value</returns>
        /// <exception cref="KeyNotFoundException">Thrown when key does not exist</exception>
        T Get<T>(string key);

        /// <summary>
        /// Tries to get a value from the blackboard.
        /// </summary>
        /// <typeparam name="T">Expected type of the value</typeparam>
        /// <param name="key">Key name</param>
        /// <param name="value">The stored value if found</param>
        /// <returns>True if the key exists and the value was retrieved</returns>
        bool TryGet<T>(string key, out T value);

        /// <summary>
        /// Checks whether a key exists in the blackboard.
        /// </summary>
        bool HasKey(string key);

        /// <summary>
        /// Removes a key from the blackboard.
        /// </summary>
        /// <returns>True if the key was removed</returns>
        bool Remove(string key);

        /// <summary>
        /// Gets all keys currently in the blackboard.
        /// </summary>
        IEnumerable<string> Keys { get; }

        /// <summary>
        /// Registers a callback that fires when a specific key changes.
        /// </summary>
        /// <param name="key">Key to observe</param>
        /// <param name="callback">Callback with (key, oldValue, newValue)</param>
        /// <returns>Registration handle for unsubscribing</returns>
        IDisposable Observe(string key, Action<string, object?, object?> callback);

        /// <summary>
        /// Removes all entries from the blackboard.
        /// </summary>
        void Clear();
    }
}
