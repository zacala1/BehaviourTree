using System;
using System.Collections.Generic;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;

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
        /// Builds a BehaviourTreeStructure by traversing the tree from the root node.
        /// </summary>
        /// <typeparam name="TContext">Context type of the behavior tree</typeparam>
        /// <param name="root">Root node of the behavior tree</param>
        /// <returns>Complete structure representation of the tree</returns>
        public static BehaviourTreeStructure Build<TContext>(IBehaviour<TContext> root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            var nodes = new Dictionary<int, NodeInfo>();
            BuildRecursive(root, -1, 0, nodes);
            return new BehaviourTreeStructure(nodes, root.Id);
        }

        private static void BuildRecursive<TContext>(IBehaviour<TContext> node, int parentId, int depth, Dictionary<int, NodeInfo> nodes)
        {
            var childIds = Array.Empty<int>();
            var isLeaf = true;

            if (node is CompositeBehaviour<TContext> composite)
            {
                isLeaf = false;
                childIds = new int[composite.Children.Length];
                for (int i = 0; i < composite.Children.Length; i++)
                {
                    childIds[i] = composite.Children[i].Id;
                }
            }
            else if (node is DecoratorBehaviour<TContext> decorator)
            {
                isLeaf = false;
                childIds = new[] { decorator.Child.Id };
            }

            var typeName = (node is BaseBehaviour baseBehaviour) ? baseBehaviour.TypeName : node.GetType().Name;

            nodes[node.Id] = new NodeInfo(
                node.Id,
                node.Name,
                typeName,
                parentId,
                depth,
                isLeaf,
                childIds
            );

            if (node is CompositeBehaviour<TContext> comp)
            {
                foreach (var child in comp.Children)
                {
                    BuildRecursive(child, node.Id, depth + 1, nodes);
                }
            }
            else if (node is DecoratorBehaviour<TContext> dec)
            {
                BuildRecursive(dec.Child, node.Id, depth + 1, nodes);
            }
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
