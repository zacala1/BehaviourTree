using System;
using System.Collections.Generic;
using System.Data;

namespace BehaviourTree.FluentBuilder
{
    /// <summary>
    /// Static factory class for creating fluent builder instances.
    /// </summary>
    public static class FluentBuilder
    {
        /// <summary>
        /// Creates a new fluent builder for the specified context type.
        /// </summary>
        /// <typeparam name="T">Context type used in the behavior tree</typeparam>
        /// <returns>New fluent builder instance</returns>
        public static FluentBuilder<T> Create<T>()
        {
            return new FluentBuilder<T>();
        }
    }

    /// <summary>
    /// Fluent builder for constructing behavior trees using a stack-based approach.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public sealed class FluentBuilder<TContext>
    {
        private readonly Stack<BehaviourBuilder<TContext>> _parentNodeStack = new Stack<BehaviourBuilder<TContext>>();
        private BehaviourBuilder<TContext> _currentBehaviourBuilder;

        /// <summary>
        /// Ends the current composite or decorator node and returns to its parent.
        /// </summary>
        /// <returns>This builder instance for method chaining</returns>
        public FluentBuilder<TContext> End()
        {
            _currentBehaviourBuilder = _parentNodeStack.Pop();
            return this;
        }

        /// <summary>
        /// Pushes a new composite node onto the builder stack.
        /// </summary>
        /// <param name="behaviourFactory">Factory function to create the composite behavior</param>
        /// <returns>This builder instance for method chaining</returns>
        public FluentBuilder<TContext> PushComposite(CreateCompositeBehaviour<TContext> behaviourFactory)
        {
            var newNode = new CompositeBehaviourBuilder<TContext>
            {
                Factory = behaviourFactory
            };

            if (_parentNodeStack.Count > 0)
            {
                var parentNode = _parentNodeStack.Peek();
                InternalAddChild(parentNode, newNode);
            }

            _parentNodeStack.Push(newNode);

            return this;
        }

        /// <summary>
        /// Pushes a new decorator node onto the builder stack.
        /// </summary>
        /// <param name="behaviourFactory">Factory function to create the decorator behavior</param>
        /// <returns>This builder instance for method chaining</returns>
        public FluentBuilder<TContext> PushDecorate(CreateDecorateBehaviour<TContext> behaviourFactory)
        {
            var newNode = new DecorateBehaviourBuilder<TContext>
            {
                Factory = behaviourFactory
            };

            if (_parentNodeStack.Count > 0)
            {
                var parentNode = _parentNodeStack.Peek();
                InternalAddChild(parentNode, newNode);
            }

            _parentNodeStack.Push(newNode);

            return this;
        }

        /// <summary>
        /// Adds a leaf node as a child of the current composite or decorator.
        /// </summary>
        /// <param name="behaviourFactory">Factory function to create the leaf behavior</param>
        /// <returns>This builder instance for method chaining</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to add a leaf without a parent</exception>
        public FluentBuilder<TContext> PushLeaf(CreateBehaviour<TContext> behaviourFactory)
        {
            if (_parentNodeStack.Count == 0)
            {
                throw new InvalidOperationException(
                    "Leaf nodes must have a parent composite or decorator. " +
                    "Start with a composite node (e.g., Sequence, Selector) or decorator before adding leaf nodes.");
            }

            var newNode = new LeafBehaviourBuilder<TContext>
            {
                Factory = behaviourFactory
            };

            var parentNode = _parentNodeStack.Peek();
            InternalAddChild(parentNode, newNode);

            return this;
        }

        private static void InternalAddChild(BehaviourBuilder<TContext> parent, BehaviourBuilder<TContext> child)
        {
            switch (parent)
            {
                case CompositeBehaviourBuilder<TContext> composite:
                    composite.Children.Add(child);
                    break;

                case DecorateBehaviourBuilder<TContext> decorate:
                    decorate.Child = child;
                    break;

                default:
                    throw new InvalidCastException("Parent must be a composite or decorate node");
            }
        }

        /// <summary>
        /// Builds the complete behavior tree from the constructed node hierarchy.
        /// </summary>
        /// <returns>Root behavior node of the tree</returns>
        /// <exception cref="InvalidOperationException">Thrown when tree is empty</exception>
        /// <exception cref="InvalidExpressionException">Thrown when End() calls are missing</exception>
        public IBehaviour<TContext> Build()
        {
            if (_currentBehaviourBuilder == null)
            {
                throw new InvalidOperationException("Tree must contain at least one node");
            }

            if (_parentNodeStack.Count != 0)
            {
                throw new InvalidExpressionException("Node stack remains. Please, check fluent syntax end");
            }

            return _currentBehaviourBuilder.Build();
        }
    }
}