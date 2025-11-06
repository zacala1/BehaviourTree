namespace BehaviourTree.FluentBuilder
{
    /// <summary>
    /// Delegate for creating composite behavior nodes with multiple children.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    /// <param name="children">Array of child behaviors</param>
    /// <returns>Created composite behavior instance</returns>
    public delegate IBehaviour<TContext> CreateCompositeBehaviour<TContext>(IBehaviour<TContext>[] children);
}