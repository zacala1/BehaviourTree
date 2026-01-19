using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using BehaviourTree.FluentBuilder;
using BehaviourTree.Graph;
using NUnit.Framework;

namespace BehaviourTree.Graph.Tests
{
    [TestFixture]
    internal sealed class BehaviourTreeGraphPlantumlTests
    {
        [Test]
        public void Format_WithSelector_ContainsSelectorMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Selector("TestSelector")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("[?]"));
            Assert.That(result, Does.Contain("TestSelector"));
        }

        [Test]
        public void Format_WithSequence_ContainsSequenceMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("TestSequence")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("[->]"));
            Assert.That(result, Does.Contain("TestSequence"));
        }

        [Test]
        public void Format_WithCondition_ContainsConditionMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("Root")
                    .Condition("MyCondition", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("(?)"));
            Assert.That(result, Does.Contain("MyCondition"));
        }

        [Test]
        public void Format_WithAction_ContainsActionMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("Root")
                    .Do("MyAction", _ => BehaviourStatus.Succeeded)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("(!)"));
            Assert.That(result, Does.Contain("MyAction"));
        }

        [Test]
        public void Format_WithInverter_ContainsInverterMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Invert("TestInverter")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("<!>"));
        }

        [Test]
        public void Format_WithSucceeder_ContainsSucceederMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .AlwaysSucceed("TestSucceeder")
                    .Condition("Cond", _ => false)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("<✓>"));
        }

        [Test]
        public void Format_WithRetry_ContainsRetryInfo()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Retry("TestRetry", 3)
                    .Condition("Cond", _ => false)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("Retry:3"));
        }

        [Test]
        public void Format_WithRepeater_ContainsRepeatInfo()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Repeat("TestRepeater", 5)
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("Repeat:5"));
        }

        [Test]
        public void Format_WithParallel_ContainsParallelInfo()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Parallel("TestParallel", 2)
                    .Condition("Cond1", _ => true)
                    .Condition("Cond2", _ => true)
                    .Condition("Cond3", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("[=2/3]"));
        }

        [Test]
        public void Format_WithRandomSelector_ContainsRandomSelectorMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .RandomSelector("TestRandomSelector")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("[?R]"));
        }

        [Test]
        public void Format_WithActiveSelector_ContainsActiveSelectorMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .ActiveSelector("TestActiveSelector")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            Assert.That(result, Does.Contain("[?A]"));
        }

        [Test]
        public void Format_IncludesNodeIds()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("Root")
                    .Condition("TestNode", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            // Should contain the node ID in the format 'ID'
            Assert.That(result, Does.Match(@"'\d+'"));
        }

        [Test]
        public void Format_NestedStructure_HasCorrectIndentation()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Selector("Root")
                    .Sequence("Child1")
                        .Condition("Leaf", _ => true)
                    .End()
                .End()
                .Build();

            var result = BehaviourTreeGraphPlantuml.Format(tree);

            // Root should have 1 asterisk, Child1 should have 2, Leaf should have 3
            Assert.That(result, Does.Contain("* **[?]**"));   // Root level
            Assert.That(result, Does.Contain("** **[->]**")); // Child level
            Assert.That(result, Does.Contain("*** **(?)**")); // Leaf level
        }
    }
}
