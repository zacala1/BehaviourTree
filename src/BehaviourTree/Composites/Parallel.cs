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

        /// <summary>
        /// Executes all children in parallel and evaluates success policy.
        /// OPTIMIZED: Cached array references and streamlined status counting.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            // OPTIMIZATION: Cache arrays and lengths
            var children = Children;
            var statuses = _childStatuses;
            var count = children.Length;

            int succeededCount = 0;
            int failedCount = 0;

            // Execute all children and collect their statuses
            for (var i = 0; i < count; i++)
            {
                var currentStatus = statuses[i];

                // Only tick children that are Ready or Running
                if (currentStatus == BehaviourStatus.Ready || currentStatus == BehaviourStatus.Running)
                {
                    statuses[i] = children[i].Tick(context);
                    currentStatus = statuses[i];
                }

                // Count final statuses (OPTIMIZATION: Skip running count as it's not used)
                if (currentStatus == BehaviourStatus.Succeeded)
                {
                    succeededCount++;
                }
                else if (currentStatus == BehaviourStatus.Failed)
                {
                    failedCount++;
                }
            }

            // Check if we've met the success condition
            if (succeededCount >= _successRequired)
            {
                return BehaviourStatus.Succeeded;
            }

            // Check if it's impossible to meet success condition
            int remainingChildren = count - failedCount - succeededCount;
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
