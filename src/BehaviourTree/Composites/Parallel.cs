using System;
using System.Runtime.CompilerServices;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Parallel composite node that executes all children concurrently.
    /// Success/failure is determined by the specified policy.
    /// Supports any number of children (N >= 1).
    /// </summary>
    public sealed partial class Parallel<TContext> : CompositeBehaviour<TContext>
    {
        private readonly ParallelPolicy _policy;
        private readonly int _successRequired;
        private readonly BehaviourStatus[] _statuses;

        /// <summary>
        /// Gets the policy used to determine success/failure of this parallel node.
        /// </summary>
        public ParallelPolicy Policy => _policy;

        /// <summary>
        /// Gets the number of children required to succeed for this node to succeed.
        /// </summary>
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

            _statuses = new BehaviourStatus[children.Length];
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
            // Treat 0 as RequireOne (at least one must succeed)
            if (successRequired < 0 || successRequired > children.Length)
            {
                throw new ArgumentException(
                    $"successRequired must be between 0 and {children.Length}",
                    nameof(successRequired));
            }

            _policy = ParallelPolicy.RequireN;
            _successRequired = successRequired == 0 ? 1 : successRequired;
            _statuses = new BehaviourStatus[children.Length];
        }

        /// <summary>
        /// Executes all children in parallel and evaluates success policy.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override BehaviourStatus Update(TContext context)
        {
            var children = Children;
            var statuses = _statuses;
            var count = children.Length;

            int succeededCount = 0;
            int failedCount = 0;

            for (var i = 0; i < count; i++)
            {
                var currentStatus = statuses[i];

                // Only tick children that are Ready or Running
                if (currentStatus == BehaviourStatus.Ready || currentStatus == BehaviourStatus.Running)
                {
                    statuses[i] = children[i].Tick(context);
                    currentStatus = statuses[i];
                }

                if (currentStatus == BehaviourStatus.Succeeded)
                    succeededCount++;
                else if (currentStatus == BehaviourStatus.Failed)
                    failedCount++;
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

            return BehaviourStatus.Running;
        }

        /// <summary>
        /// Resets child status tracking when the node is reset.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void DoReset(BehaviourStatus status)
        {
            Array.Clear(_statuses, 0, _statuses.Length);
            base.DoReset(status);
        }
    }
}
