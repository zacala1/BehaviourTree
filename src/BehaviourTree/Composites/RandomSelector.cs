namespace BehaviourTree.Composites
{
    /// <summary>
    /// Selector that shuffles child execution order randomly on each reset.
    /// Executes children in random order until one succeeds or all fail.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed partial class RandomSelector<TContext> : Selector<TContext>
    {
        private readonly IRandomProvider _randomProvider;
        private readonly int[] _shuffledIndices;

        /// <summary>
        /// Creates a random selector node with default name.
        /// </summary>
        /// <param name="children">Array of child nodes to execute in random order</param>
        /// <param name="randomProvider">Optional random provider (uses default if not specified)</param>
        public RandomSelector(IBehaviour<TContext>[] children, IRandomProvider? randomProvider = null)
            : this("RandomSelector", children, randomProvider)
        {
        }

        /// <summary>
        /// Creates a random selector node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Array of child nodes to execute in random order</param>
        /// <param name="randomProvider">Optional random provider (uses default if not specified)</param>
        public RandomSelector(string name, IBehaviour<TContext>[] children, IRandomProvider? randomProvider = null) : base(name, children)
        {
            _randomProvider = randomProvider ?? RandomProvider.Default;
            _shuffledIndices = new int[children.Length];
            InitializeAndShuffleIndices();
        }

        private void InitializeAndShuffleIndices()
        {
            for (int i = 0; i < _shuffledIndices.Length; i++)
            {
                _shuffledIndices[i] = i;
            }
            _shuffledIndices.ShuffleInPlace(_randomProvider);
        }

        /// <summary>
        /// Returns the child at the specified index from the shuffled order.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override IBehaviour<TContext> GetChild(int index)
        {
            return Children[_shuffledIndices[index]];
        }

        /// <summary>
        /// Re-shuffles the indices when the node is reset.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            _shuffledIndices.ShuffleInPlace(_randomProvider);
            base.DoReset(status);
        }
    }
}
