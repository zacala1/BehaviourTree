using System;

namespace BehaviourTree.Events
{
    public class BehaviourTreeEventArgs : EventArgs
    {
        [System.Diagnostics.DebuggerStepThrough]
        public BehaviourTreeEventArgs(int id, BehaviourStatus status, BehaviourTreeNodeInfoEventType eventType)
        {
            Id = id;
            Status = status;
            EventType = eventType;
        }

        public int Id { get; }
        public BehaviourStatus Status { get; }
        public BehaviourTreeNodeInfoEventType EventType { get; }
    }
}