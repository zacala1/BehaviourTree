using BehaviourTree.Events;
using System;
using System.Diagnostics;
using System.Threading;

namespace BehaviourTree
{
    /// <summary>
    /// Generic base class for behavior tree nodes that work with a specific context type.
    /// Handles the tick lifecycle: Initialize -> Update -> Terminate, and provides observer notifications.
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
        /// OPTIMIZED: Only measures elapsed time when observers are attached or in DEBUG mode.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public BehaviourStatus Tick(TContext context)
        {
            // Initialize on first tick
            if (Status == BehaviourStatus.Ready)
            {
                OnInitialize(context);
                NotifyObservers(BehaviourTreeNodeInfoEventType.Initialize, Status, 0);
            }

            // OPTIMIZATION: Only create Stopwatch if observers exist or in DEBUG mode
            bool needsTiming = HasObservers
#if DEBUG
                || true  // Always time in DEBUG for slow node detection
#endif
                ;

            Stopwatch? timer = null;
            long elapsedMs = 0;

            if (needsTiming)
            {
                timer = Stopwatch.StartNew();
            }

            Status = Update(context);

            if (needsTiming)
            {
                elapsedMs = timer!.ElapsedMilliseconds;
                timer.Stop();
            }

            NotifyObservers(BehaviourTreeNodeInfoEventType.Update, Status, elapsedMs);

#if DEBUG
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
                NotifyObservers(BehaviourTreeNodeInfoEventType.Terminate, Status, elapsedMs);
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
            NotifyObservers(BehaviourTreeNodeInfoEventType.Reset, Status, 0);
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
    /// Non-generic base class for all behavior tree nodes.
    /// Provides core functionality including unique IDs, status tracking, and observer pattern support.
    /// OPTIMIZED: Caches type name and observer array to minimize allocations during tick.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Node: Id = {Id}, Name = {Name}, Status = {Status}")]
    public abstract class BaseBehaviour : IDisposable
    {
        private static long BehaviorCounter = 0;
        private readonly System.Collections.Generic.List<IBehaviourTreeObserver> _observers = new System.Collections.Generic.List<IBehaviourTreeObserver>();
        private readonly object _observerLock = new object();

        // OPTIMIZATION: Cache type name to avoid repeated reflection calls during NotifyObservers
        private readonly string _cachedTypeName;

        // OPTIMIZATION: Cache observer array to avoid ToArray() allocations on every notification
        private IBehaviourTreeObserver[]? _cachedObserverArray;
        private bool _observerArrayDirty = false;

        /// <summary>
        /// Unique identifier for this behavior node, auto-incremented across all instances.
        /// </summary>
        public int Id
        {
            [System.Diagnostics.DebuggerStepThrough]
            get;
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
            get;
            [System.Diagnostics.DebuggerStepThrough]
            protected set;
        }

        /// <summary>
        /// Initializes a new behavior node with the specified name.
        /// </summary>
        /// <param name="name">Human-readable name for this node</param>
        protected BaseBehaviour(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
            Id = (int)Interlocked.Increment(ref BehaviorCounter);
            Name = name;
            Status = BehaviourStatus.Ready;

            // OPTIMIZATION: Cache type name once to avoid repeated GetType().Name calls
            _cachedTypeName = GetType().Name;
        }

        /// <summary>
        /// Returns true if there are any observers attached to this node.
        /// Used for performance optimization to skip timing when not needed.
        /// </summary>
        protected bool HasObservers
        {
            get { return _observers.Count > 0; }
        }

        /// <summary>
        /// Attaches an observer to receive lifecycle event notifications from this behavior tree.
        /// Thread-safe operation that prevents duplicate observers.
        /// OPTIMIZED: Marks observer array cache as dirty for lazy regeneration.
        /// </summary>
        /// <param name="observer">The observer to attach</param>
        public void AttachObserver(IBehaviourTreeObserver observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            lock (_observerLock)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                    _observerArrayDirty = true;  // OPTIMIZATION: Mark cache as dirty
                }
            }
        }

        /// <summary>
        /// Detaches a previously attached observer.
        /// Thread-safe operation.
        /// OPTIMIZED: Marks observer array cache as dirty for lazy regeneration.
        /// </summary>
        /// <param name="observer">The observer to detach</param>
        public void DetachObserver(IBehaviourTreeObserver observer)
        {
            if (observer == null) return;

            lock (_observerLock)
            {
                if (_observers.Remove(observer))
                {
                    _observerArrayDirty = true;  // OPTIMIZATION: Mark cache as dirty
                }
            }
        }

        /// <summary>
        /// Notifies all attached observers of a lifecycle event.
        /// Exceptions in observers are caught and logged to prevent disrupting tree execution.
        /// OPTIMIZED: Uses cached type name and cached observer array to minimize allocations.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected void NotifyObservers(BehaviourTreeNodeInfoEventType eventType, BehaviourStatus status, long elapsedMs)
        {
            // OPTIMIZATION: Early exit for performance when no observers
            if (_observers.Count == 0) return;

            // OPTIMIZATION: Use cached type name instead of GetType().Name
            var nodeEvent = new BehaviourTreeNodeEvent(
                nodeId: Id,
                nodeName: Name,
                nodeType: _cachedTypeName,
                status: status,
                eventType: eventType,
                elapsedMilliseconds: elapsedMs,
                parentId: null, // Can be set by composite nodes if needed
                depth: 0 // Can be set during tree construction if needed
            );

            IBehaviourTreeObserver[] observersCopy;

            lock (_observerLock)
            {
                // OPTIMIZATION: Regenerate cached array only when observers changed
                if (_observerArrayDirty || _cachedObserverArray == null)
                {
                    _cachedObserverArray = _observers.ToArray();
                    _observerArrayDirty = false;
                }

                // Use cached array (no allocation unless observers changed)
                observersCopy = _cachedObserverArray;
            }

            // OPTIMIZATION: Notify outside of lock to prevent deadlocks and improve concurrency
            for (int i = 0; i < observersCopy.Length; i++)
            {
                var observer = observersCopy[i];
                try
                {
                    switch (eventType)
                    {
                        case BehaviourTreeNodeInfoEventType.Initialize:
                            observer.OnNodeInitialize(nodeEvent);
                            break;
                        case BehaviourTreeNodeInfoEventType.Update:
                            observer.OnNodeUpdate(nodeEvent);
                            break;
                        case BehaviourTreeNodeInfoEventType.Terminate:
                            observer.OnNodeTerminate(nodeEvent);
                            break;
                        case BehaviourTreeNodeInfoEventType.Reset:
                            observer.OnNodeReset(nodeEvent);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Prevent observer exceptions from disrupting tree execution
                    Debug.WriteLine($"Observer error in {observer.GetType().Name}: {ex.Message}");
                }
            }
        }

        #region IDisposable

        private bool disposed;

        /// <summary>
        /// Disposes resources held by this behavior node.
        /// Clears all attached observers to prevent memory leaks.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Clear observers to prevent memory leaks
                    lock (_observerLock)
                    {
                        _observers.Clear();
                        _cachedObserverArray = null;
                        _observerArrayDirty = false;
                    }
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Disposes this behavior node and releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}