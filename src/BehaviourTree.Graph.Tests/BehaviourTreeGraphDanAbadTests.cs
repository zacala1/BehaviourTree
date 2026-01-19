using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using BehaviourTree.FluentBuilder;
using BehaviourTree.Graph;
using NUnit.Framework;

namespace BehaviourTree.Graph.Tests
{
    [TestFixture]
    internal sealed class BehaviourTreeGraphDanAbadTests
    {
        [Test]
        public void Format_WithSelector_ContainsSelectorMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Selector("TestSelector")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.StartWith("?"));
        }

        [Test]
        public void Format_WithSequence_ContainsSequenceMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("TestSequence")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.StartWith("->"));
        }

        [Test]
        public void Format_WithCondition_ContainsConditionInParentheses()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("Root")
                    .Condition("MyCondition", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("(MyCondition)"));
        }

        [Test]
        public void Format_WithAction_ContainsActionInBrackets()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("Root")
                    .Do("MyAction", _ => BehaviourStatus.Succeeded)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("[MyAction]"));
        }

        [Test]
        public void Format_WithParallel_ContainsParallelMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Parallel("TestParallel", 2)
                    .Condition("Cond1", _ => true)
                    .Condition("Cond2", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("=2"));
        }

        [Test]
        public void Format_WithWait_ContainsWaitMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("Root")
                    .Wait("TestWait", 1000)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("[TestWait:wait]"));
        }

        [Test]
        public void Format_WithDecorator_ContainsDecoratorComment()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Invert("TestInverter")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("// Invert"));
        }

        [Test]
        public void Format_WithRetryDecorator_ContainsRetryInfo()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Retry("TestRetry", 3)
                    .Condition("Cond", _ => false)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("Retry(3)"));
        }

        [Test]
        public void Format_WithRepeaterDecorator_ContainsRepeatInfo()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Repeat("TestRepeater", 5)
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("Repeat(5)"));
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

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            // Should have proper indentation with |    pattern
            var lines = result.Split('\n');

            // Root should have no indentation
            Assert.That(lines[0], Does.StartWith("?"));

            // Child should have one level of indentation
            Assert.That(lines[1], Does.StartWith("|    ->"));

            // Leaf should have two levels of indentation
            Assert.That(lines[2], Does.StartWith("|    |    (Leaf)"));
        }

        [Test]
        public void Format_WithRandomSelector_ContainsSelectorMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .RandomSelector("TestRandomSelector")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            // RandomSelector is rendered as "?" in Dan Abad format
            Assert.That(result, Does.StartWith("?"));
        }

        [Test]
        public void Format_WithAsyncAction_ContainsAsyncMarker()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("Root")
                    .DoAsync("MyAsyncAction", async (_, ct) => BehaviourStatus.Succeeded)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("[MyAsyncAction:async]"));
        }

        [Test]
        public void Format_WithCooldownDecorator_ContainsCooldownInfo()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Cooldown("TestCooldown", 5000)
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("Cooldown(5000ms)"));
        }

        [Test]
        public void Format_WithUntilSuccessDecorator_ContainsUntilSuccessInfo()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .UntilSuccess("TestUntilSuccess")
                    .Condition("Cond", _ => false)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("UntilSuccess"));
        }

        [Test]
        public void Format_WithFailerDecorator_ContainsAlwaysFailInfo()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .AlwaysFail("TestFailer")
                    .Condition("Cond", _ => true)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("AlwaysFail"));
        }

        [Test]
        public void Format_WithSucceederDecorator_ContainsAlwaysSucceedInfo()
        {
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .AlwaysSucceed("TestSucceeder")
                    .Condition("Cond", _ => false)
                .End()
                .Build();

            var result = BehaviourTreeGraphDanAbad.Format(tree);

            Assert.That(result, Does.Contain("AlwaysSucceed"));
        }
    }
}
