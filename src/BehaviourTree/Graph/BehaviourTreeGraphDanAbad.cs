using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using System;
using System.Linq;
using System.Text;

namespace BehaviourTree.Graph
{
    public static class BehaviourTreeGraphDanAbad
    {
        /// <summary>
        /// https://github.com/0xabad/behavior_tree/
        /// </summary>
        /// <returns></returns>
        public static string Format<TContext>(IBehaviour<TContext> behaviour) where TContext : IClock
        {
            StringBuilder formated = new StringBuilder();
            RenderBehaviourTree(formated, 0, behaviour);
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
            var name = GetMarksign(obj);
            //var color = GetColor(obj.Status);
            var nodeExpression = $"{indentation}{name}";
            text.AppendLine(nodeExpression);
        }

        private static string GetIndentation(int depth)
        {
            return string.Join(string.Empty, Enumerable.Repeat("|    ", depth));
        }

        private static string GetMarksign<TContext>(IBehaviour<TContext> obj) where TContext : IClock
        {
            // 0xabad behavior_tree Syntax가 Decorator는 지원하지 않음
            if (obj is Selector<TContext> ||
                obj is PrioritySelector<TContext> ||
                obj is RandomSelector<TContext>)
            {
                return "?";
            }
            else if (obj is Sequence<TContext> ||
                     obj is PrioritySequence<TContext> ||
                     obj is RandomSequence<TContext>)
            {
                return "->";
            }
            else if (obj is SimpleParallel<TContext> parallelBehaviour)
            {
                return "=" + parallelBehaviour.Children.Length.ToString();
            }
            else if (obj is Condition<TContext>)
            {
                return "(" + GetName(obj) + ")";
            }
            else if (obj is ActionBehaviour<TContext> ||
                     obj is Wait<TContext>)
            {
                return "[" + GetName(obj) + "]";
            }
            else if (obj is DecoratorBehaviour<TContext>)
            {
                return "?";
            }
            else
            {
                throw new NotSupportedException($"NodetypeNotSupportedException{obj.GetType()}");
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
