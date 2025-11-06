namespace BehaviourTree.FluentBuilder
{
    /// <summary>
    /// Builder for leaf behavior nodes without children.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public sealed class LeafBehaviourBuilder<TContext> : BehaviourBuilder<TContext>
    {
        /// <summary>
        /// Gets or sets the factory function for creating the leaf behavior.
        /// </summary>
        public CreateBehaviour<TContext> Factory { get; set; }

        /// <summary>
        /// Builds the leaf behavior using the factory.
        /// </summary>
        /// <returns>Built leaf behavior instance</returns>
        public override IBehaviour<TContext> Build() => Factory();
    }
}