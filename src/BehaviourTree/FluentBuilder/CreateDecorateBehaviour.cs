namespace BehaviourTree.FluentBuilder
{
    /// <summary>
    /// Delegate for creating decorator behavior nodes with a single child.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    /// <param name="child">Child behavior to decorate</param>
    /// <returns>Created decorator behavior instance</returns>
    public delegate IBehaviour<TContext> CreateDecorateBehaviour<TContext>(IBehaviour<TContext> child);
}