using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that references another behavior tree as a subtree.
    /// Enables modular tree composition and reuse.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class SubTree<TContext> : DecoratorBehaviour<TContext>
    {
        /// <summary>
        /// Creates a subtree node with default name.
        /// </summary>
        /// <param name="subTree">Root node of the subtree to execute</param>
        /// <exception cref="ArgumentNullException">Thrown when subTree is null</exception>
        public SubTree(IBehaviour<TContext> subTree) : this("SubTree", subTree)
        {
        }

        /// <summary>
        /// Creates a subtree node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="subTree">Root node of the subtree to execute</param>
        /// <exception cref="ArgumentNullException">Thrown when subTree is null</exception>
        public SubTree(string name, IBehaviour<TContext> subTree) : base(name, subTree ?? throw new ArgumentNullException(nameof(subTree)))
        {
        }

        /// <summary>
        /// Ticks the subtree and returns its status.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            return Child.Tick(context);
        }
    }
}
