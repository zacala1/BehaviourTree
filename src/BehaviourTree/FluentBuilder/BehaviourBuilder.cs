namespace BehaviourTree.FluentBuilder
{
    /// <summary>
    /// Abstract base class for building behavior tree nodes in the fluent builder pattern.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public abstract class BehaviourBuilder<TContext>
    {
        /// <summary>
        /// Builds and returns the behavior tree node.
        /// </summary>
        /// <returns>Built behavior instance</returns>
        public abstract IBehaviour<TContext> Build();
    }
}