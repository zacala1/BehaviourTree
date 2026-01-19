namespace BehaviourTree.Events
{
    /// <summary>
    /// Enhanced event information for behavior tree node lifecycle events.
    /// Provides comprehensive data for debugging and monitoring including
    /// node identity, execution time, and tree hierarchy information.
    /// </summary>
    public readonly struct BehaviourTreeNodeEvent
    {
        /// <summary>
        /// Unique identifier for the behavior node.
        /// </summary>
        public int NodeId { get; }

        /// <summary>
        /// Human-readable name of the behavior node.
        /// </summary>
        public string NodeName { get; }

        /// <summary>
        /// Type name of the behavior node (e.g., "Sequence", "Selector", "Action").
        /// </summary>
        public string NodeType { get; }

        /// <summary>
        /// Current status of the behavior node (Ready, Running, Success, Failed).
        /// </summary>
        public BehaviourStatus Status { get; }

        /// <summary>
        /// Type of lifecycle event (Initialize, Update, Terminate, Reset).
        /// </summary>
        public BehaviourTreeNodeInfoEventType EventType { get; }

        /// <summary>
        /// Time elapsed during the Update phase in milliseconds.
        /// Only meaningful for Update and Terminate events.
        /// </summary>
        public long ElapsedMilliseconds { get; }

        /// <summary>
        /// Parent node ID if this node has a parent, -1 for root nodes.
        /// </summary>
        public int ParentId { get; }

        /// <summary>
        /// Depth of this node in the tree hierarchy (0 for root, 1 for immediate children, etc.).
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Creates a new behavior tree node event with the specified information.
        /// </summary>
        /// <param name="nodeId">Unique identifier for the node</param>
        /// <param name="nodeName">Human-readable name of the node</param>
        /// <param name="nodeType">Type name of the node</param>
        /// <param name="status">Current status of the node</param>
        /// <param name="eventType">Type of lifecycle event</param>
        /// <param name="elapsedMilliseconds">Time elapsed during execution</param>
        /// <param name="parentId">Parent node ID if applicable, -1 for root</param>
        /// <param name="depth">Depth of this node in the tree hierarchy</param>
        public BehaviourTreeNodeEvent(
            int nodeId,
            string nodeName,
            string nodeType,
            BehaviourStatus status,
            BehaviourTreeNodeInfoEventType eventType,
            long elapsedMilliseconds = 0,
            int parentId = -1,
            int depth = 0)
        {
            NodeId = nodeId;
            NodeName = nodeName;
            NodeType = nodeType;
            Status = status;
            EventType = eventType;
            ElapsedMilliseconds = elapsedMilliseconds;
            ParentId = parentId;
            Depth = depth;
        }

        /// <summary>
        /// Returns a string representation of this event for debugging and logging.
        /// </summary>
        public override string ToString()
        {
            return $"[{EventType}] {NodeType}(id={NodeId}, name={NodeName}, status={Status}, elapsed={ElapsedMilliseconds}ms)";
        }
    }
}
