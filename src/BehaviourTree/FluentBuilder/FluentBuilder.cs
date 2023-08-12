using System;
using System.Collections.Generic;
using System.Data;

namespace BehaviourTree.FluentBuilder
{
    public static class FluentBuilder
    {
        public static FluentBuilder<T> Create<T>()
        {
            return new FluentBuilder<T>();
        }
    }

    public sealed class FluentBuilder<TContext>
    {
        private readonly Stack<BehaviourBuilder<TContext>> _parentNodeStack = new Stack<BehaviourBuilder<TContext>>();
        private BehaviourBuilder<TContext> _currentBehaviourBuilder;


        public FluentBuilder<TContext> End()
        {
            _currentBehaviourBuilder = _parentNodeStack.Pop();
            return this;
        }

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

        public FluentBuilder<TContext> PushLeaf(CreateBehaviour<TContext> behaviourFactory)
        {
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
