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
        /// Generates a behavior tree in Dan Abad's text format.
        /// See: https://github.com/0xabad/behavior_tree/
        /// Note: This format doesn't fully support decorators.
        /// </summary>
        public static string Format<TContext>(IBehaviour<TContext> behaviour)
        {
            StringBuilder formatted = new StringBuilder();
            RenderBehaviourTree(formatted, 0, behaviour);
            return formatted.ToString();
        }

        private static void RenderBehaviourTree<TContext>(StringBuilder text, int depth, IBehaviour<TContext> behaviour)
        {
            // Use pattern matching instead of dynamic dispatch
            switch (behaviour)
            {
                case CompositeBehaviour<TContext> composite:
                    RenderComposite(text, depth, composite);
                    break;

                case DecoratorBehaviour<TContext> decorator:
                    RenderDecorator(text, depth, decorator);
                    break;

                default:
                    RenderLeaf(text, depth, behaviour);
                    break;
            }
        }

        private static void RenderComposite<TContext>(StringBuilder text, int depth, CompositeBehaviour<TContext> obj)
        {
            RenderInternal(text, depth, obj);

            var childDepth = depth + 1;
            foreach (var child in obj.Children)
            {
                RenderBehaviourTree(text, childDepth, child);
            }
        }

        private static void RenderDecorator<TContext>(StringBuilder text, int depth, DecoratorBehaviour<TContext> obj)
        {
            RenderInternal(text, depth, obj);
            RenderBehaviourTree(text, depth + 1, obj.Child);
        }

        private static void RenderLeaf<TContext>(StringBuilder text, int depth, IBehaviour<TContext> obj)
        {
            RenderInternal(text, depth, obj);
        }

        private static void RenderInternal<TContext>(StringBuilder text, int depth, IBehaviour<TContext> obj)
        {
            var indentation = GetIndentation(depth);
            var name = GetMarksign(obj);
            var nodeExpression = $"{indentation}{name}";
            text.AppendLine(nodeExpression);
        }

        private static string GetIndentation(int depth)
        {
            return string.Join(string.Empty, Enumerable.Repeat("|    ", depth));
        }

        private static string GetMarksign<TContext>(IBehaviour<TContext> obj)
        {
            // Use pattern matching with C# switch expression
            return obj switch
            {
                // Selector family (all rendered as "?")
                ActiveSelector<TContext> => "?",
                RandomSelector<TContext> => "?",
                PrioritySelector<TContext> => "?",
                Selector<TContext> => "?",

                // Sequence family (all rendered as "->")
                ActiveSequence<TContext> => "->",
                RandomSequence<TContext> => "->",
                PrioritySequence<TContext> => "->",
                Sequence<TContext> => "->",

                // Parallel nodes
                Parallel<TContext> parallel => $"={parallel.SuccessRequired}",
                SimpleParallel<TContext> simple => $"={simple.Children.Length}",

                // Leaf nodes
                Condition<TContext> => $"({GetName(obj)})",
                ActionBehaviour<TContext> => $"[{GetName(obj)}]",
                AsyncAction<TContext> => $"[{GetName(obj)}:async]",
                Wait<TContext> => $"[{GetName(obj)}:wait]",
                WaitRenew<TContext> => $"[{GetName(obj)}:wait]",

                // Decorator nodes (0xabad format doesn't fully support these)
                DecoratorBehaviour<TContext> => $"?  // {GetDecoratorInfo(obj)}",

                // Fallback
                _ => $"[{obj.GetType().Name}]"
            };
        }

        private static string GetDecoratorInfo<TContext>(IBehaviour<TContext> obj)
        {
            return obj switch
            {
                Retry<TContext> retry => $"Retry({retry.RetryCount})",
                Repeater<TContext> repeater => $"Repeat({repeater.RepeatCount})",
                Inverter<TContext> => "Invert",
                Cooldown<TContext> cooldown => $"Cooldown({cooldown.CooldownTimeInMilliseconds}ms)",
                CooldownRenew<TContext> => "Cooldown(dynamic)",
                TimeLimiter<_> => "TimeLimit",
                RateLimiter<_> => "RateLimit",
                UntilSuccess<TContext> => "UntilSuccess",
                UntilFailed<TContext> => "UntilFailed",
                Succeeder<TContext> => "AlwaysSucceed",
                Failer<TContext> => "AlwaysFail",
                AutoReset<TContext> => "AutoReset",
                AfterSuccess<TContext> => "AfterSuccess",
                AfterFailed<TContext> => "AfterFailed",
                Random<TContext> random => $"Random({random.Threshold:F2})",
                _ => obj.GetType().Name
            };
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