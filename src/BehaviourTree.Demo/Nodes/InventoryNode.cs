using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;

namespace BehaviourTree.Demo.Nodes
{
    public sealed class InventoryNode : Node
    {
        public PositionComponent PositionComponent = null!;
        public RenderComponent RenderComponent = null!;
        public InventoryComponent InventoryComponent = null!;
    }
}