using System;
using System.Threading;
using System.Threading.Tasks;

namespace BehaviourTree
{
    /// <summary>
    /// Runs a behavior tree repeatedly at a specified interval until stopped or completed.
    /// OPTIMIZED: Lock-free token management using Interlocked operations.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed class BehaviourTreeRunner<TContext> : IDisposable where TContext : class
    {
        private readonly int _intervalInMilliseconds;
        private readonly TContext _context;
        private readonly IBehaviour<TContext> _behaviourTree;

        // LOCK-FREE OPTIMIZATION: Use Interlocked for lock-free CancellationTokenSource management
        private CancellationTokenSource? _tokenSource;

        /// <summary>
        /// Creates a behavior tree runner.
        /// </summary>
        /// <param name="behaviourTree">Behavior tree to execute</param>
        /// <param name="context">Context object for tree execution</param>
        /// <param name="intervalInMilliseconds">Interval between ticks in milliseconds</param>
        /// <exception cref="ArgumentNullException">Thrown when behaviourTree or context is null</exception>
        public BehaviourTreeRunner(IBehaviour<TContext> behaviourTree, TContext context, int intervalInMilliseconds)
        {
            _intervalInMilliseconds = intervalInMilliseconds;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _behaviourTree = behaviourTree ?? throw new ArgumentNullException(nameof(behaviourTree));
        }

        /// <summary>
        /// Runs the behavior tree repeatedly until it returns Success or Failed status.
        /// </summary>
        /// <returns>Task that completes with the final status (Success or Failed)</returns>
        public Task<BehaviourStatus> RunToFailureOrSuccess()
        {
            return DoWork(status =>
                status == BehaviourStatus.Succeeded ||
                status == BehaviourStatus.Failed);
        }

        /// <summary>
        /// Runs the behavior tree repeatedly until explicitly stopped via Stop() method.
        /// </summary>
        /// <returns>Task that completes when runner is stopped</returns>
        public Task<BehaviourStatus> RunUntilStopped()
        {
            return DoWork(status => false);
        }

        /// <summary>
        /// Internal execution loop that ticks the tree at specified intervals.
        /// OPTIMIZED: Lock-free token source creation using Interlocked.
        /// </summary>
        private async Task<BehaviourStatus> DoWork(Predicate<BehaviourStatus> shouldStop)
        {
            Stop();

            // LOCK-FREE: Atomic token source creation
            var newTokenSource = new CancellationTokenSource();
            Interlocked.Exchange(ref _tokenSource, newTokenSource);

            var status = await ExecuteCycle(newTokenSource.Token).ConfigureAwait(false);

            while (!shouldStop(status) && !newTokenSource.IsCancellationRequested)
            {
                status = await ExecuteCycle(newTokenSource.Token).ConfigureAwait(false);
            }

            return status;
        }

        /// <summary>
        /// Stops the running behavior tree by canceling the current execution cycle.
        /// OPTIMIZED: Lock-free cancellation using Interlocked operations.
        /// </summary>
        public void Stop()
        {
            // LOCK-FREE: Atomic token source swap and cancellation
            var currentTokenSource = Interlocked.Exchange(ref _tokenSource, null);

            if (currentTokenSource != null)
            {
                currentTokenSource.Cancel();
                currentTokenSource.Dispose();
            }
        }

        private async Task<BehaviourStatus> ExecuteCycle(CancellationToken token)
        {
            var behaviourStatus = _behaviourTree.Tick(_context);

            await Task.Delay(_intervalInMilliseconds, token).ConfigureAwait(false);

            return behaviourStatus;
        }

        /// <summary>
        /// Disposes the behavior tree runner and releases all resources.
        /// </summary>
        public void Dispose()
        {
            Stop(); // Ensure token source is disposed
            _behaviourTree.Dispose();
        }
    }
}