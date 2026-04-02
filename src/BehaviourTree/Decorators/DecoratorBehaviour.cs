using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Base class for decorator behavior nodes that modify or control a single child node.
    /// Decorators can alter the result, repeat execution, add conditions, etc.
    /// This class is not thread-safe. All access must be from a single thread.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public abstract class DecoratorBehaviour<TContext> : BaseBehaviour<TContext>
    {
        /// <summary>
        /// The child behavior node that this decorator wraps.
        /// </summary>
        public IBehaviour<TContext> Child { get; }

        /// <summary>
        /// Creates a decorator behavior node wrapping a child node.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child behavior node to decorate (must not be null)</param>
        /// <exception cref="ArgumentNullException">Thrown when child is null</exception>
        protected DecoratorBehaviour(string name, IBehaviour<TContext> child) : base(name)
        {
            Child = child ?? throw new ArgumentNullException(nameof(child));
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
