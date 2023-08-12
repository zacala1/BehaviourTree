using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using System;
using System.Linq;
using System.Text;

namespace BehaviourTree.Graph
{
    public static class BehaviourTreeGraphPlantuml
    {
        /// <summary>
        /// https://plantuml.com/mindmap-diagram
        /// </summary>
        /// <returns></returns>
        public static string Format<TContext>(IBehaviour<TContext> behaviour) where TContext : IClock
        {
            StringBuilder formated = new StringBuilder();
            RenderBehaviourTree(formated, 0, behaviour);
            formated.Insert(0, "@startmindmap\n");
            formated.AppendLine("@endmindmap");
            return formated.ToString();
        }

        private static void RenderBehaviourTree<TContext>(StringBuilder text, int depth, IBehaviour<TContext> behaviour) where TContext : IClock
        {
            RenderBehaviourTree(text, depth, (dynamic)behaviour);
        }

        private static void RenderBehaviourTree<TContext>(StringBuilder text, int depth, CompositeBehaviour<TContext> obj) where TContext : IClock
        {
            RenderInternal(text, depth, obj);

            var childDepth = depth + 1;

            foreach (var child in obj.Children)
            {
                RenderBehaviourTree(text, childDepth, child);
            }
        }

        private static void RenderBehaviourTree<TContext>(StringBuilder text, int depth, DecoratorBehaviour<TContext> obj) where TContext : IClock
        {
            RenderInternal(text, depth, obj);
            RenderBehaviourTree(text, ++depth, obj.Child);
        }

        private static void RenderBehaviourTree<TContext>(StringBuilder text, int depth, BaseBehaviour<TContext> obj) where TContext : IClock
        {
            RenderInternal(text, depth, obj);
        }

        private static void RenderInternal<TContext>(StringBuilder text, int depth, IBehaviour<TContext> obj) where TContext : IClock
        {
            var indentation = GetIndentation(depth);
            var marksign = GetMarksign(obj);
            var name = GetName(obj);
            //var color = GetColor(obj.Status);
            var nodeExpression = $"{indentation} **{marksign}** //{name}'{obj.Id}'//";
            text.AppendLine(nodeExpression);
        }

        private static string GetIndentation(int depth)
        {
            return string.Join(string.Empty, Enumerable.Repeat("*", depth + 1));
        }

        private static string GetMarksign<TContext>(IBehaviour<TContext> obj) where TContext : IClock
        {
            switch (obj)
            {
                case RandomSelector<TContext> _:
                    return "[?r]";

                case PrioritySelector<TContext> _:
                    return "[?p]";

                case Selector<TContext> _:
                    return "[?]";

                case RandomSequence<TContext> _:
                    return "[->r]";

                case PrioritySequence<TContext> _:
                    return "[->p]";

                case Sequence<TContext> _:
                    return "[->]";

                case SimpleParallel<TContext> _:
                    return "[=]";

                case Condition<TContext> _:
                    return "(?)";

                case ActionBehaviour<TContext> _:
                    return "(!)";

                case Wait<TContext> _:
                case WaitRenew<TContext> _:
                    return "(~)";

                case DecoratorBehaviour<TContext> _:
                    return $"<{obj.GetType().Name}>";

                default:
                    throw new NotSupportedException($"Node Type {obj.GetType().FullName} NotSupportedException");
            }
        }

        private static string GetName<TContext>(IBehaviour<TContext> obj)
        {
            if (!string.IsNullOrWhiteSpace(obj.Name))
            {
                return obj.Name;
            }

            var type = obj.GetType();

            // TODO: check for generic

            return type.Name;
        }
    }
}