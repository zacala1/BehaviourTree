namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that automatically resets its child after completion.
    /// Useful for repeatable actions that should reset immediately after finishing.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class AutoReset<TContext> : DecoratorBehaviour<TContext>
    {
        /// <summary>
        /// Creates an auto-reset decorator with default name.
        /// </summary>
        /// <param name="child">Child node to execute and auto-reset</param>
        public AutoReset(IBehaviour<TContext> child) : this("AutoReset", child)
        {
        }

        /// <summary>
        /// Creates an auto-reset decorator with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child node to execute and auto-reset</param>
        public AutoReset(string name, IBehaviour<TContext> child) : base(name, child)
        {
        }

        /// <summary>Core update logic for this node.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            return Child.Tick(context);
        }

        /// <summary>Called when node completes execution.</summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            Child.Reset();
        }
    }
}