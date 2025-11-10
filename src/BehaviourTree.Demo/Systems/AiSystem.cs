using BehaviourTree.Demo.Ai.BT;
using BehaviourTree.Demo.GameEngine;
using BehaviourTree.Demo.Nodes;

namespace BehaviourTree.Demo.Systems
{
    public sealed class AiSystem : IterativeSystem<BtNode>
    {
        private readonly ObjectPool<BtContext> _contextPool;

        public AiSystem(Engine engine) : base(engine)
        {
            // Initialize pool with 32 contexts, max 128
            _contextPool = new ObjectPool<BtContext>(
                factory: () => new BtContext(),
                reset: context => context.Reset(),
                initialSize: 32,
                maxSize: 128
            );
        }

        protected override void UpdateNode(BtNode node, long ellapsedMilliseconds)
        {
            // Rent from pool instead of allocating
            var context = _contextPool.Rent();
            context.Initialize(node.Entity, Engine, ellapsedMilliseconds);

            node.BehaviourComponent.BehaviourTree.Tick(context);

            // Return to pool for reuse
            _contextPool.Return(context);
        }
    }
}
