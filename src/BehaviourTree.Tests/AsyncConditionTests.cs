using System;
using System.Threading;
using System.Threading.Tasks;
using BehaviourTree.Behaviours;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    /// <summary>
    /// Tests for AsyncCondition behavior node.
    /// </summary>
    [TestFixture]
    internal sealed class AsyncConditionTests
    {
        [Test]
        public void AsyncCondition_ReturnsRunning_WhileAwaiting()
        {
            var asyncCondition = new AsyncCondition<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(100, token);
                return true;
            });

            var result = asyncCondition.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Running));
        }

        [Test]
        public void AsyncCondition_ReturnsSucceeded_WhenTrue()
        {
            var asyncCondition = new AsyncCondition<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                return true;
            });

            asyncCondition.Tick(new MockContext());
            Thread.Sleep(50);
            var result = asyncCondition.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void AsyncCondition_ReturnsFailed_WhenFalse()
        {
            var asyncCondition = new AsyncCondition<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                return false;
            });

            asyncCondition.Tick(new MockContext());
            Thread.Sleep(50);
            var result = asyncCondition.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
        }

        [Test]
        public void AsyncCondition_SyncTrue_SucceedsOnFirstTick()
        {
            var asyncCondition = new AsyncCondition<MockContext>("Test", (ctx, token) =>
            {
                return Task.FromResult(true);
            });

            var result = asyncCondition.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void AsyncCondition_SyncFalse_FailsOnFirstTick()
        {
            var asyncCondition = new AsyncCondition<MockContext>("Test", (ctx, token) =>
            {
                return Task.FromResult(false);
            });

            var result = asyncCondition.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
        }

        [Test]
        public void AsyncCondition_Exception_ReturnsFailed()
        {
            var asyncCondition = new AsyncCondition<MockContext>("Test", async (ctx, token) =>
            {
                await Task.Delay(10, token);
                throw new InvalidOperationException("Test error");
            });

            asyncCondition.Tick(new MockContext());
            Thread.Sleep(50);
            var result = asyncCondition.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(asyncCondition.LastException, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void AsyncCondition_CancelCondition_SetsCancelled()
        {
            var shouldCancel = false;
            var asyncCondition = new AsyncCondition<MockContext>("Test",
                async (ctx, token) =>
                {
                    await Task.Delay(1000, token);
                    return true;
                },
                ctx => shouldCancel);

            asyncCondition.Tick(new MockContext());
            shouldCancel = true;
            var result = asyncCondition.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(asyncCondition.WasCancelled, Is.True);
        }

        [Test]
        public void AsyncCondition_Timeout_SetsCancelled()
        {
            var asyncCondition = new AsyncCondition<MockContext>("Test",
                async (ctx, token) =>
                {
                    await Task.Delay(5000, token);
                    return true;
                },
                TimeSpan.FromMilliseconds(20));

            asyncCondition.Tick(new MockContext());
            Thread.Sleep(50);
            var result = asyncCondition.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(asyncCondition.WasCancelled, Is.True);
        }

        [Test]
        public void AsyncCondition_ExternalToken_Cancels()
        {
            using var externalCts = new CancellationTokenSource();
            var asyncCondition = new AsyncCondition<MockContext>("Test",
                async (ctx, token) =>
                {
                    await Task.Delay(5000, token);
                    return true;
                },
                null,
                default,
                externalCts.Token);

            asyncCondition.Tick(new MockContext());
            externalCts.Cancel();
            Thread.Sleep(50);
            var result = asyncCondition.Tick(new MockContext());

            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
            Assert.That(asyncCondition.WasCancelled, Is.True);
        }

        [Test]
        public void AsyncCondition_PassesContext()
        {
            MockContext? captured = null;
            var expected = new MockContext();
            var asyncCondition = new AsyncCondition<MockContext>("Test", async (ctx, token) =>
            {
                captured = ctx;
                await Task.CompletedTask;
                return true;
            });

            asyncCondition.Tick(expected);

            Assert.That(captured, Is.SameAs(expected));
        }

        [Test]
        public void AsyncCondition_CanBeReused()
        {
            var count = 0;
            var asyncCondition = new AsyncCondition<MockContext>("Test", (ctx, token) =>
            {
                count++;
                return Task.FromResult(true);
            });

            asyncCondition.Tick(new MockContext());
            asyncCondition.Reset();
            asyncCondition.Tick(new MockContext());

            Assert.That(count, Is.EqualTo(2));
        }
    }
}
