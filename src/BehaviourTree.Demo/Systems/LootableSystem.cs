using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;
using BehaviourTree.Demo.Nodes;
using System.Collections.Generic;

namespace BehaviourTree.Demo.Systems
{
    public sealed class LootableSystem : IterativeSystem<LootableNode>
    {
        private readonly List<int> _entitiesToRemove = new List<int>();

        public LootableSystem(Engine engine) : base(engine)
        {
        }

        public override void Update(long ellapsedMilliseconds)
        {
            // First pass: collect entities to remove
            base.Update(ellapsedMilliseconds);

            // Second pass: remove entities (after iteration completes)
            foreach (var entityId in _entitiesToRemove)
            {
                Engine.RemoveEntity(entityId);
            }
            _entitiesToRemove.Clear();
        }

        protected override void UpdateNode(LootableNode node, long ellapsedMilliseconds)
        {
            if (node.LootableComponent.Quantity == 0)
            {
                _entitiesToRemove.Add(node.Entity.Id);
            }
        }
    }
}
