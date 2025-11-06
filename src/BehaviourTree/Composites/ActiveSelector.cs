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
    public sealed class ActiveSelector<TContext> : PrioritySelector<TContext>
    {
        public ActiveSelector(IBehaviour<TContext>[] children) : base("ActiveSelector", children)
        {
        }

        public ActiveSelector(string name, IBehaviour<TContext>[] children) : base(name, children)
        {
        }
    }
}
