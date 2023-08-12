using System;
using System.Linq;

namespace BehaviourTree.Composites
{
    public abstract class CompositeBehaviour<TContext> : BaseBehaviour<TContext>
    {
        public readonly IBehaviour<TContext>[] Children;

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

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            foreach (var child in Children)
            {
                child.Dispose();
            }
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