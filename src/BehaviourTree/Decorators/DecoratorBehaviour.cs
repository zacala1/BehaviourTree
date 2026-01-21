namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Base class for decorator behavior nodes that modify or control a single child node.
    /// Decorators can alter the result, repeat execution, add conditions, etc.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public abstract class DecoratorBehaviour<TContext> : BaseBehaviour<TContext>
    {
        /// <summary>
        /// The child behavior node that this decorator wraps.
        /// </summary>
        public readonly IBehaviour<TContext> Child;

        /// <summary>
        /// Creates a decorator behavior node wrapping a child node.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child behavior node to decorate</param>
        protected DecoratorBehaviour(string name, IBehaviour<TContext> child) : base(name)
        {
            Child = child;
        }

        /// <summary>
        /// Called when this decorator node is reset. Resets the child node.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            Child.Reset();
        }
    }
}