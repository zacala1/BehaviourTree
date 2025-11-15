using BehaviourTree.Demo.GameEngine;
using System;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.GameEngine
{
    public class ObjectPoolTests
    {
        [Test]
        public void Constructor_PrePopulatesPool()
        {
            // Arrange & Act
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 5);

            // Assert
            Assert.AreEqual(5, pool.Count);
        }

        [Test]
        public void Constructor_NullFactory_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ObjectPool<TestObject>(null!));
        }

        [Test]
        public void Rent_ReturnsObjectFromPool()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 1);
            var initialCount = pool.Count;

            // Act
            var obj = pool.Rent();

            // Assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(initialCount - 1, pool.Count);
        }

        [Test]
        public void Rent_WhenPoolEmpty_CreatesNewObject()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 0);

            // Act
            var obj = pool.Rent();

            // Assert
            Assert.IsNotNull(obj);
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void Return_AddsObjectBackToPool()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 0);
            var obj = pool.Rent();

            // Act
            pool.Return(obj);

            // Assert
            Assert.AreEqual(1, pool.Count);
        }

        [Test]
        public void Return_Null_DoesNothing()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 0);

            // Act
            pool.Return(null!);

            // Assert
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void Return_WhenMaxSizeReached_DoesNotAddToPool()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 0, maxSize: 2);

            // Act
            pool.Return(new TestObject());
            pool.Return(new TestObject());
            pool.Return(new TestObject()); // This should not be added

            // Assert
            Assert.AreEqual(2, pool.Count);
        }

        [Test]
        public void Return_CallsResetAction()
        {
            // Arrange
            var resetCalled = false;
            var pool = new ObjectPool<TestObject>(
                () => new TestObject(),
                reset: obj => resetCalled = true,
                initialSize: 0);
            var obj = pool.Rent();

            // Act
            pool.Return(obj);

            // Assert
            Assert.IsTrue(resetCalled);
        }

        [Test]
        public void RentReturn_Cycle_ReusesSameObject()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 0);

            // Act
            var obj1 = pool.Rent();
            obj1.Value = 42;
            pool.Return(obj1);
            var obj2 = pool.Rent();

            // Assert
            Assert.AreSame(obj1, obj2);
        }

        [Test]
        public void RentReturn_WithReset_ResetsObjectState()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(
                () => new TestObject(),
                reset: obj => obj.Value = 0,
                initialSize: 0);

            // Act
            var obj1 = pool.Rent();
            obj1.Value = 42;
            pool.Return(obj1);
            var obj2 = pool.Rent();

            // Assert
            Assert.AreEqual(0, obj2.Value);
        }

        [Test]
        public void Clear_RemovesAllObjectsFromPool()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 5);

            // Act
            pool.Clear();

            // Assert
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void MultipleRents_WhenPoolEmpty_CreatesNewObjects()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 2);

            // Act
            var obj1 = pool.Rent();
            var obj2 = pool.Rent();
            var obj3 = pool.Rent(); // Pool is now empty, should create new

            // Assert
            Assert.IsNotNull(obj1);
            Assert.IsNotNull(obj2);
            Assert.IsNotNull(obj3);
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void Count_ReflectsCurrentPoolSize()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 3);

            // Act & Assert
            Assert.AreEqual(3, pool.Count);

            pool.Rent();
            Assert.AreEqual(2, pool.Count);

            pool.Rent();
            Assert.AreEqual(1, pool.Count);

            pool.Return(new TestObject());
            Assert.AreEqual(2, pool.Count);
        }

        // Test helper class
        private class TestObject
        {
            public int Value { get; set; }
        }
    }
}
