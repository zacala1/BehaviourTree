namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// Defines entity management operations.
    /// </summary>
    public interface IEntityManager
    {
        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <returns>The newly created entity.</returns>
        Entity NewEntity();

        /// <summary>
        /// Gets an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The entity, or null if not found.</returns>
        Entity GetEntityById(int id);

        /// <summary>
        /// Removes an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        void RemoveEntity(int id);
    }
}