using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using BehaviourTree.FluentBuilder;
using BehaviourTree.Graph;
using NUnit.Framework;

namespace BehaviourTree.Graph.Tests
{
    internal class TestContext { }

    [TestFixture]
    internal sealed class BehaviourTreeGraphTests
    {
        [Test]
        public void Format_WithPlantuml_ReturnsValidPlantumlString()
        {
            var tree = CreateSimpleTree();

            var result = BehaviourTreeGraph.Format(tree, BehaviourTreeGraph.FormatOptions.Plantuml);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("@startmindmap"));
            Assert.That(result, Does.Contain("@endmindmap"));
        }

        [Test]
        public void Format_WithDanAbad_ReturnsValidDanAbadString()
        {
            var tree = CreateSimpleTree();

            var result = BehaviourTreeGraph.Format(tree, BehaviourTreeGraph.FormatOptions.DanAbad);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("?"));  // Selector marker
        }

        [Test]
        public void Format_WithUnknownOption_ReturnsNull()
        {
            var tree = CreateSimpleTree();

            var result = BehaviourTreeGraph.Format(tree, (BehaviourTreeGraph.FormatOptions)999);

            Assert.That(result, Is.Null);
        }

        private static IBehaviour<TestContext> CreateSimpleTree()
        {
            return BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Selector("Root")
                    .Condition("CheckCondition", _ => true)
                    .Do("DoAction", _ => BehaviourStatus.Succeeded)
                .End()
                .Build();
        }
    }
}
