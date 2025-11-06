using System;
using System.Threading;
using System.Threading.Tasks;

namespace BehaviourTree.Behaviours
{
    /// <summary>
    /// Leaf node that executes an asynchronous action and tracks its completion.
    /// Supports timeout and cancellation conditions for async operations.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public class AsyncAction<TContext> : BaseBehaviour<TContext>
    {
        private readonly Func<TContext, CancellationToken, Task<BehaviourStatus>> action;
        private readonly TimeSpan timeout;
        private readonly Func<TContext, bool> cancelCondition;
        private Task<BehaviourStatus>? task;
        private CancellationTokenSource? cts;

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
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        public AsyncAction(string name,
            Func<TContext, CancellationToken, Task<BehaviourStatus>> action,
            Func<TContext, bool> cancelCondition,
            TimeSpan timeout = default) : base(name)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            this.action = action;
            this.cancelCondition = cancelCondition;
            this.timeout = timeout;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            if (task == null)
            {
                try
                {
                    cts = new CancellationTokenSource(timeout);
                    task = action.Invoke(context, cts.Token);
                    return BehaviourStatus.Running;
                }
                catch
                {
                    return BehaviourStatus.Failed;
                }
            }

            if (task.IsCompleted)
            {
                try
                {
                    return task.Result;
                }
                catch
                {
                    return BehaviourStatus.Failed;
                }
            }

            if (cancelCondition?.Invoke(context) ?? false)
            {
                cts.Cancel();
                return BehaviourStatus.Failed;
            }

            return BehaviourStatus.Running;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            DoReset(status);
            base.OnTerminate(status);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            base.DoReset(status);
            CleanupAsyncResources();
        }

        /// <summary>
        /// Disposes async resources (CancellationTokenSource and Task).
        /// Ensures resources are cleaned up even if Reset is never called.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanupAsyncResources();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Cleans up CancellationTokenSource and Task resources.
        /// </summary>
        private void CleanupAsyncResources()
        {
            if (cts == null && task == null)
            {
                return; // Already cleaned
            }

            try
            {
                cts?.Cancel();
                task?.Wait(100); // Wait max 100ms to avoid blocking
            }
            catch
            {
                // Ignore cancellation and timeout exceptions
            }
            finally
            {
                cts?.Dispose();
                task?.Dispose();
                cts = null;
                task = null;
            }
        }
    }
}