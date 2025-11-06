namespace BehaviourTree.Events
{
    /// <summary>
    /// Defines the types of lifecycle events that can occur during behavior tree execution.
    /// </summary>
    public enum BehaviourTreeNodeInfoEventType
    {
        /// <summary>
        /// Node is being initialized for first execution.
        /// </summary>
        Initialize,

        /// <summary>
        /// Node is being updated (ticked).
        /// </summary>
        Update,

        /// <summary>
        /// Node execution is terminating with a final status.
        /// </summary>
        Terminate,

        /// <summary>
        /// Node is being reset back to ready state.
        /// </summary>
        Reset
    }
}