using System;
using System.Linq;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    [TestFixture]
    internal sealed class ArrayExtensions
    {
        [Test]
        public void ShuffleInPlace_ShouldShuffleTheArrayInPlace()
        {
            var original = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            var copy = original.ToArray();

            copy.ShuffleInPlace(new RandomProvider());

            Console.WriteLine($"original: {string.Join(",", original)}");
            Console.WriteLine($"shuffled: {string.Join(",", copy)}");

            // Verify elements are the same (just reordered)
            CollectionAssert.AreEquivalent(original, copy);
        }
    }
}
