using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;

namespace BehaviourTree.Demo.Nodes
{
    public sealed class HealthBarNode : Node
    {
        public PositionComponent PositionComponent = null!;
        public RenderComponent RenderComponent = null!;
        public HealthBarComponent HealthBarComponent = null!;
        public HealthComponent HealthComponent = null!;
    }
}