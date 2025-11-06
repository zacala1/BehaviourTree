namespace BehaviourTree
{
    /// <summary>
    /// Represents the execution status of a behavior node.
    /// </summary>
    public enum BehaviourStatus
    {
        /// <summary>
        /// The behavior is ready to execute.
        /// </summary>
        Ready = 0,

        /// <summary>
        /// The behavior is currently executing.
        /// </summary>
        Running = 1,

        /// <summary>
        /// The behavior completed successfully.
        /// </summary>
        Succeeded = 2,

        /// <summary>
        /// The behavior failed.
        /// </summary>
        Failed = 3
    }
}