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
        public ActiveSequence(IBehaviour<TContext>[] children) : base("ActiveSequence", children)
        {
        }

        public ActiveSequence(string name, IBehaviour<TContext>[] children) : base(name, children)
        {
        }
    }
}
