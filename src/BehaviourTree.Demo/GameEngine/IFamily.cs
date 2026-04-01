using System;
using System.Collections.Generic;

namespace BehaviourTree.Demo.GameEngine
{
    public interface IFamily
    {
        IEnumerable<Node> GetNodes();

        /// <summary>
        /// Gets the component types required by this family.
        /// </summary>
        IReadOnlyList<Type> ComponentTypes { get; }

        void NewEntity(Entity entity);
        void RemoveEntity(Entity entity);

        void ComponentAddedToEntity(Entity entity, Type componentType);
        void ComponentRemovedFromEntity(Entity entity, Type componentType);
    }
}
