using System;

namespace BehaviourTree.Decorators
{
    public sealed class Random<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly IRandomProvider _randomProvider;
        private double _threshold;

        public double Threshold => _threshold;

        public Random(IBehaviour<TContext> child, double threshold, IRandomProvider randomProvider = null)
            : this("Random", child, threshold, randomProvider)
        {
        }

        public Random(string name, IBehaviour<TContext> child, double threshold, IRandomProvider randomProvider = null) : base(name, child)
        {
            if (threshold <= 0 || threshold > 1)
            {
                throw new ArgumentException(
                    "Threshold value must be between 0 (exclusive) and 100 (inclusive)",
                    nameof(threshold));
            }

            _randomProvider = randomProvider ?? RandomProvider.Default;

            _threshold = threshold;
        }

        

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            var randomValue = _randomProvider.NextRandomDouble();

            if (randomValue >= _threshold)
            {
                return Child.Tick(context);
            }

            return BehaviourStatus.Failed;
        }
    }
}
