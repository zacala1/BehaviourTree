using System.Collections.Generic;
using System.Linq;

namespace BehaviourTree.FluentBuilder
{
    /// <summary>
    /// Builder for composite behavior nodes with multiple children.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public sealed class CompositeBehaviourBuilder<TContext> : BehaviourBuilder<TContext>
    {
        /// <summary>
        /// Creates a new composite behavior builder.
        /// </summary>
        public CompositeBehaviourBuilder()
        {
            Children = new List<BehaviourBuilder<TContext>>();
        }

        /// <summary>
        /// Gets or sets the factory function for creating the composite behavior.
        /// </summary>
        public CreateCompositeBehaviour<TContext> Factory { get; set; }

        /// <summary>
        /// Gets the list of child behavior builders.
        /// </summary>
        public IList<BehaviourBuilder<TContext>> Children { get; }

        /// <summary>
        /// Builds the composite behavior by building all children and passing them to the factory.
        /// </summary>
        /// <returns>Built composite behavior instance</returns>
        public override IBehaviour<TContext> Build()
        {
            var behaviours = Children
                .Select(x => x.Build())
                .ToArray();

            return Factory(behaviours);
        }
    }
}