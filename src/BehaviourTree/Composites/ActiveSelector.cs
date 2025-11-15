namespace BehaviourTree.Composites
{
    /// <summary>
    /// Active Selector (also known as Reactive Selector) re-evaluates all children from the beginning every tick.
    /// This allows higher priority children to interrupt lower priority running children.
    ///
    /// Behavior:
    /// - Evaluates children in order until one returns Success or Running
    /// - If a child returns Failed, moves to the next child
    /// - Returns Success if any child succeeds
    /// - Returns Running if any child is running
    /// - Returns Failed only if all children fail
    ///
    /// Note: This is an alias for PrioritySelector which implements the same reactive behavior.
    /// </summary>
    public sealed partial class ActiveSelector<TContext> : PrioritySelector<TContext>
    {
        /// <summary>
        /// Creates an active selector node with default name and variable number of children.
        /// </summary>
        /// <param name="children">Variable number of child nodes to evaluate</param>
        public ActiveSelector(params IBehaviour<TContext>[] children) : base("ActiveSelector", children)
        {
        }

        /// <summary>
        /// Creates an active selector node with specified name and variable number of children.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="children">Variable number of child nodes to evaluate</param>
        public ActiveSelector(string name, params IBehaviour<TContext>[] children) : base(name, children)
        {
        }
    }
}
