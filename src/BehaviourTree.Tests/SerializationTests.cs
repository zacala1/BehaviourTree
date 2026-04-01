using BehaviourTree.Serialization;
using BehaviourTree.Tests.Utils;
using NUnit.Framework;

namespace BehaviourTree.Tests
{
    [TestFixture]
    internal sealed class SerializationTests
    {
        [Test]
        public void Deserializer_BuildsSequence_WithActions()
        {
            var deserializer = new BehaviourTreeDeserializer<MockContext>()
                .RegisterAction("doA", ctx => BehaviourStatus.Succeeded)
                .RegisterAction("doB", ctx => BehaviourStatus.Succeeded);

            var tree = deserializer.Build(new NodeDescriptor
            {
                Type = "Sequence",
                Category = "composite",
                Name = "Root",
                Children = new[]
                {
                    new NodeDescriptor { Type = "Action", Category = "leaf", ActionRef = "doA", Name = "ActionA" },
                    new NodeDescriptor { Type = "Action", Category = "leaf", ActionRef = "doB", Name = "ActionB" }
                }
            });

            var result = tree.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void Deserializer_BuildsSelector_FailsThenSucceeds()
        {
            var deserializer = new BehaviourTreeDeserializer<MockContext>()
                .RegisterAction("fail", ctx => BehaviourStatus.Failed)
                .RegisterAction("succeed", ctx => BehaviourStatus.Succeeded);

            var tree = deserializer.Build(new NodeDescriptor
            {
                Type = "Selector",
                Category = "composite",
                Name = "Root",
                Children = new[]
                {
                    new NodeDescriptor { Type = "Action", Category = "leaf", ActionRef = "fail" },
                    new NodeDescriptor { Type = "Action", Category = "leaf", ActionRef = "succeed" }
                }
            });

            var result = tree.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void Deserializer_BuildsCondition()
        {
            var deserializer = new BehaviourTreeDeserializer<MockContext>()
                .RegisterCondition("isTrue", ctx => true);

            var tree = deserializer.Build(new NodeDescriptor
            {
                Type = "Condition",
                Category = "leaf",
                ConditionRef = "isTrue"
            });

            var result = tree.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Succeeded));
        }

        [Test]
        public void Deserializer_BuildsDecorator_Inverter()
        {
            var deserializer = new BehaviourTreeDeserializer<MockContext>()
                .RegisterAction("succeed", ctx => BehaviourStatus.Succeeded);

            var tree = deserializer.Build(new NodeDescriptor
            {
                Type = "Inverter",
                Category = "decorator",
                Children = new[]
                {
                    new NodeDescriptor { Type = "Action", Category = "leaf", ActionRef = "succeed" }
                }
            });

            var result = tree.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed));
        }

        [Test]
        public void Deserializer_NestedTree()
        {
            var callCount = 0;
            var deserializer = new BehaviourTreeDeserializer<MockContext>()
                .RegisterCondition("check", ctx => true)
                .RegisterAction("work", ctx => { callCount++; return BehaviourStatus.Succeeded; });

            var tree = deserializer.Build(new NodeDescriptor
            {
                Type = "Sequence",
                Category = "composite",
                Children = new[]
                {
                    new NodeDescriptor { Type = "Condition", Category = "leaf", ConditionRef = "check" },
                    new NodeDescriptor
                    {
                        Type = "Inverter",
                        Category = "decorator",
                        Children = new[]
                        {
                            new NodeDescriptor
                            {
                                Type = "Selector",
                                Category = "composite",
                                Children = new[]
                                {
                                    new NodeDescriptor { Type = "Action", Category = "leaf", ActionRef = "work" }
                                }
                            }
                        }
                    }
                }
            });

            var result = tree.Tick(new MockContext());
            Assert.That(result, Is.EqualTo(BehaviourStatus.Failed)); // Inverter flips Succeeded
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Serializer_ProducesJson()
        {
            var deserializer = new BehaviourTreeDeserializer<MockContext>()
                .RegisterAction("doA", ctx => BehaviourStatus.Succeeded);

            var tree = deserializer.Build(new NodeDescriptor
            {
                Type = "Sequence",
                Category = "composite",
                Name = "Root",
                Children = new[]
                {
                    new NodeDescriptor { Type = "Action", Category = "leaf", ActionRef = "doA", Name = "ActionA" }
                }
            });

            var json = BehaviourTreeSerializer.ToJson(tree);
            Assert.That(json, Does.Contain("\"name\": \"Root\""));
            Assert.That(json, Does.Contain("\"name\": \"ActionA\""));
            Assert.That(json, Does.Contain("\"category\": \"composite\""));
            Assert.That(json, Does.Contain("\"children\""));
        }

        [Test]
        public void Deserializer_UnknownAction_Throws()
        {
            var deserializer = new BehaviourTreeDeserializer<MockContext>();

            Assert.Throws<System.InvalidOperationException>(() =>
            {
                deserializer.Build(new NodeDescriptor
                {
                    Type = "Action",
                    Category = "leaf",
                    ActionRef = "nonexistent"
                });
            });
        }
    }
}
