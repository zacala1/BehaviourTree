namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// Base class for entity component nodes.
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// Gets or sets the entity associated with this node.
        /// </summary>
        public Entity Entity = null!;
    }
}
