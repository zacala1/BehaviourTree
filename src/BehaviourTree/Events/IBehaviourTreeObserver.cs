namespace BehaviourTree.Events
{
    /// <summary>
    /// Observer interface for monitoring behavior tree node lifecycle events.
    /// Implement this interface to receive notifications about node initialization,
    /// updates, termination, and resets without memory leak concerns.
    /// </summary>
    public interface IBehaviourTreeObserver
    {
        /// <summary>
        /// Called when a behavior node is initialized (first tick).
        /// </summary>
        /// <param name="nodeEvent">Event information including node ID, name, and status</param>
        void OnNodeInitialize(BehaviourTreeNodeEvent nodeEvent);

        /// <summary>
        /// Called when a behavior node is updated (ticked).
        /// </summary>
        /// <param name="nodeEvent">Event information including node ID, name, and status</param>
        void OnNodeUpdate(BehaviourTreeNodeEvent nodeEvent);

        /// <summary>
        /// Called when a behavior node terminates (completes with Success or Failure).
        /// </summary>
        /// <param name="nodeEvent">Event information including node ID, name, and status</param>
        void OnNodeTerminate(BehaviourTreeNodeEvent nodeEvent);

        /// <summary>
        /// Called when a behavior node is reset back to Ready status.
        /// </summary>
        /// <param name="nodeEvent">Event information including node ID, name, and status</param>
        void OnNodeReset(BehaviourTreeNodeEvent nodeEvent);
    }
}
