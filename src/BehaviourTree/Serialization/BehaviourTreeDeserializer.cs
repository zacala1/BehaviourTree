using System;
using System.Collections.Generic;
using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;

namespace BehaviourTree.Serialization
{
    /// <summary>
    /// Deserializes a behavior tree from a simplified JSON-like node descriptor.
    /// Uses a registry of named action/condition delegates for leaf node binding.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public sealed class BehaviourTreeDeserializer<TContext>
    {
        private readonly Dictionary<string, Func<TContext, BehaviourStatus>> _actions =
            new Dictionary<string, Func<TContext, BehaviourStatus>>(32);
        private readonly Dictionary<string, Func<TContext, bool>> _conditions =
            new Dictionary<string, Func<TContext, bool>>(16);

        /// <summary>
        /// Registers a named action function that can be referenced from tree definitions.
        /// </summary>
        public BehaviourTreeDeserializer<TContext> RegisterAction(string name, Func<TContext, BehaviourStatus> action)
        {
            _actions[name ?? throw new ArgumentNullException(nameof(name))] =
                action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        /// <summary>
        /// Registers a named condition function that can be referenced from tree definitions.
        /// </summary>
        public BehaviourTreeDeserializer<TContext> RegisterCondition(string name, Func<TContext, bool> condition)
        {
            _conditions[name ?? throw new ArgumentNullException(nameof(name))] =
                condition ?? throw new ArgumentNullException(nameof(condition));
            return this;
        }

        /// <summary>
        /// Builds a behavior tree from a node descriptor hierarchy.
        /// </summary>
        /// <param name="descriptor">Root node descriptor</param>
        /// <returns>The constructed behavior tree root</returns>
        public IBehaviour<TContext> Build(NodeDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            return BuildNode(descriptor);
        }

        private IBehaviour<TContext> BuildNode(NodeDescriptor desc)
        {
            switch (desc.Category?.ToLowerInvariant())
            {
                case "composite":
                    return BuildComposite(desc);
                case "decorator":
                    return BuildDecorator(desc);
                case "leaf":
                default:
                    return BuildLeaf(desc);
            }
        }

        private IBehaviour<TContext> BuildComposite(NodeDescriptor desc)
        {
            if (desc.Children == null || desc.Children.Length == 0)
                throw new InvalidOperationException($"Composite '{desc.Name}' must have children");

            var children = new IBehaviour<TContext>[desc.Children.Length];
            for (int i = 0; i < desc.Children.Length; i++)
            {
                children[i] = BuildNode(desc.Children[i]);
            }

            var name = desc.Name ?? desc.Type;
            switch (desc.Type?.ToLowerInvariant())
            {
                case "sequence":
                    return new Sequence<TContext>(name, children);
                case "selector":
                    return new Selector<TContext>(name, children);
                case "priorityselector":
                    return new PrioritySelector<TContext>(name, children);
                case "prioritysequence":
                    return new PrioritySequence<TContext>(name, children);
                case "parallel":
                    return new Parallel<TContext>(name, ParallelPolicy.RequireAll, children);
                default:
                    throw new InvalidOperationException($"Unknown composite type: '{desc.Type}'");
            }
        }

        private IBehaviour<TContext> BuildDecorator(NodeDescriptor desc)
        {
            if (desc.Children == null || desc.Children.Length != 1)
                throw new InvalidOperationException($"Decorator '{desc.Name}' must have exactly one child");

            var child = BuildNode(desc.Children[0]);
            var name = desc.Name ?? desc.Type;

            switch (desc.Type?.ToLowerInvariant())
            {
                case "inverter":
                    return new Inverter<TContext>(name, child);
                case "succeeder":
                    return new Succeeder<TContext>(name, child);
                case "failer":
                    return new Failer<TContext>(name, child);
                case "retry":
                    var retryCount = desc.Count > 0 ? desc.Count : 3;
                    return new Retry<TContext>(name, child, retryCount);
                case "repeater":
                    var repeatCount = desc.Count > 0 ? desc.Count : 1;
                    return new Repeater<TContext>(name, child, repeatCount);
                case "untilsuccess":
                    return new UntilSuccess<TContext>(name, child, desc.Count);
                case "untilfailed":
                    return new UntilFailed<TContext>(name, child, desc.Count);
                case "guard":
                    if (desc.ConditionRef == null)
                        throw new InvalidOperationException($"Guard '{name}' requires a conditionRef");
                    if (!_conditions.TryGetValue(desc.ConditionRef, out var guardCondition))
                        throw new InvalidOperationException($"Condition '{desc.ConditionRef}' not registered");
                    return new Guard<TContext>(name, child, guardCondition);
                case "subtree":
                    return new SubTree<TContext>(name, child);
                default:
                    throw new InvalidOperationException($"Unknown decorator type: '{desc.Type}'");
            }
        }

        private IBehaviour<TContext> BuildLeaf(NodeDescriptor desc)
        {
            var name = desc.Name ?? desc.Type;

            switch (desc.Type?.ToLowerInvariant())
            {
                case "action":
                    if (desc.ActionRef == null)
                        throw new InvalidOperationException($"Action '{name}' requires an actionRef");
                    if (!_actions.TryGetValue(desc.ActionRef, out var action))
                        throw new InvalidOperationException($"Action '{desc.ActionRef}' not registered");
                    return new ActionBehaviour<TContext>(name, action);

                case "condition":
                    if (desc.ConditionRef == null)
                        throw new InvalidOperationException($"Condition '{name}' requires a conditionRef");
                    if (!_conditions.TryGetValue(desc.ConditionRef, out var condition))
                        throw new InvalidOperationException($"Condition '{desc.ConditionRef}' not registered");
                    return new Condition<TContext>(name, condition);

                case "wait":
                    return new Wait<TContext>(name, desc.Count > 0 ? desc.Count : 1000);

                default:
                    // Try action registry by type name
                    if (desc.Type != null && _actions.TryGetValue(desc.Type, out var typeAction))
                        return new ActionBehaviour<TContext>(name, typeAction);
                    throw new InvalidOperationException($"Unknown leaf type: '{desc.Type}'");
            }
        }
    }

    /// <summary>
    /// Describes a node in a behavior tree for deserialization.
    /// Can be constructed manually or parsed from JSON.
    /// </summary>
    public sealed class NodeDescriptor
    {
        /// <summary>Node type (e.g., "Sequence", "Selector", "Action", "Condition")</summary>
        public string Type { get; set; } = "";

        /// <summary>Display name for debugging</summary>
        public string? Name { get; set; }

        /// <summary>Category: "composite", "decorator", or "leaf"</summary>
        public string? Category { get; set; }

        /// <summary>Child node descriptors</summary>
        public NodeDescriptor[]? Children { get; set; }

        /// <summary>Reference to a registered action function name</summary>
        public string? ActionRef { get; set; }

        /// <summary>Reference to a registered condition function name</summary>
        public string? ConditionRef { get; set; }

        /// <summary>Count parameter (for Retry, Repeater, Wait, etc.)</summary>
        public int Count { get; set; }
    }
}
