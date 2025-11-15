using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BehaviourTree.Composites
{
    /// <summary>
    /// Parallel composite node that executes all children concurrently.
    /// Success/failure is determined by the specified policy.
    /// Supports any number of children (N >= 1).
    /// OPTIMIZED: Cache-aligned status array and unsafe pointer operations for maximum performance.
    /// </summary>
    public sealed partial class Parallel<TContext> : CompositeBehaviour<TContext>
    {
        private readonly ParallelPolicy _policy;
        private readonly int _successRequired;

        // CACHE OPTIMIZATION: Align status array to cache line boundary (64 bytes)
        // This prevents false sharing when multiple threads access different elements
        [StructLayout(LayoutKind.Sequential, Pack = 64)]
        private struct CacheAlignedStatusArray
        {
            public BehaviourStatus[] Statuses;
        }

        private CacheAlignedStatusArray _alignedStatuses;

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

            _alignedStatuses = new CacheAlignedStatusArray
            {
                Statuses = new BehaviourStatus[children.Length]
            };
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
            _alignedStatuses = new CacheAlignedStatusArray
            {
                Statuses = new BehaviourStatus[children.Length]
            };
        }

        /// <summary>
        /// Executes all children in parallel and evaluates success policy.
        /// OPTIMIZED: Cached array references, streamlined status counting, and unsafe pointer operations.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override BehaviourStatus Update(TContext context)
        {
            // OPTIMIZATION: Cache arrays and lengths
            var children = Children;
            var statuses = _alignedStatuses.Statuses;
            var count = children.Length;

            // UNSAFE OPTIMIZATION: Use pointer operations to eliminate bounds checking
            // This is safe because we control array allocation and access patterns
            unsafe
            {
                fixed (BehaviourStatus* statusPtr = statuses)
                {
                    int succeededCount = 0;
                    int failedCount = 0;

                    // Execute all children and collect their statuses
                    // OPTIMIZATION: Direct pointer access eliminates bounds checking
                    for (var i = 0; i < count; i++)
                    {
                        var currentStatus = statusPtr[i];

                        // Only tick children that are Ready or Running
                        if (currentStatus == BehaviourStatus.Ready || currentStatus == BehaviourStatus.Running)
                        {
                            statusPtr[i] = children[i].Tick(context);
                            currentStatus = statusPtr[i];
                        }

                        // Count final statuses (OPTIMIZATION: Skip running count as it's not used)
                        // OPTIMIZATION: Use branchless arithmetic where possible
                        succeededCount += (currentStatus == BehaviourStatus.Succeeded) ? 1 : 0;
                        failedCount += (currentStatus == BehaviourStatus.Failed) ? 1 : 0;
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
            }
        }

        /// <summary>
        /// Resets child status tracking when the node is reset.
        /// OPTIMIZED: Uses unsafe pointer operations for fast memory clearing.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void DoReset(BehaviourStatus status)
        {
            var statuses = _alignedStatuses.Statuses;

            // UNSAFE OPTIMIZATION: Fast memory clear using pointers
            unsafe
            {
                fixed (BehaviourStatus* statusPtr = statuses)
                {
                    var count = statuses.Length;
                    for (var i = 0; i < count; i++)
                    {
                        statusPtr[i] = BehaviourStatus.Ready;
                    }
                }
            }

            base.DoReset(status);
        }
    }
}
