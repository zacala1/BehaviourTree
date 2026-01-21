using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace BehaviourTree
{
    /// <summary>
    /// Generic base class for behavior tree nodes that work with a specific context type.
    /// Handles the tick lifecycle: Initialize -> Update -> Terminate.
    /// </summary>
    /// <typeparam name="TContext">Type of context object used during execution</typeparam>
    public abstract class BaseBehaviour<TContext> : BaseBehaviour, IBehaviour<TContext>
    {
        /// <summary>
        /// Threshold in milliseconds for detecting slow behavior nodes during debugging.
        /// Nodes taking longer than this will trigger a debug warning.
        /// </summary>
        private const int DEBUG_SLOW_NODE_THRESHOLD_MS = 80;

        /// <summary>
        /// Initializes a new behavior node with the specified name.
        /// </summary>
        /// <param name="name">Human-readable name for this node</param>
        protected BaseBehaviour(string name) : base(name)
        {
        }

        /// <summary>
        /// Executes one iteration of this behavior node's lifecycle.
        /// First tick calls Initialize, subsequent ticks call Update, and completion calls Terminate.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public BehaviourStatus Tick(TContext context)
        {
            // Initialize on first tick
            if (Status == BehaviourStatus.Ready)
            {
                OnInitialize(context);
            }

#if DEBUG
            var timer = Stopwatch.StartNew();
#endif

            Status = Update(context);

#if DEBUG
            var elapsedMs = timer.ElapsedMilliseconds;
            timer.Stop();

            // Warn about slow nodes in debug mode
            if (elapsedMs >= DEBUG_SLOW_NODE_THRESHOLD_MS)
            {
                Debug.WriteLine($"[{DateTime.Now:yyyy/MM/dd/HH:mm:ss.ffff}] Behavior Node is hanging. id={Id}, name={Name}, status={Status}, time={elapsedMs}ms");
            }
#endif

            if (Status == BehaviourStatus.Ready)
            {
                throw new InvalidOperationException("Ready status should not be returned by Behaviour Update Method");
            }

            // Terminate when complete
            if (Status != BehaviourStatus.Running)
            {
                OnTerminate(Status);
            }

            return Status;
        }

        /// <summary>
        /// Resets this behavior node back to Ready status, allowing it to be executed again.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public void Reset()
        {
            if (Status == BehaviourStatus.Ready)
            {
                return;
            }

            DoReset(Status);
            Status = BehaviourStatus.Ready;
        }

        /// <summary>
        /// Core update logic for this behavior node. Must be implemented by derived classes.
        /// </summary>
        /// <param name="context">Context object containing shared state</param>
        /// <returns>The resulting status (Running, Success, or Failed)</returns>
        [System.Diagnostics.DebuggerStepThrough]
        protected abstract BehaviourStatus Update(TContext context);

        /// <summary>
        /// Called when this behavior completes (Success or Failed). Override to cleanup resources.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected virtual void OnTerminate(BehaviourStatus status)
        { }

        /// <summary>
        /// Called on first tick when this behavior is initialized. Override to setup state.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected virtual void OnInitialize(TContext context)
        { }

        /// <summary>
        /// Called when this behavior is reset. Override to cleanup state.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected virtual void DoReset(BehaviourStatus status)
        { }
    }

    /// <summary>
    /// Cache-aligned hot fields for optimal CPU cache performance.
    /// Aligned to 64-byte cache line to prevent false sharing.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    internal struct CacheAlignedNodeState
    {
        [FieldOffset(0)]
        public BehaviourStatus Status;

        [FieldOffset(4)]
        public int Id;
    }

    /// <summary>
    /// Non-generic base class for all behavior tree nodes.
    /// Provides core functionality including unique IDs, status tracking, and cached type metadata.
    /// OPTIMIZED: Cache-aligned hot fields to improve CPU cache hit rate.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Node: Id = {Id}, Name = {Name}, Status = {Status}")]
    public abstract class BaseBehaviour
    {
        private static long BehaviorCounter = 0;

        // CACHE OPTIMIZATION: Hot fields in cache-aligned struct (64-byte aligned)
        private CacheAlignedNodeState _state;

        // OPTIMIZATION: Cache type name to avoid repeated reflection calls
        private readonly string _cachedTypeName;

        /// <summary>
        /// Unique identifier for this behavior node, auto-incremented across all instances.
        /// </summary>
        public int Id
        {
            [System.Diagnostics.DebuggerStepThrough]
            get => _state.Id;
        }

        /// <summary>
        /// Human-readable name for this behavior node, used for debugging and visualization.
        /// </summary>
        public string Name
        {
            [System.Diagnostics.DebuggerStepThrough]
            get;
        }

        /// <summary>
        /// Current execution status: Ready, Running, Success, or Failed.
        /// </summary>
        public BehaviourStatus Status
        {
            [System.Diagnostics.DebuggerStepThrough]
            get => _state.Status;
            [System.Diagnostics.DebuggerStepThrough]
            protected set => _state.Status = value;
        }

        /// <summary>
        /// Cached type name for efficient access during debugging and visualization.
        /// </summary>
        public string TypeName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get => _cachedTypeName;
        }

        /// <summary>
        /// Initializes a new behavior node with the specified name.
        /// </summary>
        /// <param name="name">Human-readable name for this node</param>
        protected BaseBehaviour(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            // Initialize cache-aligned state
            _state = new CacheAlignedNodeState
            {
                Id = (int)Interlocked.Increment(ref BehaviorCounter),
                Status = BehaviourStatus.Ready
            };

            Name = name;

            // OPTIMIZATION: Use Source Generator metadata to avoid reflection
            // Falls back to reflection only if metadata not available
            _cachedTypeName = (this is IBehaviourMetadata metadata)
                ? metadata.TypeName
                : GetType().Name;
        }
    }
}
