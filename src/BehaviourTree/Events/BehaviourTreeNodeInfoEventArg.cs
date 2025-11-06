using System;

namespace BehaviourTree.Events
{
    /// <summary>
    /// Event arguments for behavior tree node lifecycle events.
    /// Provides basic information about the node and event type.
    /// </summary>
    public class BehaviourTreeEventArgs : EventArgs
    {
        /// <summary>
        /// Creates new event arguments for a behavior tree node event.
        /// </summary>
        /// <param name="id">Unique identifier of the node</param>
        /// <param name="status">Current status of the node</param>
        /// <param name="eventType">Type of lifecycle event</param>
        [System.Diagnostics.DebuggerStepThrough]
        public BehaviourTreeEventArgs(int id, BehaviourStatus status, BehaviourTreeNodeInfoEventType eventType)
        {
            Id = id;
            Status = status;
            EventType = eventType;
        }

        /// <summary>
        /// Gets the unique identifier of the behavior node.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the current status of the behavior node.
        /// </summary>
        public BehaviourStatus Status { get; }

        /// <summary>
        /// Gets the type of lifecycle event.
        /// </summary>
        public BehaviourTreeNodeInfoEventType EventType { get; }
    }
}