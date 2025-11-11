using BehaviourTree.Demo.Components;
using Xunit;

namespace BehaviourTree.Demo.ComponentTests.Components
{
    public class InventoryComponentTests
    {
        [Fact]
        public void Add_AddsItemToInventory()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act
            component.Add(ItemTypes.Food, 5);

            // Assert
            Assert.True(component.Has(ItemTypes.Food));
            Assert.Equal(5, component.Count(ItemTypes.Food));
        }

        [Fact]
        public void Add_MultipleCalls_AccumulatesQuantity()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act
            component.Add(ItemTypes.Food, 3);
            component.Add(ItemTypes.Food, 2);

            // Assert
            Assert.Equal(5, component.Count(ItemTypes.Food));
        }

        [Fact]
        public void Remove_DecreasesQuantity()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Food, 10);

            // Act
            component.Remove(ItemTypes.Food, 4);

            // Assert
            Assert.Equal(6, component.Count(ItemTypes.Food));
        }

        [Fact]
        public void Remove_DoesNotGoBelowZero()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Food, 5);

            // Act
            component.Remove(ItemTypes.Food, 10);

            // Assert
            Assert.Equal(0, component.Count(ItemTypes.Food));
        }

        [Fact]
        public void Remove_NonExistentItem_DoesNothing()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act
            component.Remove(ItemTypes.Food, 5);

            // Assert
            Assert.Equal(0, component.Count(ItemTypes.Food));
        }

        [Fact]
        public void Has_ReturnsTrueWhenItemExists()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Wood, 5);

            // Act & Assert
            Assert.True(component.Has(ItemTypes.Wood));
        }

        [Fact]
        public void Has_ReturnsFalseWhenItemDoesNotExist()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act & Assert
            Assert.False(component.Has(ItemTypes.Wood));
        }

        [Fact]
        public void Has_WithQuantity_ReturnsTrueWhenSufficientQuantity()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Wood, 10);

            // Act & Assert
            Assert.True(component.Has(ItemTypes.Wood, 5));
            Assert.True(component.Has(ItemTypes.Wood, 10));
        }

        [Fact]
        public void Has_WithQuantity_ReturnsFalseWhenInsufficientQuantity()
        {
            // Arrange
            var component = new InventoryComponent();
            component.Add(ItemTypes.Wood, 5);

            // Act & Assert
            Assert.False(component.Has(ItemTypes.Wood, 10));
        }

        [Fact]
        public void Count_ReturnsZeroForNonExistentItem()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act & Assert
            Assert.Equal(0, component.Count(ItemTypes.Stone));
        }

        [Fact]
        public void MultipleItemTypes_ManagedIndependently()
        {
            // Arrange
            var component = new InventoryComponent();

            // Act
            component.Add(ItemTypes.Food, 5);
            component.Add(ItemTypes.Wood, 10);
            component.Add(ItemTypes.Stone, 3);

            // Assert
            Assert.Equal(5, component.Count(ItemTypes.Food));
            Assert.Equal(10, component.Count(ItemTypes.Wood));
            Assert.Equal(3, component.Count(ItemTypes.Stone));
        }
    }
}
