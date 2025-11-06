namespace BehaviourTree.Decorators
{
    public abstract class DecoratorBehaviour<TContext> : BaseBehaviour<TContext>
    {
        public readonly IBehaviour<TContext> Child;

        protected DecoratorBehaviour(string name, IBehaviour<TContext> child) : base(name)
        {
            Child = child;
        }

        /// <summary>
        /// Disposes this decorator node and its child.
        /// Ensures proper disposal chain by calling base.Dispose.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose child first
                Child?.Dispose();
            }

            // IMPORTANT: Call base to clear observers
            base.Dispose(disposing);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            Child.Reset();
        }
    }
}