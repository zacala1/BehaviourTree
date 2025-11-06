namespace BehaviourTree.Reflection
{
    /// <summary>
    /// Defines the types of nodes in a behavior tree for reflection and visualization.
    /// </summary>
    public enum TreeNodeType
    {
        /// <summary>
        /// Generic composite node with multiple children.
        /// </summary>
        Composite,

        /// <summary>
        /// Parallel composite that executes children concurrently.
        /// </summary>
        Composite_Parallel,

        /// <summary>
        /// Sequence composite that executes children in order until one fails.
        /// </summary>
        Composite_Sequence,

        /// <summary>
        /// Selector composite that executes children until one succeeds.
        /// </summary>
        Composite_Selector,

        /// <summary>
        /// Decorator node that modifies behavior of a single child.
        /// </summary>
        Decorate,

        /// <summary>
        /// Generic leaf node without children.
        /// </summary>
        Leaf,

        /// <summary>
        /// Action leaf that executes custom logic.
        /// </summary>
        Leaf_Action,

        /// <summary>
        /// Condition leaf that evaluates a boolean expression.
        /// </summary>
        Leaf_Condition,

        /// <summary>
        /// Wait leaf that delays execution.
        /// </summary>
        Leaf_Wait
    }
}