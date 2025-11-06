namespace BehaviourTree.Composites
{
    /// <summary>
    /// Policy for determining when a Parallel composite node succeeds or fails
    /// </summary>
    public enum ParallelPolicy
    {
        /// <summary>
        /// Succeeds when all children succeed, fails when any child fails
        /// </summary>
        RequireAll,

        /// <summary>
        /// Succeeds when at least one child succeeds, fails when all children fail
        /// </summary>
        RequireOne,

        /// <summary>
        /// Custom policy where you specify the required number of successes
        /// Use with Parallel constructor that accepts successRequired parameter
        /// </summary>
        RequireN
    }
}
