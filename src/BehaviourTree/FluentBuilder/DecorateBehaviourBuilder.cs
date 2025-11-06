using System;

namespace BehaviourTree.FluentBuilder
{
    /// <summary>
    /// Builder for decorator behavior nodes with a single child.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public sealed class DecorateBehaviourBuilder<TContext> : BehaviourBuilder<TContext>
    {
        /// <summary>
        /// Creates a new decorator behavior builder.
        /// </summary>
        public DecorateBehaviourBuilder()
        {
        }

        /// <summary>
        /// Gets or sets the factory function for creating the decorator behavior.
        /// </summary>
        public CreateDecorateBehaviour<TContext> Factory { get; set; }

        private BehaviourBuilder<TContext> _child;

        /// <summary>
        /// Gets or sets the child behavior builder.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when attempting to set a child when one already exists</exception>
        public BehaviourBuilder<TContext> Child
        {
            get { return _child; }
            set
            {
                if (_child != null) throw new ArgumentException("Must have only one child", nameof(value));
                _child = value;
            }
        }

        /// <summary>
        /// Builds the decorator behavior by building the child and passing it to the factory.
        /// </summary>
        /// <returns>Built decorator behavior instance</returns>
        public override IBehaviour<TContext> Build()
        {
            var behaviours = Child?.Build();

            return Factory(behaviours);
        }
    }
}