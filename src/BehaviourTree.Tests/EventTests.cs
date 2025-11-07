using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourTree.Behaviours;
using BehaviourTree.Events;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    /// <summary>
    /// Tests for behavior tree event system using the observer pattern.
    /// </summary>
    [TestFixture]
    internal sealed class EventTests
    {
        /// <summary>
        /// Helper class to capture events from behavior nodes.
        /// </summary>
        private class TestObserver : IBehaviourTreeObserver
        {
            public readonly Queue<BehaviourTreeNodeEvent> Events = new Queue<BehaviourTreeNodeEvent>();

            public void OnNodeInitialize(BehaviourTreeNodeEvent nodeEvent)
            {
                Events.Enqueue(nodeEvent);
            }

            public void OnNodeUpdate(BehaviourTreeNodeEvent nodeEvent)
            {
                Events.Enqueue(nodeEvent);
            }

            public void OnNodeTerminate(BehaviourTreeNodeEvent nodeEvent)
            {
                Events.Enqueue(nodeEvent);
            }

            public void OnNodeReset(BehaviourTreeNodeEvent nodeEvent)
            {
                Events.Enqueue(nodeEvent);
            }
        }

        [Test]
        public void ObserverReceivesEvents_SingleNode()
        {
            // Arrange
            var observer = new TestObserver();
            var node = new Condition<MockContext>("Test", c => true);
            node.AttachObserver(observer);

            // Act - Execute one tick
            node.Tick(new MockContext());

            // Assert - Should receive Initialize, Update, Terminate events
            Assert.That(observer.Events.Count, Is.EqualTo(3), "Should receive 3 events for one tick");

            var initEvent = observer.Events.Dequeue();
            Assert.That(initEvent.NodeId, Is.EqualTo(node.Id));
            Assert.That(initEvent.EventType, Is.EqualTo(BehaviourTreeNodeInfoEventType.Initialize));
            Assert.That(initEvent.NodeName, Is.EqualTo("Test"));

            var updateEvent = observer.Events.Dequeue();
            Assert.That(updateEvent.NodeId, Is.EqualTo(node.Id));
            Assert.That(updateEvent.EventType, Is.EqualTo(BehaviourTreeNodeInfoEventType.Update));
            Assert.That(updateEvent.Status, Is.EqualTo(BehaviourStatus.Succeeded));

            var terminateEvent = observer.Events.Dequeue();
            Assert.That(terminateEvent.NodeId, Is.EqualTo(node.Id));
            Assert.That(terminateEvent.EventType, Is.EqualTo(BehaviourTreeNodeInfoEventType.Terminate));

            // Act - Reset
            node.Reset();

            // Assert - Should receive Reset event
            Assert.That(observer.Events.Count, Is.EqualTo(1), "Should receive 1 reset event");
            var resetEvent = observer.Events.Dequeue();
            Assert.That(resetEvent.NodeId, Is.EqualTo(node.Id));
            Assert.That(resetEvent.EventType, Is.EqualTo(BehaviourTreeNodeInfoEventType.Reset));

            // Act - Detach and tick again
            node.DetachObserver(observer);
            node.Tick(new MockContext());

            // Assert - Should not receive events after detaching
            Assert.That(observer.Events.Count, Is.EqualTo(0), "Should not receive events after detaching");
        }

        [Test]
        public void ObserverReceivesEvents_MultipleNodes()
        {
            // Arrange
            var observer = new TestObserver();
            var node1 = new Condition<MockContext>("Node1", c => true);
            var node2 = new Condition<MockContext>("Node2", c => false);
            node1.AttachObserver(observer);
            node2.AttachObserver(observer);

            // Act
            node1.Tick(new MockContext());
            node2.Tick(new MockContext());

            // Assert - Should receive 6 events total (3 from each node)
            Assert.That(observer.Events.Count, Is.EqualTo(6), "Should receive 3 events from each node");

            // Verify node1 events
            var node1Init = observer.Events.Dequeue();
            Assert.That(node1Init.NodeId, Is.EqualTo(node1.Id));
            Assert.That(node1Init.EventType, Is.EqualTo(BehaviourTreeNodeInfoEventType.Initialize));

            var node1Update = observer.Events.Dequeue();
            Assert.That(node1Update.NodeId, Is.EqualTo(node1.Id));
            Assert.That(node1Update.Status, Is.EqualTo(BehaviourStatus.Succeeded));

            var node1Terminate = observer.Events.Dequeue();
            Assert.That(node1Terminate.NodeId, Is.EqualTo(node1.Id));
            Assert.That(node1Terminate.EventType, Is.EqualTo(BehaviourTreeNodeInfoEventType.Terminate));

            // Verify node2 events
            var node2Init = observer.Events.Dequeue();
            Assert.That(node2Init.NodeId, Is.EqualTo(node2.Id));
            Assert.That(node2Init.NodeName, Is.EqualTo("Node2"));

            var node2Update = observer.Events.Dequeue();
            Assert.That(node2Update.NodeId, Is.EqualTo(node2.Id));
            Assert.That(node2Update.Status, Is.EqualTo(BehaviourStatus.Failed));

            var node2Terminate = observer.Events.Dequeue();
            Assert.That(node2Terminate.NodeId, Is.EqualTo(node2.Id));
        }

        [Test]
        public void ObserverIsolation_SeparateObserversForSeparateTrees()
        {
            // Arrange
            var observer1 = new TestObserver();
            var observer2 = new TestObserver();
            var node1 = new Condition<MockContext>("Node1", c => true);
            var node2 = new Condition<MockContext>("Node2", c => true);
            node1.AttachObserver(observer1);
            node2.AttachObserver(observer2);

            // Act
            node1.Tick(new MockContext());
            node2.Tick(new MockContext());

            // Assert - Each observer should only receive events from their own node
            Assert.That(observer1.Events.Count, Is.EqualTo(3), "Observer1 should only receive events from node1");
            Assert.That(observer2.Events.Count, Is.EqualTo(3), "Observer2 should only receive events from node2");

            Assert.That(observer1.Events.Peek().NodeId, Is.EqualTo(node1.Id));
            Assert.That(observer2.Events.Peek().NodeId, Is.EqualTo(node2.Id));
        }

        [Test]
        public void MultipleObservers_AllReceiveEvents()
        {
            // Arrange
            var observer1 = new TestObserver();
            var observer2 = new TestObserver();
            var node = new Condition<MockContext>("Test", c => true);
            node.AttachObserver(observer1);
            node.AttachObserver(observer2);

            // Act
            node.Tick(new MockContext());

            // Assert - Both observers should receive the same events
            Assert.That(observer1.Events.Count, Is.EqualTo(3));
            Assert.That(observer2.Events.Count, Is.EqualTo(3));
        }

        [Test]
        public void DuplicateObserver_OnlyAttachedOnce()
        {
            // Arrange
            var observer = new TestObserver();
            var node = new Condition<MockContext>("Test", c => true);
            node.AttachObserver(observer);
            node.AttachObserver(observer); // Attach same observer twice

            // Act
            node.Tick(new MockContext());

            // Assert - Should only receive 3 events (not 6), proving duplicate prevention
            Assert.That(observer.Events.Count, Is.EqualTo(3));
        }

        [Test]
        public void EventInfo_ContainsAccurateData()
        {
            // Arrange
            var observer = new TestObserver();
            var node = new ActionBehaviour<MockContext>("TestAction", c => BehaviourStatus.Succeeded);
            node.AttachObserver(observer);

            // Act
            node.Tick(new MockContext());

            // Assert
            var events = observer.Events.ToArray();
            var updateEvent = events[1]; // Update event

            Assert.That(updateEvent.NodeName, Is.EqualTo("TestAction"));
            Assert.That(updateEvent.NodeType, Contains.Substring("Action"));
            Assert.That(updateEvent.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(0));
            Assert.That(updateEvent.Status, Is.EqualTo(BehaviourStatus.Succeeded));
        }
    }
}
