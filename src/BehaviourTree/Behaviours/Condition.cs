using System;

namespace BehaviourTree.Behaviours
{
    /// <summary>
    /// Leaf node that evaluates a condition predicate and returns Success or Failed.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    // ReSharper disable once ClassCanBeSealed.Global
    public partial class Condition<TContext> : BaseBehaviour<TContext>
    {
        private readonly Func<TContext, bool> _predicate;

        /// <summary>
        /// Creates a condition node with default name.
        /// </summary>
        /// <param name="predicate">Condition predicate to evaluate</param>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null</exception>
        public Condition(Func<TContext, bool> predicate) : this(null, predicate)
        {
        }

        /// <summary>
        /// Creates a condition node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="predicate">Condition predicate to evaluate</param>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null</exception>
        public Condition(string? name, Func<TContext, bool> predicate) : base(name ?? "Condition")
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Evaluates the condition predicate and returns Success or Failed status.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            return _predicate(context) ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }
    }
}
