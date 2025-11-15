using System;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Optimized parallel composite for exactly two children.
    /// Executes both children concurrently with configurable success policy.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class SimpleParallel<TContext> : CompositeBehaviour<TContext>
    {
        private readonly IBehaviour<TContext> _first;
        private readonly IBehaviour<TContext> _second;
        private BehaviourStatus _firstStatus;
        private BehaviourStatus _secondStatus;
        private readonly Func<TContext, BehaviourStatus> _behave;

        /// <summary>
        /// Policy determining when this parallel node succeeds or fails.
        /// </summary>
        public readonly SimpleParallelPolicy Policy;

        /// <summary>
        /// Creates a simple parallel node with default name.
        /// </summary>
        /// <param name="policy">Policy for determining success/failure</param>
        /// <param name="first">First child node</param>
        /// <param name="second">Second child node</param>
        public SimpleParallel(SimpleParallelPolicy policy, IBehaviour<TContext> first, IBehaviour<TContext> second) : this("SimpleParallel", policy, first, second)
        {
        }

        /// <summary>
        /// Creates a simple parallel node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="policy">Policy for determining success/failure</param>
        /// <param name="first">First child node</param>
        /// <param name="second">Second child node</param>
        public SimpleParallel(string name, SimpleParallelPolicy policy, IBehaviour<TContext> first, IBehaviour<TContext> second) : base(name, new[] { first, second })
        {
            Policy = policy;
            _first = first;
            _second = second;
            _behave = policy == SimpleParallelPolicy.BothMustSucceed ? (Func<TContext, BehaviourStatus>)BothMustSucceedBehaviour : OnlyOneMustSucceedBehaviour;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private BehaviourStatus OnlyOneMustSucceedBehaviour(TContext context)
        {
            if (_firstStatus == BehaviourStatus.Succeeded || _secondStatus == BehaviourStatus.Succeeded)
            {
                return BehaviourStatus.Succeeded;
            }

            if (_firstStatus == BehaviourStatus.Failed && _secondStatus == BehaviourStatus.Failed)
            {
                return BehaviourStatus.Failed;
            }

            return BehaviourStatus.Running;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private BehaviourStatus BothMustSucceedBehaviour(TContext context)
        {
            if (_firstStatus == BehaviourStatus.Succeeded && _secondStatus == BehaviourStatus.Succeeded)
            {
                return BehaviourStatus.Succeeded;
            }

            if (_firstStatus == BehaviourStatus.Failed || _secondStatus == BehaviourStatus.Failed)
            {
                return BehaviourStatus.Failed;
            }

            return BehaviourStatus.Running;
        }

        /// <summary>
        /// Executes both children and evaluates the result based on the policy.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            if (Status != BehaviourStatus.Running)
            {
                _firstStatus = _first.Tick(context);
                _secondStatus = _second.Tick(context);
            }
            else
            {
                if (_firstStatus == BehaviourStatus.Ready || _firstStatus == BehaviourStatus.Running)
                {
                    _firstStatus = _first.Tick(context);
                }

                if (_secondStatus == BehaviourStatus.Ready || _secondStatus == BehaviourStatus.Running)
                {
                    _secondStatus = _second.Tick(context);
                }
            }

            return _behave(context);
        }

        /// <summary>
        /// Resets both child status tracking when the node is reset.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _firstStatus = BehaviourStatus.Ready;
            _secondStatus = BehaviourStatus.Ready;
            base.DoReset(status);
        }
    }
}