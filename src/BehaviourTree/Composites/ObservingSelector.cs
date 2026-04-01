namespace BehaviourTree.Composites
{
    /// <summary>
    /// Event-driven selector that only re-evaluates higher priority children
    /// when an IAbortableObserver child signals a condition change.
    /// Unlike PrioritySelector (which re-evaluates everything every tick),
    /// this selector stays on the current running child and only interrupts
    /// when a higher priority child's observed condition changes.
    /// This is significantly more efficient for large trees with many branches.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class ObservingSelector<TContext> : CompositeBehaviour<TContext>
    {
        private int _currentChildIndex;

        public ObservingSelector(params IBehaviour<TContext>[] children) : this("ObservingSelector", children)
        {
        }

        public ObservingSelector(string name, params IBehaviour<TContext>[] children) : base(name, children)
        {
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var children = Children;
            var count = children.Length;

            // Check if any higher-priority child with IAbortableObserver wants to interrupt
            bool shouldReEvaluate = false;
            for (var i = 0; i < _currentChildIndex; i++)
            {
                if (children[i] is IAbortableObserver observer && observer.IsAbortRequested)
                {
                    observer.ClearAbortRequest();
                    shouldReEvaluate = true;
                    break;
                }
            }

            // If a higher priority child's condition changed, re-evaluate from the beginning
            if (shouldReEvaluate)
            {
                // Reset current running child
                if (_currentChildIndex < count && children[_currentChildIndex].Status == BehaviourStatus.Running)
                {
                    children[_currentChildIndex].Reset();
                }
                _currentChildIndex = 0;
            }

            // Normal selector behavior from current position
            while (_currentChildIndex < count)
            {
                var childStatus = children[_currentChildIndex].Tick(context);

                if (childStatus != BehaviourStatus.Failed)
                {
                    // Reset all children after current (lower priority)
                    for (var j = _currentChildIndex + 1; j < count; j++)
                    {
                        if (children[j].Status != BehaviourStatus.Ready)
                        {
                            children[j].Reset();
                        }
                    }
                    return childStatus;
                }

                _currentChildIndex++;
            }

            return BehaviourStatus.Failed;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            // Clear any pending abort requests
            for (var i = 0; i < Children.Length; i++)
            {
                if (Children[i] is IAbortableObserver observer)
                {
                    observer.ClearAbortRequest();
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _currentChildIndex = 0;
            base.DoReset(status);
        }
    }
}
