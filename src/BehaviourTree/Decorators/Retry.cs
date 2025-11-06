using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that retries the child behavior a specified number of times on failure.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public sealed class Retry<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, int> _getRetryCount;
        private int _retryCount;
        private int _counter;

        /// <summary>
        /// Gets the maximum number of retry attempts.
        /// </summary>
        public int RetryCount => _retryCount;

        /// <summary>
        /// Gets the current retry attempt counter.
        /// </summary>
        public int Counter => _counter;

        /// <summary>
        /// Creates a new Retry decorator with a dynamic retry count.
        /// </summary>
        /// <param name="child">Child behavior to retry on failure</param>
        /// <param name="getRetryCount">Function to get retry count from context</param>
        public Retry(IBehaviour<TContext> child, Func<TContext, int> getRetryCount)
            : this("Retry", child, getRetryCount)
        {
        }

        /// <summary>
        /// Creates a new Retry decorator with a dynamic retry count and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to retry on failure</param>
        /// <param name="getRetryCount">Function to get retry count from context</param>
        /// <exception cref="ArgumentNullException">Thrown when getRetryCount is null</exception>
        public Retry(string name, IBehaviour<TContext> child, Func<TContext, int> getRetryCount)
            : base(name, child)
        {
            _getRetryCount = getRetryCount ?? throw new ArgumentNullException(nameof(getRetryCount));
        }

        /// <summary>
        /// Creates a new Retry decorator with a fixed retry count.
        /// </summary>
        /// <param name="child">Child behavior to retry on failure</param>
        /// <param name="retryCount">Number of retry attempts (must be at least one)</param>
        public Retry(IBehaviour<TContext> child, int retryCount)
            : this("Retry", child, retryCount)
        {
        }

        /// <summary>
        /// Creates a new Retry decorator with a fixed retry count and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to retry on failure</param>
        /// <param name="retryCount">Number of retry attempts (must be at least one)</param>
        /// <exception cref="ArgumentException">Thrown when retryCount is less than one</exception>
        public Retry(string name, IBehaviour<TContext> child, int retryCount)
            : base(name, child)
        {
            if (retryCount < 1)
            {
                throw new ArgumentException("retryCount must be at least one", nameof(retryCount));
            }

            _retryCount = retryCount;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Failed)
            {
                _counter++;

                if (_counter < _retryCount)
                {
                    Child.Reset();  // Reset child before retry
                    return BehaviourStatus.Running;
                }
            }

            return childStatus;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            if (_getRetryCount != null)
            {
                _retryCount = _getRetryCount.Invoke(context);
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            _counter = 0;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _counter = 0;
            base.DoReset(status);
        }
    }
}