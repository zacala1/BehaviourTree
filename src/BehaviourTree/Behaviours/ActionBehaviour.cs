using System;
using System.Diagnostics;

namespace BehaviourTree.Behaviours
{
    /// <summary>
    /// Leaf node that executes an action and returns the resulting status.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed class ActionBehaviour<TContext> : BaseBehaviour<TContext>
    {
        /// <summary>
        /// Threshold in milliseconds for logging slow action execution during debugging
        /// </summary>
        private const int DEBUG_SLOW_ACTION_THRESHOLD_MS = 100;

        private readonly Func<TContext, BehaviourStatus> _action;

        /// <summary>
        /// Creates an action behavior node.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="action">Action function to execute</param>
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        public ActionBehaviour(string name, Func<TContext, BehaviourStatus> action) : base(name)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Core update logic for this node.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
#if DEBUG
            var timer = Stopwatch.StartNew();
#endif
            var status = _action(context);
#if DEBUG
            if (timer.ElapsedMilliseconds >= DEBUG_SLOW_ACTION_THRESHOLD_MS)
            {
                Debug.WriteLine(
                    $"[{DateTime.Now:HH:mm:ss.ffff}] Behavior Node Time. id={Id}, name={Name}, context={typeof(TContext).Name}, status={status}, time={timer.ElapsedMilliseconds}ms");
            }
            timer.Stop();
#endif
            return status;
        }
    }
}