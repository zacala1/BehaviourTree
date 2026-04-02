using System;
using System.Collections.Generic;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Base class for composite behavior nodes that contain multiple child nodes.
    /// Composites control the execution flow of their children (e.g., Sequence, Selector).
    /// This class is not thread-safe. All access must be from a single thread.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public abstract class CompositeBehaviour<TContext> : BaseBehaviour<TContext>
    {
        private readonly IBehaviour<TContext>[] _children;

        /// <summary>
        /// Gets the child behavior nodes as a read-only list.
        /// </summary>
        public IReadOnlyList<IBehaviour<TContext>> ChildNodes => _children;

        /// <summary>
        /// Internal array access for subclasses. Do not expose publicly.
        /// </summary>
        protected IBehaviour<TContext>[] Children => _children;

        /// <summary>
        /// Creates a composite behavior node with the specified children.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Array of child nodes</param>
        /// <exception cref="ArgumentNullException">Thrown when children is null</exception>
        /// <exception cref="ArgumentException">Thrown when children contains null elements</exception>
        protected CompositeBehaviour(string name, IBehaviour<TContext>[] children) : base(name)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == null)
                {
                    throw new ArgumentException($"Children[{i}] cannot be null", nameof(children));
                }
            }

            _children = children;
        }

        /// <summary>
        /// Called when this composite node completes execution. Resets all children.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            DoReset(status);
        }

        /// <summary>
        /// Called when this composite node is reset. Resets all children to Ready status.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            ResetChildren();
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void ResetChildren()
        {
            var children = _children;
            for (int i = 0; i < children.Length; i++)
            {
                children[i].Reset();
            }
        }
    }
}
