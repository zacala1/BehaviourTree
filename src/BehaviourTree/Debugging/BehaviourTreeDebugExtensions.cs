using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using System.Collections.Generic;

namespace BehaviourTree.Debugging
{
    /// <summary>
    /// Extension methods for debugging and monitoring behavior trees.
    /// </summary>
    public static class BehaviourTreeDebugExtensions
    {
        /// <summary>
        /// Gets the IDs of all currently running leaf nodes in the behavior tree.
        /// This is the minimal data needed to determine the active execution path.
        /// Use with BehaviourTreeStructure.GetAllActiveNodeIds() to derive the full path.
        /// </summary>
        /// <typeparam name="TContext">Context type of the behavior tree</typeparam>
        /// <param name="root">Root node of the behavior tree</param>
        /// <returns>List of IDs for all leaf nodes with Running status</returns>
        /// <remarks>
        /// Performance: O(n) where n is number of Running nodes in the tree.
        /// Only traverses active branches, not the entire tree.
        /// </remarks>
        public static List<int> GetActiveLeafIds<TContext>(this IBehaviour<TContext> root)
        {
            var result = new List<int>();
            CollectActiveLeafIds(root, result);
            return result;
        }

        /// <summary>
        /// Gets snapshot of all running nodes (both internal and leaf) with their status.
        /// Useful for debugging to see the complete active path.
        /// </summary>
        /// <typeparam name="TContext">Context type of the behavior tree</typeparam>
        /// <param name="root">Root node of the behavior tree</param>
        /// <returns>List of running node snapshots</returns>
        public static List<RunningNodeSnapshot> GetRunningNodes<TContext>(this IBehaviour<TContext> root)
        {
            var result = new List<RunningNodeSnapshot>();
            CollectRunningNodes(root, result);
            return result;
        }

        private static void CollectActiveLeafIds<TContext>(IBehaviour<TContext> node, List<int> result)
        {
            // Only traverse Running nodes
            if (node.Status != BehaviourStatus.Running)
            {
                return;
            }

            // Check if this is a composite with children
            if (node is CompositeBehaviour<TContext> composite)
            {
                foreach (var child in composite.ChildNodes)
                {
                    CollectActiveLeafIds(child, result);
                }
                return;
            }

            // Check if this is a decorator with a child
            if (node is DecoratorBehaviour<TContext> decorator)
            {
                CollectActiveLeafIds(decorator.Child, result);
                return;
            }

            // This is a leaf node (action/condition) that's Running
            result.Add(node.Id);
        }

        private static void CollectRunningNodes<TContext>(IBehaviour<TContext> node, List<RunningNodeSnapshot> result)
        {
            // Only traverse Running nodes
            if (node.Status != BehaviourStatus.Running)
            {
                return;
            }

            // Add this node
            result.Add(new RunningNodeSnapshot(
                node.Id,
                node.Name,
                GetTypeName(node),
                node.Status
            ));

            // Check if this is a composite with children
            if (node is CompositeBehaviour<TContext> composite)
            {
                foreach (var child in composite.ChildNodes)
                {
                    CollectRunningNodes(child, result);
                }
                return;
            }

            // Check if this is a decorator with a child
            if (node is DecoratorBehaviour<TContext> decorator)
            {
                CollectRunningNodes(decorator.Child, result);
            }
        }

        private static string GetTypeName<TContext>(IBehaviour<TContext> node)
        {
            if (node is BaseBehaviour baseBehaviour)
            {
                return baseBehaviour.TypeName;
            }
            return node.GetType().Name;
        }
    }

    /// <summary>
    /// Snapshot of a running node's state for debugging.
    /// </summary>
    public readonly struct RunningNodeSnapshot
    {
        /// <summary>
        /// Node ID.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Node name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Node type name.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Current status (always Running for this snapshot).
        /// </summary>
        public BehaviourStatus Status { get; }

        /// <summary>
        /// Creates a new running node snapshot.
        /// </summary>
        public RunningNodeSnapshot(int id, string name, string typeName, BehaviourStatus status)
        {
            Id = id;
            Name = name;
            TypeName = typeName;
            Status = status;
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString() => $"{TypeName}(id={Id}, name={Name})";
    }
}
