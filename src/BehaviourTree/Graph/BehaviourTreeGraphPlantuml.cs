using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using System;
using System.Text;

namespace BehaviourTree.Graph
{
    /// <summary>
    /// Generates behavior tree visualizations in PlantUML mindmap format.
    /// See: https://plantuml.com/mindmap-diagram
    /// </summary>
    public static class BehaviourTreeGraphPlantuml
    {
        /// <summary>
        /// Generates a PlantUML mindmap diagram from a behavior tree.
        /// See: https://plantuml.com/mindmap-diagram
        /// </summary>
        /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
        /// <param name="behaviour">Behavior tree to format</param>
        /// <returns>PlantUML mindmap markup string</returns>
        public static string Format<TContext>(IBehaviour<TContext> behaviour)
        {
            StringBuilder formatted = new StringBuilder();
            RenderBehaviourTree(formatted, 0, behaviour);
            formatted.Insert(0, "@startmindmap\n");
            formatted.AppendLine("@endmindmap");
            return formatted.ToString();
        }

        private static void RenderBehaviourTree<TContext>(StringBuilder text, int depth, IBehaviour<TContext> behaviour)
        {
            // Use pattern matching instead of dynamic dispatch for better performance
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
            var marksign = GetMarksign(obj);
            var name = GetName(obj);
            var nodeExpression = $"{indentation} **{marksign}** //{name}'{obj.Id}'//";
            text.AppendLine(nodeExpression);
        }

        private static string GetIndentation(int depth)
        {
            // OPTIMIZATION: Use string constructor instead of LINQ for better performance
            return new string('*', depth + 1);
        }

        private static string GetMarksign<TContext>(IBehaviour<TContext> obj)
        {
            // Use pattern matching with C# switch expression
            return obj switch
            {
                // Active (Reactive) nodes
                ActiveSelector<TContext> => "[?A]",
                ActiveSequence<TContext> => "[->A]",

                // Random nodes
                RandomSelector<TContext> => "[?R]",
                RandomSequence<TContext> => "[->R]",

                // Priority nodes
                PrioritySelector<TContext> => "[?P]",
                PrioritySequence<TContext> => "[->P]",

                // Standard composites
                Selector<TContext> => "[?]",
                Sequence<TContext> => "[->]",

                // Parallel nodes
                Parallel<TContext> parallel => $"[={parallel.SuccessRequired}/{parallel.Children.Length}]",
                SimpleParallel<TContext> => "[=2]",

                // Leaf nodes
                Condition<TContext> => "(?)",
                ActionBehaviour<TContext> => "(!)",
                AsyncAction<TContext> => "(!A)",

                // Wait nodes
                Wait<TContext> => "(~)",
                WaitRenew<TContext> => "(~R)",

                // Decorator nodes - delegate to GetDecoratorSymbol
                DecoratorBehaviour<TContext> => $"<{GetDecoratorSymbol(obj)}>",

                // Fallback for unknown types - use Source Generator metadata
                _ => $"[{(obj is IBehaviourMetadata metadata ? metadata.TypeName : obj.GetType().Name)}]"
            };
        }

        private static string GetDecoratorSymbol<TContext>(IBehaviour<TContext> obj)
        {
            // OPTIMIZATION: Use Source Generator metadata to avoid reflection
            var typeName = (obj is IBehaviourMetadata metadata) ? metadata.TypeName : obj.GetType().Name;

            // Handle types with IClock constraint using runtime type checking
            if (typeName.StartsWith("TimeLimiter")) return "TL";
            if (typeName.StartsWith("RateLimiter")) return "RL";
            if (typeName.StartsWith("UntilSuccessWithinTimeout")) return "UST";

            return obj switch
            {
                // Retry/Repeat
                Retry<TContext> retry => $"Retry:{retry.RetryCount}",
                Repeater<TContext> repeater => $"Repeat:{repeater.RepeatCount}",

                // Logic inverters/transformers
                Inverter<TContext> => "!",
                Succeeder<TContext> => "✓",
                Failer<TContext> => "✗",

                // Time-based decorators
                Cooldown<TContext> cooldown => $"CD:{cooldown.CooldownTimeInMilliseconds}ms",
                CooldownRenew<TContext> => "CD:R",

                // Until decorators
                UntilSuccess<TContext> => "US",
                UntilFailed<TContext> => "UF",

                // After decorators
                AfterSuccess<TContext> => "→S",
                AfterFailed<TContext> => "→F",

                // Other decorators
                AutoReset<TContext> => "AR",
                Random<TContext> random => $"Rnd:{random.Threshold:F2}",

                // Fallback
                _ => typeName
            };
        }

        private static string GetName<TContext>(IBehaviour<TContext> obj)
        {
            if (!string.IsNullOrWhiteSpace(obj.Name))
            {
                return obj.Name;
            }

            // OPTIMIZATION: Use Source Generator metadata to avoid reflection
            if (obj is IBehaviourMetadata metadata)
            {
                return metadata.TypeName;
            }

            return obj.GetType().Name;
        }
    }
}
