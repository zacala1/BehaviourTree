namespace BehaviourTree.Composites
{
    /// <summary>
    /// Active Sequence (also known as Reactive Sequence) re-evaluates all children from the beginning every tick.
    /// This allows the sequence to react to changing conditions in earlier children.
    ///
    /// Behavior:
    /// - Evaluates children in order until one returns Failed or Running
    /// - If a child returns Success, moves to the next child
    /// - Returns Success only if all children succeed
    /// - Returns Running if any child is running
    /// - Returns Failed if any child fails
    ///
    /// Note: This is an alias for PrioritySequence which implements the same reactive behavior.
    /// </summary>
    public sealed class ActiveSequence<TContext> : PrioritySequence<TContext>
    {
        /// <summary>
        /// Creates an active sequence node with default name.
        /// </summary>
        /// <param name="children">Array of child nodes to evaluate</param>
        public ActiveSequence(IBehaviour<TContext>[] children) : base("ActiveSequence", children)
        {
        }

        /// <summary>
        /// Creates an active sequence node with specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Array of child nodes to evaluate</param>
        public ActiveSequence(string name, IBehaviour<TContext>[] children) : base(name, children)
        {
        }

        /// <summary>
        /// Creates an active sequence node with default name and variable number of children.
        /// </summary>
        /// <param name="children">Variable number of child nodes to evaluate</param>
        public ActiveSequence(params IBehaviour<TContext>[] children) : base("ActiveSequence", children)
        {
        }

        /// <summary>
        /// Creates an active sequence node with specified name and variable number of children.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Variable number of child nodes to evaluate</param>
        public ActiveSequence(string name, params IBehaviour<TContext>[] children) : base(name, children)
        {
        }
    }
}
