using System;

namespace BehaviourTree
{
    public class BehaviourTreeEventArgs : EventArgs
    {
        [System.Diagnostics.DebuggerStepThrough]
        public BehaviourTreeEventArgs(int id, string name, BehaviourTreeEventType type, BehaviourStatus status)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            Status = status;
        }
        public int Id { get; }
        public string Name { get; }
        public BehaviourTreeEventType Type { get; } 
        public BehaviourStatus Status { get; }
    }
}