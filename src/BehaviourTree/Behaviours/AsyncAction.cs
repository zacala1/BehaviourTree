using System;
using System.Threading;
using System.Threading.Tasks;

namespace BehaviourTree.Behaviours
{
    /// <summary>
    /// Leaf node that executes an asynchronous action and tracks its completion.
    /// Supports timeout, cancellation conditions, and external cancellation tokens.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public partial class AsyncAction<TContext> : BaseBehaviour<TContext>, IDisposable
    {
        private readonly Func<TContext, CancellationToken, Task<BehaviourStatus>> _action;
        private readonly TimeSpan _timeout;
        private readonly Func<TContext, bool>? _cancelCondition;
        private readonly CancellationToken _externalToken;
        private Task<BehaviourStatus>? _task;
        private CancellationTokenSource? _cts;
        private CancellationTokenRegistration _externalRegistration;

        /// <summary>
        /// True if the last terminal status was caused by cancellation
        /// (timeout, cancelCondition, or external token).
        /// Reset to false when the node re-initializes.
        /// </summary>
        public bool WasCancelled { get; private set; }

        /// <summary>
        /// The exception from the last faulted async task, if any.
        /// Null when the task succeeded, was cancelled, or has not yet completed.
        /// Reset when the node re-initializes.
        /// </summary>
        public Exception? LastException { get; private set; }

        /// <summary>
        /// Creates an async action node with default name.
        /// </summary>
        /// <param name="action">Async action to execute</param>
        /// <param name="timeout">Optional timeout duration (default: no timeout)</param>
        public AsyncAction(Func<TContext, CancellationToken, Task<BehaviourStatus>> action,
            TimeSpan timeout = default) : this("ActionAsync", action, timeout)
        {
        }

        /// <summary>
        /// Creates an async action node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="action">Async action to execute</param>
        /// <param name="timeout">Optional timeout duration (default: no timeout)</param>
        public AsyncAction(string name, Func<TContext, CancellationToken, Task<BehaviourStatus>> action,
            TimeSpan timeout = default) : this(name, action, null, timeout)
        {
        }

        /// <summary>
        /// Creates an async action node with cancellation condition.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="action">Async action to execute</param>
        /// <param name="cancelCondition">Predicate that when true cancels the action</param>
        /// <param name="timeout">Optional timeout duration (default: no timeout)</param>
        public AsyncAction(string name,
            Func<TContext, CancellationToken, Task<BehaviourStatus>> action,
            Func<TContext, bool>? cancelCondition,
            TimeSpan timeout = default) : this(name, action, cancelCondition, timeout, default)
        {
        }

        /// <summary>
        /// Creates an async action node with full configuration.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="action">Async action to execute</param>
        /// <param name="cancelCondition">Predicate that when true cancels the action</param>
        /// <param name="timeout">Optional timeout duration (default: no timeout)</param>
        /// <param name="externalToken">External cancellation token to link</param>
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        public AsyncAction(string name,
            Func<TContext, CancellationToken, Task<BehaviourStatus>> action,
            Func<TContext, bool>? cancelCondition,
            TimeSpan timeout,
            CancellationToken externalToken) : base(name)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _cancelCondition = cancelCondition;
            _timeout = timeout;
            _externalToken = externalToken;
        }

        /// <summary>
        /// Core update logic. Starts the async task on first call, polls completion on subsequent calls.
        /// If the task completes synchronously, returns the result on the same tick.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            // Start task on first tick
            if (_task == null)
            {
                try
                {
                    _cts = CreateCancellationTokenSource();
                    _task = _action.Invoke(context, _cts.Token);

                    // Optimization: if task completed synchronously, return result immediately
                    if (_task.IsCompleted)
                    {
                        return ExtractResult(_task);
                    }

                    return BehaviourStatus.Running;
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    return BehaviourStatus.Failed;
                }
            }

            // Check completion
            if (_task.IsCompleted)
            {
                return ExtractResult(_task);
            }

            // Check cancel condition
            if (_cancelCondition?.Invoke(context) ?? false)
            {
                WasCancelled = true;
                try { _cts?.Cancel(); }
                catch (ObjectDisposedException) { }
                return BehaviourStatus.Failed;
            }

            return BehaviourStatus.Running;
        }

        /// <summary>
        /// Extracts the result from a completed task without throwing exceptions.
        /// </summary>
        private BehaviourStatus ExtractResult(Task<BehaviourStatus> completedTask)
        {
            if (completedTask.IsFaulted)
            {
                var exception = completedTask.Exception;
                LastException = exception?.InnerExceptions.Count == 1
                    ? exception.InnerException
                    : exception;
                return BehaviourStatus.Failed;
            }

            if (completedTask.IsCanceled)
            {
                WasCancelled = true;
                return BehaviourStatus.Failed;
            }

            // RanToCompletion - safe to access Result
            return completedTask.Result;
        }

        /// <summary>
        /// Creates a CancellationTokenSource with optional timeout and external token linking.
        /// </summary>
        private CancellationTokenSource CreateCancellationTokenSource()
        {
            CancellationTokenSource cts;

            if (_externalToken.CanBeCanceled)
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(_externalToken);
                if (_timeout > TimeSpan.Zero)
                {
                    cts.CancelAfter(_timeout);
                }
            }
            else
            {
                cts = _timeout > TimeSpan.Zero
                    ? new CancellationTokenSource(_timeout)
                    : new CancellationTokenSource();
            }

            return cts;
        }

        /// <summary>
        /// Called when node terminates.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            DoReset(status);
            base.OnTerminate(status);
        }

        /// <summary>
        /// Called on first tick to reset diagnostic state.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            WasCancelled = false;
            LastException = null;
        }

        /// <summary>
        /// Called when node is reset.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            base.DoReset(status);
            CleanupAsyncResources();
        }

        /// <summary>
        /// Cancels the running task and releases resources without blocking.
        /// </summary>
        private void CleanupAsyncResources()
        {
            if (_cts == null && _task == null)
            {
                return;
            }

            _externalRegistration.Dispose();

            try { _cts?.Cancel(); }
            catch (ObjectDisposedException) { }

            _cts?.Dispose();
            _cts = null;
            _task = null;
        }

        /// <summary>
        /// Disposes the async action, cancelling any running task.
        /// </summary>
        public void Dispose()
        {
            CleanupAsyncResources();
        }
    }
}
