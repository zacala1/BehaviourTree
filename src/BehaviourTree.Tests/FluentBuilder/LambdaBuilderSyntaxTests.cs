using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using BehaviourTree.FluentBuilder;
using NUnit.Framework;

namespace BehaviourTree.Tests.FluentBuilder
{
    [TestFixture]
    public class LambdaBuilderSyntaxTests
    {
        private class TestContext
        {
            public bool ConditionResult { get; set; }
            public int ActionCallCount { get; set; }
        }

        [Test]
        public void LambdaSequence_BuildsCorrectly_WithNestedChildren()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("root", seq =>
                {
                    seq.Condition("check1", ctx => ctx.ConditionResult);
                    seq.Do("action1", ctx => BehaviourStatus.Succeeded);
                    seq.Condition("check2", ctx => true);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            Assert.IsInstanceOf<Sequence<TestContext>>(tree);

            var sequence = tree as Sequence<TestContext>;
            Assert.AreEqual(3, sequence.Children.Length);
            Assert.AreEqual("root", sequence.Name);
        }

        [Test]
        public void LambdaSelector_BuildsCorrectly_WithNestedChildren()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Selector("root", sel =>
                {
                    sel.Condition("check1", ctx => false);
                    sel.Do("action1", ctx => BehaviourStatus.Failed);
                    sel.Condition("check2", ctx => true);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            Assert.IsInstanceOf<Selector<TestContext>>(tree);

            var selector = tree as Selector<TestContext>;
            Assert.AreEqual(3, selector.Children.Length);
            Assert.AreEqual("root", selector.Name);
        }

        [Test]
        public void LambdaNestedComposites_BuildsHierarchyCorrectly()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("patrol-sequence", seq =>
                {
                    seq.Condition("has-target", ctx => ctx.ConditionResult);
                    seq.Selector("combat", sel =>
                    {
                        sel.Do("attack", ctx => BehaviourStatus.Succeeded);
                        sel.Do("retreat", ctx => BehaviourStatus.Failed);
                    });
                    seq.Do("patrol", ctx => BehaviourStatus.Succeeded);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            var rootSequence = tree as Sequence<TestContext>;
            Assert.IsNotNull(rootSequence);
            Assert.AreEqual(3, rootSequence.Children.Length);

            // Check nested selector
            var selector = rootSequence.Children[1] as Selector<TestContext>;
            Assert.IsNotNull(selector);
            Assert.AreEqual("combat", selector.Name);
            Assert.AreEqual(2, selector.Children.Length);
        }

        [Test]
        public void LambdaActiveSequence_BuildsCorrectly()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .ActiveSequence("active-seq", seq =>
                {
                    seq.Condition("check", ctx => true);
                    seq.Do("action", ctx => BehaviourStatus.Running);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            Assert.IsInstanceOf<ActiveSequence<TestContext>>(tree);

            var activeSeq = tree as ActiveSequence<TestContext>;
            Assert.AreEqual(2, activeSeq.Children.Length);
        }

        [Test]
        public void LambdaActiveSelector_BuildsCorrectly()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .ActiveSelector("active-sel", sel =>
                {
                    sel.Condition("check", ctx => false);
                    sel.Do("action", ctx => BehaviourStatus.Running);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            Assert.IsInstanceOf<ActiveSelector<TestContext>>(tree);

            var activeSel = tree as ActiveSelector<TestContext>;
            Assert.AreEqual(2, activeSel.Children.Length);
        }

        [Test]
        public void LambdaParallel_BuildsCorrectly()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Parallel("parallel", ParallelPolicy.RequireAll, par =>
                {
                    par.Do("action1", ctx => BehaviourStatus.Succeeded);
                    par.Do("action2", ctx => BehaviourStatus.Succeeded);
                    par.Do("action3", ctx => BehaviourStatus.Succeeded);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            Assert.IsInstanceOf<Parallel<TestContext>>(tree);

            var parallel = tree as Parallel<TestContext>;
            Assert.AreEqual(3, parallel.Children.Length);
            Assert.AreEqual(ParallelPolicy.RequireAll, parallel.Policy);
        }

        [Test]
        public void LambdaRetry_BuildsCorrectly()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Retry("retry", 3, retry =>
                {
                    retry.Do("action", ctx => BehaviourStatus.Failed);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            Assert.IsInstanceOf<Retry<TestContext>>(tree);

            var retryNode = tree as Retry<TestContext>;
            Assert.AreEqual(3, retryNode.RetryCount);
            Assert.IsNotNull(retryNode.Child);
        }

        [Test]
        public void LambdaRepeat_BuildsCorrectly()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Repeat("repeat", 5, rep =>
                {
                    rep.Do("action", ctx => BehaviourStatus.Succeeded);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            Assert.IsInstanceOf<Repeat<TestContext>>(tree);

            var repeatNode = tree as Repeat<TestContext>;
            Assert.AreEqual(5, repeatNode.RepeatCount);
            Assert.IsNotNull(repeatNode.Child);
        }

        [Test]
        public void LambdaInvert_BuildsCorrectly()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Invert("inverter", inv =>
                {
                    inv.Condition("check", ctx => true);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            Assert.IsInstanceOf<Inverter<TestContext>>(tree);

            var inverter = tree as Inverter<TestContext>;
            Assert.IsNotNull(inverter.Child);
        }

        [Test]
        public void LambdaTimeLimit_BuildsCorrectly()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .TimeLimit("time-limit", 1000, tl =>
                {
                    tl.Do("action", ctx => BehaviourStatus.Running);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            Assert.IsInstanceOf<TimeLimit<TestContext>>(tree);

            var timeLimit = tree as TimeLimit<TestContext>;
            Assert.AreEqual(1000, timeLimit.TimeLimitInMilliseconds);
            Assert.IsNotNull(timeLimit.Child);
        }

        [Test]
        public void LambdaSyntax_ExecutesCorrectly()
        {
            // Arrange
            var context = new TestContext { ConditionResult = true, ActionCallCount = 0 };

            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("root", seq =>
                {
                    seq.Condition("check", ctx => ctx.ConditionResult);
                    seq.Do("increment", ctx =>
                    {
                        ctx.ActionCallCount++;
                        return BehaviourStatus.Succeeded;
                    });
                })
                .Build();

            // Act
            var status = tree.Tick(context);

            // Assert
            Assert.AreEqual(BehaviourStatus.Succeeded, status);
            Assert.AreEqual(1, context.ActionCallCount);
        }

        [Test]
        public void LambdaSyntax_CanMixWithTraditionalSyntax()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("root", seq =>
                {
                    seq.Condition("check1", ctx => true);

                    // Traditional syntax within lambda
                    seq.Selector("traditional-selector")
                        .Do("action1", ctx => BehaviourStatus.Failed)
                        .Do("action2", ctx => BehaviourStatus.Succeeded)
                    .End();

                    seq.Condition("check2", ctx => true);
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            var rootSeq = tree as Sequence<TestContext>;
            Assert.AreEqual(3, rootSeq.Children.Length);

            var selector = rootSeq.Children[1] as Selector<TestContext>;
            Assert.IsNotNull(selector);
            Assert.AreEqual(2, selector.Children.Length);
        }

        [Test]
        public void LambdaSyntax_DeepNesting_BuildsCorrectly()
        {
            // Arrange & Act
            var tree = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("level1", seq1 =>
                {
                    seq1.Selector("level2", sel =>
                    {
                        sel.Sequence("level3", seq2 =>
                        {
                            seq2.Do("action", ctx => BehaviourStatus.Succeeded);
                        });
                    });
                })
                .Build();

            // Assert
            Assert.IsNotNull(tree);
            var level1 = tree as Sequence<TestContext>;
            Assert.IsNotNull(level1);

            var level2 = level1.Children[0] as Selector<TestContext>;
            Assert.IsNotNull(level2);

            var level3 = level2.Children[0] as Sequence<TestContext>;
            Assert.IsNotNull(level3);
            Assert.AreEqual(1, level3.Children.Length);
        }
    }
}
