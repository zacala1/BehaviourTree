using System;
using System.Collections.Generic;
using BehaviourTree.Behaviours;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    [TestFixture]
    internal sealed class EventTests
    {
        [Test]
        public void EventOccurred_SingleNode()
        {
            var queue = new Queue<BehaviourTreeEventArgs>();
            EventHandler<BehaviourTreeEventArgs> handler = (s, arg) =>
            {
                var sender = (BaseBehaviour)s;
                if (sender.Id != arg.Id) throw new ArgumentException();
                queue.Enqueue(arg);
            };
            var mock1 = new Condition<MockContext>(c => true);
            BaseBehaviour.StatusEvent += handler;
            
            mock1.Tick(new MockContext());
            Assert.That(mock1.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Initialize, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            Assert.That(mock1.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Update, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            Assert.That(mock1.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Terminate, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            Assert.That(0, Is.EqualTo(queue.Count));

            mock1.Reset();
            Assert.That(mock1.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Reset, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            
            BaseBehaviour.StatusEvent -= handler;
            mock1.Tick(new MockContext());
            Assert.That(0, Is.EqualTo(queue.Count));
        }

        [Test]
        public void EventOccurred_MultipleNode()
        {
            var queue = new Queue<BehaviourTreeEventArgs>();
            EventHandler<BehaviourTreeEventArgs> handler = (s, arg) =>
            {
                var sender = (BaseBehaviour)s;
                if (sender.Id != arg.Id) throw new ArgumentException();
                queue.Enqueue(arg);
            };
            var mock1 = new Condition<MockContext>(c => true);
            var mock2 = new Condition<MockContext>(c => true);
            BaseBehaviour.StatusEvent += handler;
            
            mock1.Tick(new MockContext());
            Assert.That(mock1.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Initialize, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            Assert.That(mock1.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Update, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            Assert.That(mock1.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Terminate, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            Assert.That(0, Is.EqualTo(queue.Count));
            
            mock2.Tick(new MockContext());
            Assert.That(mock2.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Initialize, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            Assert.That(mock2.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Update, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            Assert.That(mock2.Id, Is.EqualTo(queue.Peek().Id));
            Assert.That(BehaviourTreeEventType.Terminate, Is.EqualTo(queue.Peek().Type));
            queue.Dequeue();
            Assert.That(0, Is.EqualTo(queue.Count));
            
            BaseBehaviour.StatusEvent -= handler;
            mock1.Tick(new MockContext());
            Assert.That(0, Is.EqualTo(queue.Count));
            mock2.Tick(new MockContext());
            Assert.That(0, Is.EqualTo(queue.Count));
        }
    }
}
