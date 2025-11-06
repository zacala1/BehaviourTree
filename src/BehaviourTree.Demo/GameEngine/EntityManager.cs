using System;
using System.Collections.Generic;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// Manages entity creation, removal, and retrieval.
    /// </summary>
    public class EntityManager : IEntityManager
    {
        private int _maxId;
        private readonly Dictionary<int, Entity> _entities = new Dictionary<int, Entity>();

        /// <summary>
        /// Occurs when an entity is added.
        /// </summary>
        public event EventHandler<Entity> EntityAdded = delegate { };

        /// <summary>
        /// Occurs when an entity is removed.
        /// </summary>
        public event EventHandler<Entity> EntityRemoved = delegate { };

        /// <summary>
        /// Gets an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The entity, or null if not found.</returns>
        public Entity GetEntityById(int id)
        {
            _entities.TryGetValue(id, out var entity);

            return entity;
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <returns>The newly created entity.</returns>
        public Entity NewEntity()
        {
            var entity = new Entity(_maxId++);

            _entities.Add(entity.Id, entity);
            OnEntityAdded(entity);

            return entity;
        }

        /// <summary>
        /// Removes an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public void RemoveEntity(int id)
        {
            if (!_entities.TryGetValue(id, out var entity))
            {
                return;
            }

            _entities.Remove(id);
            OnEntityRemoved(entity);
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>All entities in the manager.</returns>
        public IEnumerable<Entity> GetAllEntities()
        {
            return _entities.Values;
        }

        protected virtual void OnEntityAdded(Entity component)
        {
            EntityAdded?.Invoke(this, component);
        }

        protected virtual void OnEntityRemoved(Entity component)
        {
            EntityRemoved?.Invoke(this, component);
        }
    }
}
