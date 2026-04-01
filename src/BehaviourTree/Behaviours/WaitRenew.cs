using System;

namespace BehaviourTree.Behaviours
{
    /// <summary>
    /// Deprecated: Use <see cref="Wait{TContext}"/> with Func constructor instead.
    /// Kept for backward compatibility.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    [Obsolete("Use Wait<TContext> with Func<TContext, long> constructor instead.")]
    public sealed partial class WaitRenew<TContext> : BaseBehaviour<TContext>
    {
        private readonly Wait<TContext> _inner;

        /// <summary>
        /// Gets the current wait time in milliseconds.
        /// </summary>
        public long WaitTimeInMilliseconds => _inner.WaitTimeInMilliseconds;

        /// <summary>
        /// Creates a new WaitRenew behavior with a dynamic wait time.
        /// </summary>
        /// <param name="getWaitTimeInMilliseconds">Function to get wait time from context</param>
        public WaitRenew(Func<TContext, long> getWaitTimeInMilliseconds) : this("Wait", getWaitTimeInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a new WaitRenew behavior with a dynamic wait time and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="getWaitTimeInMilliseconds">Function to get wait time from context</param>
        /// <exception cref="ArgumentNullException">Thrown when getWaitTimeInMilliseconds is null</exception>
        public WaitRenew(string name, Func<TContext, long> getWaitTimeInMilliseconds) : base(name)
        {
            _inner = new Wait<TContext>(name, getWaitTimeInMilliseconds);
        }

        /// <summary>Core update logic for this node.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            return _inner.Tick(context);
        }

        /// <summary>Called when node completes execution.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _inner.Reset();
        }

        /// <summary>Resets node state for re-execution.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _inner.Reset();
        }
    }
}
