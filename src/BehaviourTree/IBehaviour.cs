using BehaviourTree.Events;
using System;

namespace BehaviourTree
{
    /// <summary>
    /// Core interface for all behavior tree nodes.
    /// Provides lifecycle management (Tick, Reset) and observer pattern support.
    /// </summary>
    public interface IBehaviour<in TContext> : IDisposable
    {
        /// <summary>
        /// Unique identifier for this behavior node.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Human-readable name for this behavior node.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Current execution status of this behavior node.
        /// </summary>
        BehaviourStatus Status { get; }

        /// <summary>
        /// Executes one tick of this behavior node.
        /// </summary>
        /// <param name="context">Context object containing shared state</param>
        /// <returns>The resulting status after this tick</returns>
        BehaviourStatus Tick(TContext context);

        /// <summary>
        /// Resets this behavior node back to Ready status.
        /// </summary>
        void Reset();

        /// <summary>
        /// Attaches an observer to receive lifecycle event notifications from this behavior tree.
        /// The observer will be notified of Initialize, Update, Terminate, and Reset events.
        /// </summary>
        /// <param name="observer">The observer to attach</param>
        void AttachObserver(IBehaviourTreeObserver observer);

        /// <summary>
        /// Detaches a previously attached observer.
        /// </summary>
        /// <param name="observer">The observer to detach</param>
        void DetachObserver(IBehaviourTreeObserver observer);
    }
}