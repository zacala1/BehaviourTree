using System;
using System.Linq;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Base class for composite behavior nodes that contain multiple child nodes.
    /// Composites control the execution flow of their children (e.g., Sequence, Selector).
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public abstract class CompositeBehaviour<TContext> : BaseBehaviour<TContext>
    {
        /// <summary>
        /// Array of child behavior nodes that this composite will execute.
        /// </summary>
        public readonly IBehaviour<TContext>[] Children;

        /// <summary>
        /// Creates a composite behavior node with the specified children.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Array of child nodes (must contain at least one non-null child)</param>
        /// <exception cref="ArgumentNullException">Thrown when children is null</exception>
        /// <exception cref="ArgumentException">Thrown when children is empty or contains null elements</exception>
        protected CompositeBehaviour(string name, IBehaviour<TContext>[] children) : base(name)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            if (children.Length == 0)
            {
                throw new ArgumentException("Must have at least one child", nameof(children));
            }

            if (children.Any(x => x == null))
            {
                throw new ArgumentException("Children cannot contain null elements", nameof(children));
            }

            Children = children;
        }

        /// <summary>
        /// Disposes this composite node and all its children.
        /// Ensures proper disposal chain by calling base.Dispose.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose all children first
                foreach (var child in Children)
                {
                    child?.Dispose();
                }
            }

            // IMPORTANT: Call base to clear observers
            base.Dispose(disposing);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            DoReset(status);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            ResetChildren();
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void ResetChildren()
        {
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
    }
}