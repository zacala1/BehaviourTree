namespace BehaviourTree.FluentBuilder
{
    /// <summary>
    /// Delegate for creating leaf behavior nodes without children.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    /// <returns>Created leaf behavior instance</returns>
    public delegate IBehaviour<TContext> CreateBehaviour<in TContext>();
}