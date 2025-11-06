using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourTree.Demo.GameEngine
{
    /// <summary>
    /// Represents an entity that can hold components.
    /// </summary>
    public class Entity
    {
        private readonly Dictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public Entity(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Occurs when a component is added.
        /// </summary>
        public event EventHandler<IComponent> ComponentAdded = delegate { };

        /// <summary>
        /// Occurs when a component is removed.
        /// </summary>
        public event EventHandler<IComponent> ComponentRemoved = delegate { };

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Adds a component to the entity.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>The entity for method chaining.</returns>
        public Entity AddComponent(IComponent component)
        {
            _components[component.GetType()] = component;
            OnComponentAdded(component);
            return this;
        }

        /// <summary>
        /// Removes a component by type.
        /// </summary>
        /// <param name="componentType">The type of component to remove.</param>
        /// <returns>The entity for method chaining.</returns>
        public Entity RemoveComponent(Type componentType)
        {
            if (_components.TryGetValue(componentType, out var component))
            {
                OnComponentRemoved(component);
            }

            return this;
        }

        /// <summary>
        /// Removes a component by type.
        /// </summary>
        /// <typeparam name="T">The type of component to remove.</typeparam>
        /// <returns>The entity for method chaining.</returns>
        public Entity RemoveComponent<T>()
        {
            return RemoveComponent(typeof(T));
        }

        /// <summary>
        /// Checks if the entity has a component of the specified type.
        /// </summary>
        /// <param name="componentType">The type of component to check.</param>
        /// <returns>True if the component exists; otherwise, false.</returns>
        public bool HasComponent(Type componentType)
        {
            return _components.ContainsKey(componentType);
        }

        /// <summary>
        /// Checks if the entity has a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component to check.</typeparam>
        /// <returns>True if the component exists; otherwise, false.</returns>
        public bool HasComponent<T>()
        {
            return HasComponent(typeof(T));
        }

        /// <summary>
        /// Gets a component by type.
        /// </summary>
        /// <param name="componentType">The type of component to get.</param>
        /// <returns>The component.</returns>
        public IComponent GetComponent(Type componentType)
        {
            return _components[componentType];
        }

        /// <summary>
        /// Gets a component by type.
        /// </summary>
        /// <typeparam name="T">The type of component to get.</typeparam>
        /// <returns>The component.</returns>
        public T GetComponent<T>()
        {
            _components.TryGetValue(typeof(T), out var component);
            return (T) component;
        }

        /// <summary>
        /// Gets all components.
        /// </summary>
        /// <returns>All components in the entity.</returns>
        public IComponent[] GetComponents()
        {
            return _components.Values.ToArray();
        }

        protected virtual void OnComponentAdded(IComponent component)
        {
            ComponentAdded?.Invoke(this, component);
        }

        protected virtual void OnComponentRemoved(IComponent component)
        {
            ComponentRemoved?.Invoke(this, component);
        }
    }
}