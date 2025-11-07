using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using BehaviourTree.FluentBuilder;
using NUnit.Framework;

namespace BehaviourTree.Tests.FluentBuilder
{
    [TestFixture]
    internal sealed class LambdaBuilderSyntaxTests
    {
        private class TestContext : IClock
        {
            private long _timestamp;

            public bool ConditionResult { get; set; }
            public int ActionCallCount { get; set; }

            public long GetTimeStampInMilliseconds()
            {
                return _timestamp;
            }

            public void SetTimeStamp(long milliseconds)
            {
                _timestamp = milliseconds;
            }
        }

        [Test]
        public void WhenSequenceBuiltWithLambdaSyntax_BuildsCorrectlyWithNestedChildren()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("root", seq =>
                {
                    seq.Condition("check1", ctx => ctx.ConditionResult);
                    seq.Do("action1", ctx => BehaviourStatus.Succeeded);
                    seq.Condition("check2", ctx => true);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut, Is.InstanceOf<Sequence<TestContext>>());

            var sequence = sut as Sequence<TestContext>;
            Assert.That(sequence.Children.Length, Is.EqualTo(3));
            Assert.That(sequence.Name, Is.EqualTo("root"));
        }

        [Test]
        public void WhenSelectorBuiltWithLambdaSyntax_BuildsCorrectlyWithNestedChildren()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Selector("root", sel =>
                {
                    sel.Condition("check1", ctx => false);
                    sel.Do("action1", ctx => BehaviourStatus.Failed);
                    sel.Condition("check2", ctx => true);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut, Is.InstanceOf<Selector<TestContext>>());

            var selector = sut as Selector<TestContext>;
            Assert.That(selector.Children.Length, Is.EqualTo(3));
            Assert.That(selector.Name, Is.EqualTo("root"));
        }

        [Test]
        public void WhenNestedCompositesBuiltWithLambdaSyntax_BuildsHierarchyCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
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

            Assert.That(sut, Is.Not.Null);
            var rootSequence = sut as Sequence<TestContext>;
            Assert.That(rootSequence, Is.Not.Null);
            Assert.That(rootSequence.Children.Length, Is.EqualTo(3));

            var selector = rootSequence.Children[1] as Selector<TestContext>;
            Assert.That(selector, Is.Not.Null);
            Assert.That(selector.Name, Is.EqualTo("combat"));
            Assert.That(selector.Children.Length, Is.EqualTo(2));
        }

        [Test]
        public void WhenActiveSequenceBuiltWithLambdaSyntax_BuildsCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .ActiveSequence("active-seq", seq =>
                {
                    seq.Condition("check", ctx => true);
                    seq.Do("action", ctx => BehaviourStatus.Running);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut, Is.InstanceOf<ActiveSequence<TestContext>>());

            var activeSeq = sut as ActiveSequence<TestContext>;
            Assert.That(activeSeq.Children.Length, Is.EqualTo(2));
        }

        [Test]
        public void WhenActiveSelectorBuiltWithLambdaSyntax_BuildsCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .ActiveSelector("active-sel", sel =>
                {
                    sel.Condition("check", ctx => false);
                    sel.Do("action", ctx => BehaviourStatus.Running);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut, Is.InstanceOf<ActiveSelector<TestContext>>());

            var activeSel = sut as ActiveSelector<TestContext>;
            Assert.That(activeSel.Children.Length, Is.EqualTo(2));
        }

        [Test]
        public void WhenParallelBuiltWithLambdaSyntax_BuildsCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Parallel("parallel", ParallelPolicy.RequireAll, par =>
                {
                    par.Do("action1", ctx => BehaviourStatus.Succeeded);
                    par.Do("action2", ctx => BehaviourStatus.Succeeded);
                    par.Do("action3", ctx => BehaviourStatus.Succeeded);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut, Is.InstanceOf<Parallel<TestContext>>());

            var parallel = sut as Parallel<TestContext>;
            Assert.That(parallel.Children.Length, Is.EqualTo(3));
            Assert.That(parallel.Policy, Is.EqualTo(ParallelPolicy.RequireAll));
        }

        [Test]
        public void WhenRetryBuiltWithLambdaSyntax_BuildsCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Retry("retry", 3, retry =>
                {
                    retry.Do("action", ctx => BehaviourStatus.Failed);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut, Is.InstanceOf<Retry<TestContext>>());

            var retryNode = sut as Retry<TestContext>;
            Assert.That(retryNode.RetryCount, Is.EqualTo(3));
            Assert.That(retryNode.Child, Is.Not.Null);
        }

        [Test]
        public void WhenRepeatBuiltWithLambdaSyntax_BuildsCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Repeat("repeat", 5, rep =>
                {
                    rep.Do("action", ctx => BehaviourStatus.Succeeded);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut, Is.InstanceOf<Repeater<TestContext>>());

            var repeatNode = sut as Repeater<TestContext>;
            Assert.That(repeatNode.RepeatCount, Is.EqualTo(5));
            Assert.That(repeatNode.Child, Is.Not.Null);
        }

        [Test]
        public void WhenInverterBuiltWithLambdaSyntax_BuildsCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Invert("inverter", inv =>
                {
                    inv.Condition("check", ctx => true);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut, Is.InstanceOf<Inverter<TestContext>>());

            var inverter = sut as Inverter<TestContext>;
            Assert.That(inverter.Child, Is.Not.Null);
        }

        [Test]
        public void WhenTimeLimitBuiltWithLambdaSyntax_BuildsCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .TimeLimit("time-limit", 1000, tl =>
                {
                    tl.Do("action", ctx => BehaviourStatus.Running);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut, Is.InstanceOf<TimeLimiter<TestContext>>());

            var timeLimit = sut as TimeLimiter<TestContext>;
            Assert.That(timeLimit.TimeLimitInMilliseconds, Is.EqualTo(1000));
            Assert.That(timeLimit.Child, Is.Not.Null);
        }

        [Test]
        public void WhenTreeBuiltWithLambdaSyntax_ExecutesCorrectly()
        {
            var context = new TestContext { ConditionResult = true, ActionCallCount = 0 };

            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
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

            var status = sut.Tick(context);

            Assert.That(status, Is.EqualTo(BehaviourStatus.Succeeded));
            Assert.That(context.ActionCallCount, Is.EqualTo(1));
        }

        [Test]
        public void WhenLambdaSyntaxMixedWithTraditionalSyntax_BuildsCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
                .Sequence("root", seq =>
                {
                    seq.Condition("check1", ctx => true);

                    seq.Selector("traditional-selector")
                        .Do("action1", ctx => BehaviourStatus.Failed)
                        .Do("action2", ctx => BehaviourStatus.Succeeded)
                    .End();

                    seq.Condition("check2", ctx => true);
                })
                .Build();

            Assert.That(sut, Is.Not.Null);
            var rootSeq = sut as Sequence<TestContext>;
            Assert.That(rootSeq.Children.Length, Is.EqualTo(3));

            var selector = rootSeq.Children[1] as Selector<TestContext>;
            Assert.That(selector, Is.Not.Null);
            Assert.That(selector.Children.Length, Is.EqualTo(2));
        }

        [Test]
        public void WhenDeepNestingUsesLambdaSyntax_BuildsCorrectly()
        {
            var sut = BehaviourTree.FluentBuilder.FluentBuilder.Create<TestContext>()
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

            Assert.That(sut, Is.Not.Null);
            var level1 = sut as Sequence<TestContext>;
            Assert.That(level1, Is.Not.Null);

            var level2 = level1.Children[0] as Selector<TestContext>;
            Assert.That(level2, Is.Not.Null);

            var level3 = level2.Children[0] as Sequence<TestContext>;
            Assert.That(level3, Is.Not.Null);
            Assert.That(level3.Children.Length, Is.EqualTo(1));
        }
    }
}
