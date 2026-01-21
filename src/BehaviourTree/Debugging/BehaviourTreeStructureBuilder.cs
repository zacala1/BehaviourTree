using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using System.Collections.Generic;

namespace BehaviourTree.Debugging
{
    /// <summary>
    /// Builds a BehaviourTreeStructure from a behavior tree root node.
    /// Call this once at initialization to create a structure for debugging/visualization.
    /// </summary>
    public static class BehaviourTreeStructureBuilder
    {
        /// <summary>
        /// Builds the complete tree structure from the root node.
        /// </summary>
        /// <typeparam name="TContext">Context type of the behavior tree</typeparam>
        /// <param name="root">Root node of the behavior tree</param>
        /// <returns>Complete tree structure for debugging/visualization</returns>
        public static BehaviourTreeStructure Build<TContext>(IBehaviour<TContext> root)
        {
            var nodes = new Dictionary<int, NodeInfo>();
            BuildNodeRecursive(root, -1, 0, nodes);
            return new BehaviourTreeStructure(nodes, root.Id);
        }

        private static void BuildNodeRecursive<TContext>(
            IBehaviour<TContext> node,
            int parentId,
            int depth,
            Dictionary<int, NodeInfo> nodes)
        {
            var childIds = GetChildIds(node);
            var isLeaf = childIds.Length == 0;

            var nodeInfo = new NodeInfo(
                id: node.Id,
                name: node.Name,
                typeName: GetTypeName(node),
                parentId: parentId,
                depth: depth,
                isLeaf: isLeaf,
                childIds: childIds
            );

            nodes[node.Id] = nodeInfo;

            // Recursively process children
            foreach (var child in GetChildren(node))
            {
                BuildNodeRecursive(child, node.Id, depth + 1, nodes);
            }
        }

        private static string GetTypeName<TContext>(IBehaviour<TContext> node)
        {
            // Use cached TypeName if available from BaseBehaviour
            if (node is BaseBehaviour baseBehaviour)
            {
                return baseBehaviour.TypeName;
            }
            return node.GetType().Name;
        }

        private static int[] GetChildIds<TContext>(IBehaviour<TContext> node)
        {
            if (node is CompositeBehaviour<TContext> composite)
            {
                var ids = new int[composite.Children.Length];
                for (int i = 0; i < composite.Children.Length; i++)
                {
                    ids[i] = composite.Children[i].Id;
                }
                return ids;
            }

            if (node is DecoratorBehaviour<TContext> decorator)
            {
                return new[] { decorator.Child.Id };
            }

            return System.Array.Empty<int>();
        }

        private static IEnumerable<IBehaviour<TContext>> GetChildren<TContext>(IBehaviour<TContext> node)
        {
            if (node is CompositeBehaviour<TContext> composite)
            {
                return composite.Children;
            }

            if (node is DecoratorBehaviour<TContext> decorator)
            {
                return new[] { decorator.Child };
            }

            return System.Array.Empty<IBehaviour<TContext>>();
        }
    }
}
