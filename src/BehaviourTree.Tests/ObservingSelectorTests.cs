using BehaviourTree.Blackboard;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    [TestFixture]
    internal sealed class ObservingSelectorTests
    {
        private sealed class BbContext
        {
            public Blackboard.Blackboard Blackboard { get; } = new Blackboard.Blackboard();
        }

        private sealed class SimpleMock<T> : BaseBehaviour<T>
        {
            public BehaviourStatus ReturnStatus { get; set; }
            public int TickCount { get; private set; }

            public SimpleMock(BehaviourStatus status) : base("Mock")
            {
                ReturnStatus = status;
            }

            protected override BehaviourStatus Update(T context)
            {
                TickCount++;
                return ReturnStatus;
            }
        }

        [Test]
        public void ObservingSelector_NormalBehavior_SelectsFirstSuccess()
        {
            var child1 = new MockBehaviour(BehaviourStatus.Failed);
            var child2 = new MockBehaviour(BehaviourStatus.Succeeded);
            var selector = new ObservingSelector<MockContext>(child1, child2);

            var result = selector.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void ObservingSelector_StaysOnRunningChild()
        {
            var child1 = new MockBehaviour(BehaviourStatus.Failed);
            var child2 = new MockBehaviour(BehaviourStatus.Running);
            var selector = new ObservingSelector<MockContext>(child1, child2);

            selector.Tick(new MockContext());
            // Record tick count after first pass
            var ticksBefore = child1.InitializeCallCount;
            selector.Tick(new MockContext());
            // child1 should NOT be ticked again (unlike PrioritySelector)
            Assert.That(child1.InitializeCallCount, Is.EqualTo(ticksBefore));
        }

        [Test]
        public void ObservingSelector_InterruptsOnAbortRequest()
        {
            var ctx = new BbContext();
            ctx.Blackboard.Set("enemyVisible", false);

            var attackAction = new SimpleMock<BbContext>(BehaviourStatus.Succeeded);
            var bbCondition = new BlackboardCondition<BbContext>(
                attackAction,
                c => c.Blackboard,
                "enemyVisible",
                val => val is bool b && b,
                AbortMode.LowerPriority);

            var patrolAction = new SimpleMock<BbContext>(BehaviourStatus.Running);

            var selector = new ObservingSelector<BbContext>(
                new IBehaviour<BbContext>[] { bbCondition, patrolAction });

            // First tick: condition false → patrol runs
            var result1 = selector.Tick(ctx);
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));

            // Blackboard changes
            ctx.Blackboard.Set("enemyVisible", true);

            // Next tick: abort requested → re-evaluates → attack succeeds
            var result2 = selector.Tick(ctx);
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void BlackboardCondition_SelfAbort_AbortsRunningChild()
        {
            var ctx = new BbContext();
            ctx.Blackboard.Set("hasAmmo", true);

            var shootAction = new SimpleMock<BbContext>(BehaviourStatus.Running);
            var bbCondition = new BlackboardCondition<BbContext>(
                shootAction,
                c => c.Blackboard,
                "hasAmmo",
                val => val is bool b && b,
                AbortMode.Self);

            var result1 = bbCondition.Tick(ctx);
            Assert.That(result1, Is.EqualTo(BehaviourStatus.Running));

            ctx.Blackboard.Set("hasAmmo", false);

            var result2 = bbCondition.Tick(ctx);
            Assert.That(result2, Is.EqualTo(BehaviourStatus.Failed));
        }
    }
}
