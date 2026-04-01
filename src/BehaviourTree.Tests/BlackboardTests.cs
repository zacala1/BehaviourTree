using System;
using System.Collections.Generic;
using BehaviourTree.Blackboard;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    [TestFixture]
    internal sealed class BlackboardTests
    {
        [Test]
        public void Set_And_Get_ReturnsValue()
        {
            var bb = new Blackboard.Blackboard();
            bb.Set("health", 100);
            Assert.That(bb.Get<int>("health"), Is.EqualTo(100));
        }

        [Test]
        public void TryGet_ExistingKey_ReturnsTrue()
        {
            var bb = new Blackboard.Blackboard();
            bb.Set("name", "bot");
            Assert.That(bb.TryGet<string>("name", out var val), Is.True);
            Assert.That(val, Is.EqualTo("bot"));
        }

        [Test]
        public void TryGet_MissingKey_ReturnsFalse()
        {
            var bb = new Blackboard.Blackboard();
            Assert.That(bb.TryGet<int>("missing", out _), Is.False);
        }

        [Test]
        public void Get_MissingKey_Throws()
        {
            var bb = new Blackboard.Blackboard();
            Assert.Throws<KeyNotFoundException>(() => bb.Get<int>("missing"));
        }

        [Test]
        public void HasKey_Exists_ReturnsTrue()
        {
            var bb = new Blackboard.Blackboard();
            bb.Set("key", 1);
            Assert.That(bb.HasKey("key"), Is.True);
        }

        [Test]
        public void HasKey_NotExists_ReturnsFalse()
        {
            var bb = new Blackboard.Blackboard();
            Assert.That(bb.HasKey("key"), Is.False);
        }

        [Test]
        public void Remove_ExistingKey_ReturnsTrue()
        {
            var bb = new Blackboard.Blackboard();
            bb.Set("key", 1);
            Assert.That(bb.Remove("key"), Is.True);
            Assert.That(bb.HasKey("key"), Is.False);
        }

        [Test]
        public void Set_OverwriteExisting_UpdatesValue()
        {
            var bb = new Blackboard.Blackboard();
            bb.Set("hp", 100);
            bb.Set("hp", 50);
            Assert.That(bb.Get<int>("hp"), Is.EqualTo(50));
        }

        [Test]
        public void Observe_NotifiesOnChange()
        {
            var bb = new Blackboard.Blackboard();
            string? changedKey = null;
            object? oldVal = null;
            object? newVal = null;

            bb.Observe("target", (k, o, n) => { changedKey = k; oldVal = o; newVal = n; });
            bb.Set("target", 42);

            Assert.That(changedKey, Is.EqualTo("target"));
            Assert.That(oldVal, Is.Null);
            Assert.That(newVal, Is.EqualTo(42));
        }

        [Test]
        public void Observe_NotifiesOnOverwrite()
        {
            var bb = new Blackboard.Blackboard();
            bb.Set("target", 10);

            object? capturedOld = null;
            bb.Observe("target", (k, o, n) => capturedOld = o);
            bb.Set("target", 20);

            Assert.That(capturedOld, Is.EqualTo(10));
        }

        [Test]
        public void Observe_Dispose_StopsNotification()
        {
            var bb = new Blackboard.Blackboard();
            var count = 0;
            var handle = bb.Observe("key", (k, o, n) => count++);

            bb.Set("key", 1);
            Assert.That(count, Is.EqualTo(1));

            handle.Dispose();
            bb.Set("key", 2);
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void ChildBlackboard_FallsThrough_ToParent()
        {
            var parent = new Blackboard.Blackboard();
            parent.Set("global", "shared");

            var child = new Blackboard.Blackboard(parent);
            Assert.That(child.Get<string>("global"), Is.EqualTo("shared"));
        }

        [Test]
        public void ChildBlackboard_LocalWrite_DoesNotAffectParent()
        {
            var parent = new Blackboard.Blackboard();
            parent.Set("val", 1);

            var child = new Blackboard.Blackboard(parent);
            child.Set("val", 99);

            Assert.That(child.Get<int>("val"), Is.EqualTo(99));
            Assert.That(parent.Get<int>("val"), Is.EqualTo(1));
        }

        [Test]
        public void ChildBlackboard_HasKey_ChecksParent()
        {
            var parent = new Blackboard.Blackboard();
            parent.Set("parentOnly", true);

            var child = new Blackboard.Blackboard(parent);
            Assert.That(child.HasKey("parentOnly"), Is.True);
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            var bb = new Blackboard.Blackboard();
            bb.Set("a", 1);
            bb.Set("b", 2);
            bb.Clear();
            Assert.That(bb.HasKey("a"), Is.False);
            Assert.That(bb.HasKey("b"), Is.False);
        }
    }
}
