using System;

namespace BehaviourTree
{
    public class BehaviourTreeEventArgs : EventArgs
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BehaviourTreeEventType Type { get; set; } 
        public BehaviourStatus Status { get; set; }
    }
    
    public enum BehaviourTreeEventType
    {
        Initialize,
        Update,
        Terminate,
        Reset
    }
}