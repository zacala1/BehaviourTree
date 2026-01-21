using System.Collections.Generic;

namespace BehaviourTree.Debugging
{
    /// <summary>
    /// Represents the static structure of a behavior tree for debugging and visualization.
    /// Build this once at initialization time and use it to derive active node paths.
    /// </summary>
    public sealed class BehaviourTreeStructure
    {
        /// <summary>
        /// All nodes in the tree, indexed by their unique ID.
        /// </summary>
        public IReadOnlyDictionary<int, NodeInfo> Nodes { get; }

        /// <summary>
        /// The root node ID of the tree.
        /// </summary>
        public int RootId { get; }

        /// <summary>
        /// Creates a new behavior tree structure.
        /// </summary>
        /// <param name="nodes">Dictionary of all nodes by ID</param>
        /// <param name="rootId">ID of the root node</param>
        public BehaviourTreeStructure(IReadOnlyDictionary<int, NodeInfo> nodes, int rootId)
        {
            Nodes = nodes;
            RootId = rootId;
        }

        /// <summary>
        /// Gets the path from root to the specified node.
        /// </summary>
        /// <param name="nodeId">Target node ID</param>
        /// <returns>List of node IDs from root to target (inclusive)</returns>
        public List<int> GetPathToNode(int nodeId)
        {
            var path = new List<int>();
            var currentId = nodeId;

            while (currentId != -1 && Nodes.TryGetValue(currentId, out var node))
            {
                path.Add(currentId);
                currentId = node.ParentId;
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Gets all active node IDs given a set of active leaf IDs.
        /// This derives the complete active path from just the leaf information.
        /// </summary>
        /// <param name="activeLeafIds">IDs of currently running leaf nodes</param>
        /// <returns>Set of all active node IDs (leaves + all ancestors)</returns>
        public HashSet<int> GetAllActiveNodeIds(IEnumerable<int> activeLeafIds)
        {
            var activeNodes = new HashSet<int>();

            foreach (var leafId in activeLeafIds)
            {
                var currentId = leafId;

                while (currentId != -1 && Nodes.TryGetValue(currentId, out var node))
                {
                    if (!activeNodes.Add(currentId))
                    {
                        // Already processed this branch
                        break;
                    }
                    currentId = node.ParentId;
                }
            }

            return activeNodes;
        }
    }

    /// <summary>
    /// Information about a single node in the behavior tree structure.
    /// </summary>
    public readonly struct NodeInfo
    {
        /// <summary>
        /// Unique identifier for this node.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Human-readable name of this node.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Type name of this node (e.g., "Sequence", "Selector").
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Parent node ID, or -1 if this is the root.
        /// </summary>
        public int ParentId { get; }

        /// <summary>
        /// Depth in the tree (0 for root).
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// Whether this node is a leaf node (has no children).
        /// </summary>
        public bool IsLeaf { get; }

        /// <summary>
        /// IDs of child nodes.
        /// </summary>
        public int[] ChildIds { get; }

        /// <summary>
        /// Creates a new node info.
        /// </summary>
        public NodeInfo(int id, string name, string typeName, int parentId, int depth, bool isLeaf, int[] childIds)
        {
            Id = id;
            Name = name;
            TypeName = typeName;
            ParentId = parentId;
            Depth = depth;
            IsLeaf = isLeaf;
            ChildIds = childIds;
        }
    }
}
