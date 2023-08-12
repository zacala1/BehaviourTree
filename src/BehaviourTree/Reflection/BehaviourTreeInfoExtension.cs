using System;
using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;

namespace BehaviourTree.Reflection
{
    public static class BehaviourTreeInfoExtension
    {
        public static BehaviourTreeInfo GetInfo<TContext>(this IBehaviour<TContext> behaviour)
        {
            BehaviourTreeInfo treeInfo = null;
            InternalGetInfos(ref treeInfo, 0, behaviour);
            return treeInfo;
        }

        private static void InternalGetInfos<TContext>(ref BehaviourTreeInfo treeInfo, int depth, IBehaviour<TContext> child)
        {
            InternalGetInfos(ref treeInfo, depth, (dynamic)child);
        }

        private static void InternalGetInfos<TContext>(ref BehaviourTreeInfo treeInfo, int depth, CompositeBehaviour<TContext> obj)
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
                NodeTypeSpecific = obj.GetType().FullName,
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

        private static void InternalGetInfos<TContext>(ref BehaviourTreeInfo treeInfo, int depth, DecoratorBehaviour<TContext> obj)
        {
            var nodeInfo = new BehaviourTreeInfo(obj.Name, obj.Id, TreeNodeType.Decorate)
            {
                NodeTypeSpecific = obj.GetType().FullName,
                Parent = treeInfo,
                Status = obj.Status,
                Depth = depth,
                IsExpanded = true
            };

            if (treeInfo == null) treeInfo = nodeInfo;
            else treeInfo.ChildrenTreeInfos.Add(nodeInfo);

            InternalGetInfos(ref nodeInfo, ++depth, obj.Child);
        }

        private static void InternalGetInfos<TContext>(ref BehaviourTreeInfo treeInfo, int depth, BaseBehaviour<TContext> obj) where TContext : IClock
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
                NodeTypeSpecific = obj.GetType().FullName,
                Parent = treeInfo,
                Status = obj.Status,
                Depth = depth,
                IsExpanded = true
            };

            if (treeInfo == null) treeInfo = nodeInfo;
            else treeInfo.ChildrenTreeInfos.Add(nodeInfo);
        }
    }

    public static partial class BehaviourTree
    {
        public static IBehaviour<TContext> First<TContext>(this IBehaviour<TContext> source, Func<IBehaviour<TContext>, bool> predicate)
        {
            var first = source.TryGetFirst(predicate, out var found);
            if (!found)
            {
                throw new InvalidOperationException("NoElements");
            }

            return first;
        }

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
            return default;
        }
    }
}