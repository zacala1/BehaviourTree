using System;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Parallel composite node that executes all children concurrently.
    /// Success/failure is determined by the specified policy.
    /// Supports any number of children (N >= 1).
    /// </summary>
    public sealed class Parallel<TContext> : CompositeBehaviour<TContext>
    {
        private readonly ParallelPolicy _policy;
        private readonly int _successRequired;
        private BehaviourStatus[] _childStatuses;

        public ParallelPolicy Policy => _policy;
        public int SuccessRequired => _successRequired;

        /// <summary>
        /// Creates a Parallel node with the specified policy
        /// </summary>
        public Parallel(ParallelPolicy policy, params IBehaviour<TContext>[] children)
            : this("Parallel", policy, children)
        {
        }

        /// <summary>
        /// Creates a Parallel node with the specified policy and name
        /// </summary>
        public Parallel(string name, ParallelPolicy policy, params IBehaviour<TContext>[] children)
            : base(name, children)
        {
            _policy = policy;

            switch (policy)
            {
                case ParallelPolicy.RequireAll:
                    _successRequired = children.Length;
                    break;
                case ParallelPolicy.RequireOne:
                    _successRequired = 1;
                    break;
                default:
                    throw new ArgumentException(
                        "For ParallelPolicy.RequireN, use constructor with successRequired parameter",
                        nameof(policy));
            }

            _childStatuses = new BehaviourStatus[children.Length];
        }

        /// <summary>
        /// Creates a Parallel node that requires a specific number of children to succeed
        /// </summary>
        public Parallel(int successRequired, params IBehaviour<TContext>[] children)
            : this("Parallel", successRequired, children)
        {
        }

        /// <summary>
        /// Creates a Parallel node that requires a specific number of children to succeed
        /// </summary>
        public Parallel(string name, int successRequired, params IBehaviour<TContext>[] children)
            : base(name, children)
        {
            if (successRequired < 1 || successRequired > children.Length)
            {
                throw new ArgumentException(
                    $"successRequired must be between 1 and {children.Length}",
                    nameof(successRequired));
            }

            _policy = ParallelPolicy.RequireN;
            _successRequired = successRequired;
            _childStatuses = new BehaviourStatus[children.Length];
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            int succeededCount = 0;
            int failedCount = 0;
            int runningCount = 0;

            // Execute all children and collect their statuses
            for (var i = 0; i < Children.Length; i++)
            {
                // Only tick children that are Ready or Running
                if (_childStatuses[i] == BehaviourStatus.Ready || _childStatuses[i] == BehaviourStatus.Running)
                {
                    _childStatuses[i] = Children[i].Tick(context);
                }

                // Count final statuses
                switch (_childStatuses[i])
                {
                    case BehaviourStatus.Succeeded:
                        succeededCount++;
                        break;
                    case BehaviourStatus.Failed:
                        failedCount++;
                        break;
                    case BehaviourStatus.Running:
                        runningCount++;
                        break;
                }
            }

            // Check if we've met the success condition
            if (succeededCount >= _successRequired)
            {
                return BehaviourStatus.Succeeded;
            }

            // Check if it's impossible to meet success condition
            // (too many failures to ever reach the required successes)
            int remainingChildren = Children.Length - failedCount - succeededCount;
            if (succeededCount + remainingChildren < _successRequired)
            {
                return BehaviourStatus.Failed;
            }

            // Still running
            return BehaviourStatus.Running;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            for (var i = 0; i < _childStatuses.Length; i++)
            {
                _childStatuses[i] = BehaviourStatus.Ready;
            }
            base.DoReset(status);
        }
    }
}
