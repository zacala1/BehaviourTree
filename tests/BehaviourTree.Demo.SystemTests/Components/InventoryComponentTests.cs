using BehaviourTree.Demo.Components;
using NUnit.Framework;

namespace BehaviourTree.Demo.SystemTests.Components
{
    public class InventoryComponentTests
    {
        [Test]
        public void Add_AddsItemToInventory()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act
            component.Add(ItemTypes.Food, 5);

            // Assert
            Assert.IsTrue(component.Has(ItemTypes.Food));
            Assert.AreEqual(5, component.Count(ItemTypes.Food));
        }

        [Test]
        public void Add_MultipleCalls_AccumulatesQuantity()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act
            component.Add(ItemTypes.Food, 3);
            component.Add(ItemTypes.Food, 2);

            // Assert
            Assert.AreEqual(5, component.Count(ItemTypes.Food));
        }

        [Test]
        public void Remove_DecreasesQuantity()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Food, 10);

            // Act
            component.Remove(ItemTypes.Food, 4);

            // Assert
            Assert.AreEqual(6, component.Count(ItemTypes.Food));
        }

        [Test]
        public void Remove_DoesNotGoBelowZero()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Food, 5);

            // Act
            component.Remove(ItemTypes.Food, 10);

            // Assert
            Assert.AreEqual(0, component.Count(ItemTypes.Food));
        }

        [Test]
        public void Remove_NonExistentItem_DoesNothing()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act
            component.Remove(ItemTypes.Food, 5);

            // Assert
            Assert.AreEqual(0, component.Count(ItemTypes.Food));
        }

        [Test]
        public void Has_ReturnsTrueWhenItemExists()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Wood, 5);

            // Act & Assert
            Assert.IsTrue(component.Has(ItemTypes.Wood));
        }

        [Test]
        public void Has_ReturnsFalseWhenItemDoesNotExist()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act & Assert
            Assert.IsFalse(component.Has(ItemTypes.Wood));
        }

        [Test]
        public void Has_WithQuantity_ReturnsTrueWhenSufficientQuantity()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Wood, 10);

            // Act & Assert
            Assert.IsTrue(component.Has(ItemTypes.Wood, 5));
            Assert.IsTrue(component.Has(ItemTypes.Wood, 10));
        }

        [Test]
        public void Has_WithQuantity_ReturnsFalseWhenInsufficientQuantity()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Wood, 5);

            // Act & Assert
            Assert.IsFalse(component.Has(ItemTypes.Wood, 10));
        }

        [Test]
        public void Count_ReturnsZeroForNonExistentItem()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act & Assert
            Assert.AreEqual(0, component.Count(ItemTypes.Stone));
        }

        [Test]
        public void MultipleItemTypes_ManagedIndependently()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act
            component.Add(ItemTypes.Food, 5);
            component.Add(ItemTypes.Wood, 10);
            component.Add(ItemTypes.Stone, 3);

            // Assert
            Assert.AreEqual(5, component.Count(ItemTypes.Food));
            Assert.AreEqual(10, component.Count(ItemTypes.Wood));
            Assert.AreEqual(3, component.Count(ItemTypes.Stone));
        }
    }
}
