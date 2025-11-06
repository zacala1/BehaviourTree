using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;

namespace BehaviourTree.Demo.Nodes
{
    public sealed class RenderableNode : Node
    {
        public RenderComponent RenderComponent = null!;
        public PositionComponent PositionComponent = null!;
    }
}
