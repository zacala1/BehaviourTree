using System;

namespace BehaviourTree
{
    public interface IBehaviour<in TContext> : IDisposable
    {
        int Id { get; }
        string Name { get; }
        BehaviourStatus Status { get; }
        BehaviourStatus Tick(TContext context);
        void Reset();
    }
}