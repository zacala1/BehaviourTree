using BehaviourTree.Demo.Components;
using BehaviourTree.Demo.GameEngine;

namespace BehaviourTree.Demo.Nodes
{
    public sealed class MoveNode : Node
    {
        public PositionComponent PositionComponent = null!;
        public MovementComponent MovementComponent = null!;
    }
}
