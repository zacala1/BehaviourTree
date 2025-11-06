namespace BehaviourTree.Composites
{
    /// <summary>
    /// Sequence that shuffles child execution order randomly on each reset.
    /// Executes children in random order until one fails or all succeed.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed class RandomSequence<TContext> : Sequence<TContext>
    {
        private readonly IRandomProvider _randomProvider;

        /// <summary>
        /// Creates a random sequence node with default name.
        /// </summary>
        /// <param name="children">Array of child nodes to execute in random order</param>
        /// <param name="randomProvider">Optional random provider (uses default if not specified)</param>
        public RandomSequence(IBehaviour<TContext>[] children, IRandomProvider? randomProvider = null)
            : this("RandomSequence", children, randomProvider)
        {
        }

        /// <summary>
        /// Creates a random sequence node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Array of child nodes to execute in random order</param>
        /// <param name="randomProvider">Optional random provider (uses default if not specified)</param>
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