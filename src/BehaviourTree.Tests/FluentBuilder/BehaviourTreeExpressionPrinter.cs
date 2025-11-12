using System;
using System.Linq;
using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;

namespace BehaviourTree.Tests.FluentBuilder
{
    public sealed class BehaviourTreeExpressionPrinter<TContext>
        where TContext : IClock
    {
        public static string GetExpression(IBehaviour<TContext> obj)
        {
            return GetExpression(obj, 0);
        }

        private static string GetExpression(IBehaviour<TContext> obj, int depth)
        {
            return obj switch
            {
                // Specific decorators with parameters
                Wait<TContext> wait => InternalGetExpression(wait, depth, wait.WaitTimeInMilliseconds),
                Cooldown<TContext> cooldown => InternalGetExpression(cooldown, depth, cooldown.CooldownTimeInMilliseconds) + GetExpression(cooldown.Child, depth + 1),
                RateLimiter<TContext> rateLimiter => InternalGetExpression(rateLimiter, depth, rateLimiter.IntervalInMilliseconds) + GetExpression(rateLimiter.Child, depth + 1),
                Repeater<TContext> repeater => InternalGetExpression(repeater, depth, repeater.RepeatCount) + GetExpression(repeater.Child, depth + 1),
                Random<TContext> random => InternalGetExpression(random, depth, random.Threshold) + GetExpression(random.Child, depth + 1),
                TimeLimiter<TContext> timeLimiter => InternalGetExpression(timeLimiter, depth, timeLimiter.TimeLimitInMilliseconds) + GetExpression(timeLimiter.Child, depth + 1),

                // Generic composite behaviour
                CompositeBehaviour<TContext> composite => GetCompositeExpression(composite, depth),

                // Generic decorator behaviour (not already handled above)
                DecoratorBehaviour<TContext> decorator => InternalGetExpression(decorator, depth) + GetExpression(decorator.Child, depth + 1),

                // Base behaviour (leaf nodes)
                BaseBehaviour<TContext> baseBehaviour => InternalGetExpression(baseBehaviour, depth),

                _ => InternalGetExpression(obj, depth)
            };
        }

        private static string GetCompositeExpression(CompositeBehaviour<TContext> obj, int depth)
        {
            var expression = InternalGetExpression(obj, depth);
            var childDepth = depth + 1;

            foreach (var child in obj.Children)
            {
                expression += GetExpression(child, childDepth);
            }

            return expression;
        }

        private static string InternalGetExpression(IBehaviour<TContext> obj, int depth, params object[] parameters)
        {
            var paramsExpression = parameters.Any() ? $"({string.Join(",", parameters)})" : string.Empty;
            return  $"{GetIndentation(depth)}{GetName(obj)} {paramsExpression}{Environment.NewLine}";
        }

        private static string GetIndentation(int depth)
        {
            return string.Join(string.Empty, Enumerable.Repeat("   ", depth));
        }

        private static string GetName(IBehaviour<TContext> obj)
        {
            if (!string.IsNullOrWhiteSpace(obj.Name))
            {
                return obj.Name;
            }

            // OPTIMIZATION: Use Source Generator metadata to avoid reflection
            if (obj is IBehaviourMetadata metadata)
            {
                var typeName = metadata.TypeName;

                // Handle generic types by removing backtick and type parameters
                if (metadata.IsGenericType)
                {
                    var backtickIndex = typeName.IndexOf('`');
                    return backtickIndex > 0 ? typeName.Substring(0, backtickIndex) : typeName;
                }

                return typeName;
            }

            // Fallback to reflection (shouldn't happen in normal usage)
            var type = obj.GetType();

            // Handle generic types by removing backtick and type parameters
            if (type.IsGenericType)
            {
                var name = type.Name;
                var backtickIndex = name.IndexOf('`');
                return backtickIndex > 0 ? name.Substring(0, backtickIndex) : name;
            }

            return type.Name;
        }
    }
}
