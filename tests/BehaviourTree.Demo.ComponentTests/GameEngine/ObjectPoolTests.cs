using BehaviourTree.Demo.GameEngine;
using System;
using Xunit;

namespace BehaviourTree.Demo.ComponentTests.GameEngine
{
    public class ObjectPoolTests
    {
        [Fact]
        public void Constructor_PrePopulatesPool()
        {
            // Arrange & Act
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 5);

            // Assert
            Assert.Equal(5, pool.Count);
        }

        [Fact]
        public void Constructor_NullFactory_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ObjectPool<TestObject>(null!));
        }

        [Fact]
        public void Rent_ReturnsObjectFromPool()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 1);
            var initialCount = pool.Count;

            // Act
            var obj = pool.Rent();

            // Assert
            Assert.NotNull(obj);
            Assert.Equal(initialCount - 1, pool.Count);
        }

        [Fact]
        public void Rent_WhenPoolEmpty_CreatesNewObject()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 0);

            // Act
            var obj = pool.Rent();

            // Assert
            Assert.NotNull(obj);
            Assert.Equal(0, pool.Count);
        }

        [Fact]
        public void Return_AddsObjectBackToPool()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 0);
            var obj = pool.Rent();

            // Act
            pool.Return(obj);

            // Assert
            Assert.Equal(1, pool.Count);
        }

        [Fact]
        public void Return_Null_DoesNothing()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 0);

            // Act
            pool.Return(null!);

            // Assert
            Assert.Equal(0, pool.Count);
        }

        [Fact]
        public void Return_WhenMaxSizeReached_DoesNotAddToPool()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 0, maxSize: 2);

            // Act
            pool.Return(new TestObject());
            pool.Return(new TestObject());
            pool.Return(new TestObject()); // This should not be added

            // Assert
            Assert.Equal(2, pool.Count);
        }

        [Fact]
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
            Assert.True(resetCalled);
        }

        [Fact]
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
            Assert.Same(obj1, obj2);
        }

        [Fact]
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
            Assert.Equal(0, obj2.Value);
        }

        [Fact]
        public void Clear_RemovesAllObjectsFromPool()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 5);

            // Act
            pool.Clear();

            // Assert
            Assert.Equal(0, pool.Count);
        }

        [Fact]
        public void MultipleRents_WhenPoolEmpty_CreatesNewObjects()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 2);

            // Act
            var obj1 = pool.Rent();
            var obj2 = pool.Rent();
            var obj3 = pool.Rent(); // Pool is now empty, should create new

            // Assert
            Assert.NotNull(obj1);
            Assert.NotNull(obj2);
            Assert.NotNull(obj3);
            Assert.Equal(0, pool.Count);
        }

        [Fact]
        public void Count_ReflectsCurrentPoolSize()
        {
            // Arrange
            var pool = new ObjectPool<TestObject>(() => new TestObject(), initialSize: 3);

            // Act & Assert
            Assert.Equal(3, pool.Count);

            pool.Rent();
            Assert.Equal(2, pool.Count);

            pool.Rent();
            Assert.Equal(1, pool.Count);

            pool.Return(new TestObject());
            Assert.Equal(2, pool.Count);
        }

        // Test helper class
        private class TestObject
        {
            public int Value { get; set; }
        }
    }
}
