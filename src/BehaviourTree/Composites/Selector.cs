namespace BehaviourTree.Composites
{
    public class Selector<TContext> : CompositeBehaviour<TContext>
    {
        private int _currentChildIndex;

        public Selector(IBehaviour<TContext>[] children) : this("Selector", children)
        {
        }

        public Selector(string name, IBehaviour<TContext>[] children) : base(name, children)
        {
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected virtual IBehaviour<TContext> GetChild(int index)
        {
            return Children[index];
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            do
            {
                var childStatus = GetChild(_currentChildIndex).Tick(context);

                if (childStatus != BehaviourStatus.Failed)
                {
                    return childStatus;
                }
            } while (++_currentChildIndex < Children.Length);

            return BehaviourStatus.Failed;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _currentChildIndex = 0;
            base.DoReset(status);
        }
    }
}