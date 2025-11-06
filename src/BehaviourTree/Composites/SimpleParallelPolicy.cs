namespace BehaviourTree.Composites
{
    /// <summary>
    /// Policy for determining success/failure in SimpleParallel composite.
    /// </summary>
    public enum SimpleParallelPolicy
    {
        /// <summary>
        /// Succeeds only when both children succeed, fails when either child fails.
        /// </summary>
        BothMustSucceed,

        /// <summary>
        /// Succeeds when at least one child succeeds, fails when both children fail.
        /// </summary>
        OnlyOneMustSucceed
    }
}