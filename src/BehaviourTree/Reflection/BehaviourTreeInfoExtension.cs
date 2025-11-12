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
            switch (child)
            {
                case CompositeBehaviour<TContext> composite:
                    TreeNodeType compositeNodeType = composite switch
                    {
                        SimpleParallel<TContext> => TreeNodeType.Composite_Parallel,
                        PrioritySelector<TContext> or RandomSelector<TContext> or Selector<TContext> => TreeNodeType.Composite_Selector,
                        PrioritySequence<TContext> or RandomSequence<TContext> or Sequence<TContext> => TreeNodeType.Composite_Sequence,
                        _ => TreeNodeType.Composite
                    };

                    var compositeNodeInfo = new BehaviourTreeInfo(composite.Name, composite.Id, compositeNodeType)
                    {
                        NodeTypeSpecific = GetTypeFullNameOrName(composite),
                        Parent = treeInfo,
                        Status = composite.Status,
                        Depth = depth,
                        IsExpanded = true
                    };

                    if (treeInfo == null) treeInfo = compositeNodeInfo;
                    else treeInfo.ChildrenTreeInfos.Add(compositeNodeInfo);

                    var childDepth = depth + 1;
                    foreach (var compositeChild in composite.Children)
                    {
                        InternalGetInfos(ref compositeNodeInfo, childDepth, compositeChild);
                    }
                    break;

                case DecoratorBehaviour<TContext> decorator:
                    var decoratorNodeInfo = new BehaviourTreeInfo(decorator.Name, decorator.Id, TreeNodeType.Decorate)
                    {
                        NodeTypeSpecific = GetTypeFullNameOrName(decorator),
                        Parent = treeInfo,
                        Status = decorator.Status,
                        Depth = depth,
                        IsExpanded = true
                    };

                    if (treeInfo == null) treeInfo = decoratorNodeInfo;
                    else treeInfo.ChildrenTreeInfos.Add(decoratorNodeInfo);

                    InternalGetInfos(ref decoratorNodeInfo, depth + 1, decorator.Child);
                    break;

                case BaseBehaviour<TContext> baseBehaviour:
                    TreeNodeType leafNodeType = baseBehaviour switch
                    {
                        Wait<TContext> => TreeNodeType.Leaf_Wait,
                        Condition<TContext> => TreeNodeType.Leaf_Condition,
                        ActionBehaviour<TContext> => TreeNodeType.Leaf_Action,
                        _ => TreeNodeType.Leaf
                    };

                    var leafNodeInfo = new BehaviourTreeInfo(baseBehaviour.Name, baseBehaviour.Id, leafNodeType)
                    {
                        NodeTypeSpecific = GetTypeFullNameOrName(baseBehaviour),
                        Parent = treeInfo,
                        Status = baseBehaviour.Status,
                        Depth = depth,
                        IsExpanded = true
                    };

                    if (treeInfo == null) treeInfo = leafNodeInfo;
                    else treeInfo.ChildrenTreeInfos.Add(leafNodeInfo);
                    break;
            }
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

        /// <summary>
        /// Gets the full type name using Source Generator metadata, falling back to reflection if needed.
        /// </summary>
        private static string GetTypeFullNameOrName<TContext>(IBehaviour<TContext> behaviour)
        {
            // OPTIMIZATION: Use Source Generator metadata to avoid reflection
            if (behaviour is IBehaviourMetadata metadata)
            {
                return metadata.FullTypeName ?? metadata.TypeName;
            }

            // Fallback to reflection (shouldn't happen in normal usage)
            return behaviour.GetType().FullName ?? behaviour.GetType().Name;
        }
    }
}