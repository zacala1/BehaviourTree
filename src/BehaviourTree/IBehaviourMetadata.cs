using System;

namespace BehaviourTree
{
    /// <summary>
    /// Provides compile-time metadata about behavior tree nodes, eliminating reflection overhead.
    /// This interface is implemented via Source Generator for all IBehaviour types.
    /// </summary>
    public interface IBehaviourMetadata
    {
        /// <summary>
        /// Gets the simple type name (e.g., "Selector", "Sequence").
        /// Replaces GetType().Name reflection calls.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Gets the full type name including namespace (e.g., "BehaviourTree.Composites.Selector").
        /// Replaces GetType().FullName reflection calls.
        /// </summary>
        string FullTypeName { get; }

        /// <summary>
        /// Gets whether this type is a generic type.
        /// Replaces GetType().IsGenericType reflection calls.
        /// </summary>
        bool IsGenericType { get; }
    }
}
