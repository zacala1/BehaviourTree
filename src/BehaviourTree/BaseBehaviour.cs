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
                NotifyObservers(BehaviourTreeNodeInfoEventType.Initialize, Status, 0);
            }

            // Measure execution time for performance monitoring
            var timer = Stopwatch.StartNew();

            Status = Update(context);

            long elapsedMs = timer.ElapsedMilliseconds;
            timer.Stop();

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
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Node: Id = {Id}, Name = {Name}, Status = {Status}")]
    public abstract class BaseBehaviour : IDisposable
    {
        private static long BehaviorCounter = 0;
        private readonly System.Collections.Generic.List<IBehaviourTreeObserver> _observers = new System.Collections.Generic.List<IBehaviourTreeObserver>();
        private readonly object _observerLock = new object();

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

        protected BaseBehaviour(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
            Id = (int)Interlocked.Increment(ref BehaviorCounter);
            Name = name;
            Status = BehaviourStatus.Ready;
        }

        /// <summary>
        /// Attaches an observer to receive lifecycle event notifications from this behavior tree.
        /// Thread-safe operation that prevents duplicate observers.
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
                }
            }
        }

        /// <summary>
        /// Detaches a previously attached observer.
        /// Thread-safe operation.
        /// </summary>
        /// <param name="observer">The observer to detach</param>
        public void DetachObserver(IBehaviourTreeObserver observer)
        {
            if (observer == null) return;

            lock (_observerLock)
            {
                _observers.Remove(observer);
            }
        }

        /// <summary>
        /// Notifies all attached observers of a lifecycle event.
        /// Exceptions in observers are caught and logged to prevent disrupting tree execution.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected void NotifyObservers(BehaviourTreeNodeInfoEventType eventType, BehaviourStatus status, long elapsedMs)
        {
            // Early exit for performance when no observers
            if (_observers.Count == 0) return;

            var nodeEvent = new BehaviourTreeNodeEvent(
                nodeId: Id,
                nodeName: Name,
                nodeType: GetType().Name,
                status: status,
                eventType: eventType,
                elapsedMilliseconds: elapsedMs,
                parentId: null, // Can be set by composite nodes if needed
                depth: 0 // Can be set during tree construction if needed
            );

            lock (_observerLock)
            {
                // Create a copy to avoid issues if observers are modified during notification
                var observersCopy = _observers.ToArray();

                foreach (var observer in observersCopy)
                {
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
        }

        #region IDisposable

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}