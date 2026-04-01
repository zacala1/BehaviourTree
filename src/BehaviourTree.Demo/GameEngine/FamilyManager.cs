using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourTree.Demo.GameEngine
{
    public sealed class FamilyManager
    {
        private readonly EntityManager _entityManager;
        private readonly Dictionary<Type, IFamily> _families = new Dictionary<Type, IFamily>(16);

        // Index: component type -> families that care about it
        private readonly Dictionary<Type, List<IFamily>> _componentToFamilies = new Dictionary<Type, List<IFamily>>(32);

        // Cache: node type -> typed enumerable (avoids Cast + ImmutableCollection allocation per frame)
        private readonly Dictionary<Type, object> _typedNodeCache = new Dictionary<Type, object>(16);

        public FamilyManager(EntityManager entityManager)
        {
            _entityManager = entityManager;
            _entityManager.EntityAdded += EntityManager_EntityAdded;
            _entityManager.EntityRemoved += EntityManager_EntityRemoved;
        }

        public IEnumerable<T> GetNodes<T>() where T : Node
        {
            var nodeType = typeof(T);

            // Return cached typed view if available
            if (_typedNodeCache.TryGetValue(nodeType, out var cached))
            {
                return (TypedNodeView<T>)cached;
            }

            if (!_families.TryGetValue(nodeType, out var family))
            {
                family = new ComponentMatchingFamily(nodeType);
                _families[nodeType] = family;
                RegisterFamilyComponentIndex(family);

                foreach (var entity in _entityManager.GetAllEntities())
                {
                    family.NewEntity(entity);
                }
            }

            var view = new TypedNodeView<T>(family);
            _typedNodeCache[nodeType] = view;
            return view;
        }

        private void RegisterFamilyComponentIndex(IFamily family)
        {
            foreach (var componentType in family.ComponentTypes)
            {
                if (!_componentToFamilies.TryGetValue(componentType, out var families))
                {
                    families = new List<IFamily>(4);
                    _componentToFamilies[componentType] = families;
                }
                families.Add(family);
            }
        }

        private void EntityManager_EntityAdded(object? sender, Entity e)
        {
            e.ComponentAdded += Entity_ComponentAdded;
            e.ComponentRemoved += Entity_ComponentRemoved;

            foreach (var family in _families.Values)
            {
                family.NewEntity(e);
            }
        }

        private void EntityManager_EntityRemoved(object? sender, Entity e)
        {
            foreach (var family in _families.Values)
            {
                family.RemoveEntity(e);
            }

            e.ComponentAdded -= Entity_ComponentAdded;
            e.ComponentRemoved -= Entity_ComponentRemoved;
        }

        private void Entity_ComponentRemoved(object? sender, IComponent e)
        {
            var componentType = e.GetType();
            if (_componentToFamilies.TryGetValue(componentType, out var families))
            {
                foreach (var family in families)
                {
                    family.ComponentRemovedFromEntity((Entity)sender!, componentType);
                }
            }
        }

        private void Entity_ComponentAdded(object? sender, IComponent e)
        {
            var componentType = e.GetType();
            if (_componentToFamilies.TryGetValue(componentType, out var families))
            {
                foreach (var family in families)
                {
                    family.ComponentAddedToEntity((Entity)sender!, componentType);
                }
            }
        }

        /// <summary>
        /// Lightweight typed wrapper over IFamily.GetNodes() that avoids
        /// LINQ Cast and allocation per enumeration.
        /// </summary>
        private sealed class TypedNodeView<T> : IEnumerable<T> where T : Node
        {
            private readonly IFamily _family;

            public TypedNodeView(IFamily family)
            {
                _family = family;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new CastEnumerator(_family.GetNodes().GetEnumerator());
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private struct CastEnumerator : IEnumerator<T>
            {
                private readonly IEnumerator<Node> _inner;

                public CastEnumerator(IEnumerator<Node> inner)
                {
                    _inner = inner;
                }

                public T Current => (T)_inner.Current;
                object System.Collections.IEnumerator.Current => Current!;
                public bool MoveNext() => _inner.MoveNext();
                public void Reset() => _inner.Reset();
                public void Dispose() => _inner.Dispose();
            }
        }
    }
}
