using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using System;

namespace BehaviourTree.Reflection
{
    /// <summary>
    /// Extension methods for extracting reflection information from behavior tree nodes.
    /// </summary>
    public static class BehaviourTreeInfoExtension
    {
        /// <summary>
        /// Gets reflection information for the entire behavior tree starting from this node.
        /// </summary>
        /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
        /// <param name="behaviour">Root behavior node to analyze</param>
        /// <returns>Hierarchical tree information structure</returns>
        public static BehaviourTreeInfo GetInfo<TContext>(this IBehaviour<TContext> behaviour)
        {
            BehaviourTreeInfo? treeInfo = null;
            InternalGetInfos(ref treeInfo, 0, behaviour);
            return treeInfo!;
        }

        private static void InternalGetInfos<TContext>(ref BehaviourTreeInfo? treeInfo, int depth, IBehaviour<TContext> child)
        {
            InternalGetInfos(ref treeInfo, depth, (dynamic)child);
        }

        private static void InternalGetInfos<TContext>(ref BehaviourTreeInfo? treeInfo, int depth, CompositeBehaviour<TContext> obj)
        {
            TreeNodeType nodeType;
            switch (obj)
            {
                case SimpleParallel<TContext> _:
                    nodeType = TreeNodeType.Composite_Parallel;
                    break;

                case PrioritySelector<TContext> _:
                case RandomSelector<TContext> _:
                case Selector<TContext> _:
                    nodeType = TreeNodeType.Composite_Selector;
                    break;

                case PrioritySequence<TContext> _:
                case RandomSequence<TContext> _:
                case Sequence<TContext> _:
                    nodeType = TreeNodeType.Composite_Sequence;
                    break;

                default:
                    nodeType = TreeNodeType.Composite;
                    break;
            }
            var nodeInfo = new BehaviourTreeInfo(obj.Name, obj.Id, nodeType)
            {
                NodeTypeSpecific = obj.GetType().FullName ?? obj.GetType().Name,
                Parent = treeInfo,
                Status = obj.Status,
                Depth = depth,
                IsExpanded = true
            };

            if (treeInfo == null) treeInfo = nodeInfo;
            else treeInfo.ChildrenTreeInfos.Add(nodeInfo);

            var childDepth = depth + 1;
            foreach (var child in obj.Children)
            {
                InternalGetInfos(ref nodeInfo, depth + 1, child);
            }
        }

        private static void InternalGetInfos<TContext>(ref BehaviourTreeInfo? treeInfo, int depth, DecoratorBehaviour<TContext> obj)
        {
            var nodeInfo = new BehaviourTreeInfo(obj.Name, obj.Id, TreeNodeType.Decorate)
            {
                NodeTypeSpecific = obj.GetType().FullName ?? obj.GetType().Name,
                Parent = treeInfo,
                Status = obj.Status,
                Depth = depth,
                IsExpanded = true
            };

            if (treeInfo == null) treeInfo = nodeInfo;
            else treeInfo.ChildrenTreeInfos.Add(nodeInfo);

            InternalGetInfos(ref nodeInfo, ++depth, obj.Child);
        }

        private static void InternalGetInfos<TContext>(ref BehaviourTreeInfo? treeInfo, int depth, BaseBehaviour<TContext> obj) where TContext : IClock
        {
            TreeNodeType nodeType;
            switch (obj)
            {
                case Wait<TContext> _:
                    nodeType = TreeNodeType.Leaf_Wait;
                    break;

                case Condition<TContext> _:
                    nodeType = TreeNodeType.Leaf_Condition;
                    break;

                case ActionBehaviour<TContext> _:
                    nodeType = TreeNodeType.Leaf_Action;
                    break;

                default:
                    nodeType = TreeNodeType.Leaf;
                    break;
            }
            var nodeInfo = new BehaviourTreeInfo(obj.Name, obj.Id, nodeType)
            {
                NodeTypeSpecific = obj.GetType().FullName ?? obj.GetType().Name,
                Parent = treeInfo,
                Status = obj.Status,
                Depth = depth,
                IsExpanded = true
            };

            if (treeInfo == null) treeInfo = nodeInfo;
            else treeInfo.ChildrenTreeInfos.Add(nodeInfo);
        }
    }

    /// <summary>
    /// Extension methods for querying behavior tree nodes using LINQ-style methods.
    /// </summary>
    public static partial class BehaviourTree
    {
        /// <summary>
        /// Returns the first behavior node that matches the specified predicate.
        /// </summary>
        /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
        /// <param name="source">Root behavior node to search from</param>
        /// <param name="predicate">Function to test each node</param>
        /// <returns>First matching behavior node</returns>
        /// <exception cref="InvalidOperationException">Thrown when no matching node is found</exception>
        public static IBehaviour<TContext> First<TContext>(this IBehaviour<TContext> source, Func<IBehaviour<TContext>, bool> predicate)
        {
            var first = source.TryGetFirst(predicate, out var found);
            if (!found)
            {
                throw new InvalidOperationException("NoElements");
            }

            return first;
        }

        /// <summary>
        /// Returns the first behavior node that matches the specified predicate, or null if none found.
        /// </summary>
        /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
        /// <param name="source">Root behavior node to search from</param>
        /// <param name="predicate">Function to test each node</param>
        /// <returns>First matching behavior node, or null if none found</returns>
        public static IBehaviour<TContext> FirstOrDefault<TContext>(this IBehaviour<TContext> source, Func<IBehaviour<TContext>, bool> predicate)
        {
            return source.TryGetFirst(predicate, out _);
        }

        private static IBehaviour<TContext> TryGetFirst<TContext>(this IBehaviour<TContext> source,
            Func<IBehaviour<TContext>, bool> predicate, out bool found)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate(source))
            {
                found = true;
                return source;
            }

            switch (source)
            {
                case CompositeBehaviour<TContext> composite:
                    foreach (var child in composite.Children)
                    {
                        var foundValue = child.TryGetFirst(predicate, out found);
                        if (found) return foundValue;
                    }
                    break;

                case DecoratorBehaviour<TContext> decorate:
                    return decorate.Child.TryGetFirst(predicate, out found);
            }

            found = false;
            return default!;
        }
    }
}