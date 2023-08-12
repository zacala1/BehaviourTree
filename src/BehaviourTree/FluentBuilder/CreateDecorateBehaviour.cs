namespace BehaviourTree.FluentBuilder
{
    public delegate IBehaviour<TContext> CreateDecorateBehaviour<TContext>(IBehaviour<TContext> child);
}