using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that probabilistically executes the child behavior based on a threshold.
    /// Executes child if random value is greater than or equal to threshold, otherwise returns failure.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public sealed class Random<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly IRandomProvider _randomProvider;

        /// <summary>
        /// Creates a new Random decorator with specified threshold.
        /// </summary>
        /// <param name="child">Child behavior to execute conditionally</param>
        /// <param name="threshold">Threshold value between 0.0 (exclusive) and 1.0 (inclusive)</param>
        /// <param name="randomProvider">Optional random number provider (uses default if null)</param>
        public Random(IBehaviour<TContext> child, double threshold, IRandomProvider? randomProvider = null)
            : this("Random", child, threshold, randomProvider)
        {
        }

        /// <summary>
        /// Creates a new Random decorator with specified threshold and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to execute conditionally</param>
        /// <param name="threshold">Threshold value between 0.0 (exclusive) and 1.0 (inclusive)</param>
        /// <param name="randomProvider">Optional random number provider (uses default if null)</param>
        /// <exception cref="ArgumentException">Thrown when threshold is not between 0.0 (exclusive) and 1.0 (inclusive)</exception>
        public Random(string name, IBehaviour<TContext> child, double threshold, IRandomProvider? randomProvider = null) : base(name, child)
        {
            if (threshold <= 0 || threshold > 1)
            {
                throw new ArgumentException(
                    "Threshold value must be between 0.0 (exclusive) and 1.0 (inclusive)",
                    nameof(threshold));
            }

            _randomProvider = randomProvider ?? RandomProvider.Default;

            Threshold = threshold;
        }

        /// <summary>
        /// Gets the threshold value for random execution.
        /// </summary>
        public double Threshold { get; }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var randomValue = _randomProvider.NextRandomDouble();

            if (randomValue >= Threshold)
            {
                return Child.Tick(context);
            }

            return BehaviourStatus.Failed;
        }
    }
}