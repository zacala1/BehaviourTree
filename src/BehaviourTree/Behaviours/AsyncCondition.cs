using System;
using System.Threading;
using System.Threading.Tasks;

namespace BehaviourTree.Behaviours
{
    /// <summary>
    /// Leaf node that evaluates an asynchronous condition predicate.
    /// Returns Succeeded if true, Failed if false, Running while awaiting.
    /// Supports timeout, cancellation conditions, and external cancellation tokens.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public partial class AsyncCondition<TContext> : BaseBehaviour<TContext>
    {
        private readonly Func<TContext, CancellationToken, Task<bool>> _predicate;
        private readonly TimeSpan _timeout;
        private readonly Func<TContext, bool>? _cancelCondition;
        private readonly CancellationToken _externalToken;
        private Task<bool>? _task;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// True if the last terminal status was caused by cancellation.
        /// </summary>
        public bool WasCancelled { get; private set; }

        /// <summary>
        /// The exception from the last faulted async task, if any.
        /// </summary>
        public Exception? LastException { get; private set; }

        /// <summary>
        /// Creates an async condition node with default name.
        /// </summary>
        /// <param name="predicate">Async predicate to evaluate</param>
        /// <param name="timeout">Optional timeout duration</param>
        public AsyncCondition(Func<TContext, CancellationToken, Task<bool>> predicate,
            TimeSpan timeout = default) : this("AsyncCondition", predicate, timeout)
        {
        }

        /// <summary>
        /// Creates an async condition node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="predicate">Async predicate to evaluate</param>
        /// <param name="timeout">Optional timeout duration</param>
        public AsyncCondition(string name,
            Func<TContext, CancellationToken, Task<bool>> predicate,
            TimeSpan timeout = default) : this(name, predicate, null, timeout)
        {
        }

        /// <summary>
        /// Creates an async condition node with cancellation condition.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="predicate">Async predicate to evaluate</param>
        /// <param name="cancelCondition">Predicate that when true cancels the evaluation</param>
        /// <param name="timeout">Optional timeout duration</param>
        public AsyncCondition(string name,
            Func<TContext, CancellationToken, Task<bool>> predicate,
            Func<TContext, bool>? cancelCondition,
            TimeSpan timeout = default) : this(name, predicate, cancelCondition, timeout, default)
        {
        }

        /// <summary>
        /// Creates an async condition node with full configuration.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="predicate">Async predicate to evaluate</param>
        /// <param name="cancelCondition">Predicate that when true cancels the evaluation</param>
        /// <param name="timeout">Optional timeout duration</param>
        /// <param name="externalToken">External cancellation token to link</param>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null</exception>
        public AsyncCondition(string name,
            Func<TContext, CancellationToken, Task<bool>> predicate,
            Func<TContext, bool>? cancelCondition,
            TimeSpan timeout,
            CancellationToken externalToken) : base(name)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _cancelCondition = cancelCondition;
            _timeout = timeout;
            _externalToken = externalToken;
        }

        /// <summary>
        /// Core update logic. Starts the async predicate on first call, polls completion on subsequent calls.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            if (_task == null)
            {
                try
                {
                    _cts = CreateCancellationTokenSource();
                    _task = _predicate.Invoke(context, _cts.Token);

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

            if (_task.IsCompleted)
            {
                return ExtractResult(_task);
            }

            if (_cancelCondition?.Invoke(context) ?? false)
            {
                WasCancelled = true;
                try { _cts?.Cancel(); }
                catch (ObjectDisposedException) { }
                return BehaviourStatus.Failed;
            }

            return BehaviourStatus.Running;
        }

        private BehaviourStatus ExtractResult(Task<bool> completedTask)
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

            return completedTask.Result ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }

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

        private void CleanupAsyncResources()
        {
            if (_cts == null && _task == null)
            {
                return;
            }

            try { _cts?.Cancel(); }
            catch (ObjectDisposedException) { }

            _cts?.Dispose();
            _cts = null;
            _task = null;
        }
    }
}
