using BehaviourTree.Decorators;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    [TestFixture]
    internal sealed class TimeLimiterTests
    {
        [TestCase(BehaviourStatus.Succeeded)]
        [TestCase(BehaviourStatus.Failed)]
        [TestCase(BehaviourStatus.Running)]
        public void WhileTimeLimitHasNotExpired_ReturnChildStatus(BehaviourStatus status)
        {
            var child = new MockBehaviour { ReturnStatus = status };
            var sut = new TimeLimiter<MockContext>(child, 1000);

            var behaviourStatus = sut.Tick(new MockContext());

            Assert.That(behaviourStatus, Is.EqualTo(status));
            Assert.That(child.UpdateCallCount, Is.EqualTo(1));
        }

        [TestCase(BehaviourStatus.Succeeded)]
        [TestCase(BehaviourStatus.Failed)]
        public void WhenTimeLimitHasExpired_ReturnFailureAndResetChild(BehaviourStatus status)
        {
            var child = new MockBehaviour { ReturnStatus = BehaviourStatus.Running };
            var sut = new TimeLimiter<MockContext>(child, 1000);
            var context = new MockContext();

            sut.Tick(context);

            context.AddMilliseconds(2000);

            var behaviourStatus = sut.Tick(context);

            Assert.That(behaviourStatus, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(child.UpdateCallCount, Is.EqualTo(1));

            child.ReturnStatus = status;

            behaviourStatus = sut.Tick(context);

            Assert.That(behaviourStatus, Is.EqualTo(status));
            Assert.That(child.UpdateCallCount, Is.EqualTo(2));
        }

        [Test]
        public void WhenResettingWhileRunning_ReInitializeTimer()
        {
            var child = new MockBehaviour
            {
                ReturnStatus = BehaviourStatus.Running
            };

            var context = new MockContext();

            var sut = new TimeLimiter<MockContext>(child, 1000);

            sut.Tick(context);

            context.AddMilliseconds(2000);

            sut.Reset();

            var behaviourStatus = sut.Tick(context);

            Assert.That(behaviourStatus, Is.EqualTo(BehaviourStatus.Running));
        }
    }
}
