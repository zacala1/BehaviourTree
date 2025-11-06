namespace BehaviourTree.Composites
{
    public sealed class RandomSequence<TContext> : Sequence<TContext>
    {
        private readonly IRandomProvider _randomProvider;

        public RandomSequence(IBehaviour<TContext>[] children, IRandomProvider? randomProvider = null)
            : this("RandomSequence", children, randomProvider)
        {
        }

        public RandomSequence(string name, IBehaviour<TContext>[] children, IRandomProvider? randomProvider = null) : base(name, children)
        {
            _randomProvider = randomProvider ?? RandomProvider.Default;
            _shuffledChildren = Children.Shuffle(_randomProvider);
        }

        private IBehaviour<TContext>[] _shuffledChildren;

        [System.Diagnostics.DebuggerStepThrough]
        protected override IBehaviour<TContext> GetChild(int index)
        {
            return _shuffledChildren[index];
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _shuffledChildren = Children.Shuffle(_randomProvider);
            base.DoReset(status);
        }
    }
}