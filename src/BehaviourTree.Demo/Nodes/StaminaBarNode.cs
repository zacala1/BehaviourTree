using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;

namespace BehaviourTree.Demo.Nodes
{
    public sealed class StaminaBarNode : Node
    {
        public PositionComponent PositionComponent = null!;
        public RenderComponent RenderComponent = null!;
        public StaminaBarComponent StaminaBarComponent = null!;
        public StaminaComponent StaminaComponent = null!;
    }
}