using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BehaviourTree.Demo.GameEngine
{
    public sealed class ComponentMatchingFamily : IFamily
    {
        private readonly Dictionary<int, Node> _entityNodeLookup = new Dictionary<int, Node>(1024);
        private readonly Dictionary<Type, Action<Node, IComponent>> _componentSetters;
        private readonly Type[] _componentTypes;
        private readonly Func<Node> _nodeFactory;
        private readonly List<Node> _nodes = new List<Node>();

        public ComponentMatchingFamily(Type nodeType)
        {
            // Create compiled factory delegate for fast instantiation
            _nodeFactory = CreateNodeFactory(nodeType);

            // Create compiled property setters instead of reflection
            var fieldInfos = nodeType
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => typeof(IComponent).IsAssignableFrom(x.FieldType))
                .ToArray();

            _componentTypes = fieldInfos.Select(f => f.FieldType).ToArray();
            _componentSetters = new Dictionary<Type, Action<Node, IComponent>>(fieldInfos.Length);

            foreach (var fieldInfo in fieldInfos)
            {
                _componentSetters[fieldInfo.FieldType] = CreateFieldSetter(fieldInfo);
            }
        }

        private static Func<Node> CreateNodeFactory(Type nodeType)
        {
            var ctor = nodeType.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                throw new InvalidOperationException($"Node type {nodeType.Name} must have a parameterless constructor");

            var newExpr = Expression.New(ctor);
            var lambda = Expression.Lambda<Func<Node>>(newExpr);
            return lambda.Compile();
        }

        private static Action<Node, IComponent> CreateFieldSetter(FieldInfo fieldInfo)
        {
            var nodeParam = Expression.Parameter(typeof(Node), "node");
            var componentParam = Expression.Parameter(typeof(IComponent), "component");

            var castNode = Expression.Convert(nodeParam, fieldInfo.DeclaringType!);
            var castComponent = Expression.Convert(componentParam, fieldInfo.FieldType);
            var fieldAccess = Expression.Field(castNode, fieldInfo);
            var assign = Expression.Assign(fieldAccess, castComponent);

            var lambda = Expression.Lambda<Action<Node, IComponent>>(assign, nodeParam, componentParam);
            return lambda.Compile();
        }

        public IEnumerable<Node> GetNodes()
        {
            return _nodes;
        }

        public void NewEntity(Entity entity)
        {
            AddIfMatch(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            RemoveIfMatch(entity);
        }

        public void ComponentAddedToEntity(Entity entity, Type componentType)
        {
            AddIfMatch(entity);
        }

        public void ComponentRemovedFromEntity(Entity entity, Type componentType)
        {
            if (_componentSetters.ContainsKey(componentType))
            {
                RemoveIfMatch(entity);
            }
        }

        private void AddIfMatch(Entity entity)
        {
            if (_entityNodeLookup.ContainsKey(entity.Id))
            {
                return;
            }

            // Check if entity has all required components
            foreach (var componentType in _componentTypes)
            {
                if (!entity.HasComponent(componentType))
                {
                    return;
                }
            }

            // Use compiled factory instead of Activator.CreateInstance
            var node = _nodeFactory();
            node.Entity = entity;

            // Use compiled setters instead of reflection
            foreach (var componentType in _componentTypes)
            {
                var component = entity.GetComponent(componentType);
                var setter = _componentSetters[componentType];
                setter(node, component);
            }

            _entityNodeLookup[entity.Id] = node;
            _nodes.Add(node);
        }

        private void RemoveIfMatch(Entity entity)
        {
            if (!_entityNodeLookup.TryGetValue(entity.Id, out var node))
            {
                return;
            }

            _entityNodeLookup.Remove(entity.Id);
            _nodes.Remove(node);
        }
    }
}
