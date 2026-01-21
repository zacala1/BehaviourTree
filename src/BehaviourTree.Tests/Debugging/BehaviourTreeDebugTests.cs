using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Debugging;
using BehaviourTree.Decorators;
using NUnit.Framework;
using System.Linq;

namespace BehaviourTree.Tests.Debugging
{
    public class BehaviourTreeDebugTests
    {
        [Test]
        public void BuildStructure_SingleNode_ReturnsCorrectStructure()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("TestAction", _ => BehaviourStatus.Succeeded);

            // Act
            var structure = BehaviourTreeStructureBuilder.Build(action);

            // Assert
            Assert.AreEqual(action.Id, structure.RootId);
            Assert.AreEqual(1, structure.Nodes.Count);
            Assert.IsTrue(structure.Nodes.ContainsKey(action.Id));

            var nodeInfo = structure.Nodes[action.Id];
            Assert.AreEqual("TestAction", nodeInfo.Name);
            Assert.AreEqual(-1, nodeInfo.ParentId);
            Assert.AreEqual(0, nodeInfo.Depth);
            Assert.IsTrue(nodeInfo.IsLeaf);
            Assert.IsEmpty(nodeInfo.ChildIds);
        }

        [Test]
        public void BuildStructure_SequenceWithChildren_ReturnsCorrectHierarchy()
        {
            // Arrange
            var action1 = new ActionBehaviour<TestContext>("Action1", _ => BehaviourStatus.Succeeded);
            var action2 = new ActionBehaviour<TestContext>("Action2", _ => BehaviourStatus.Succeeded);
            var sequence = new Sequence<TestContext>("TestSequence", new IBehaviour<TestContext>[] { action1, action2 });

            // Act
            var structure = BehaviourTreeStructureBuilder.Build(sequence);

            // Assert
            Assert.AreEqual(sequence.Id, structure.RootId);
            Assert.AreEqual(3, structure.Nodes.Count);

            // Check sequence node
            var seqInfo = structure.Nodes[sequence.Id];
            Assert.AreEqual("TestSequence", seqInfo.Name);
            Assert.AreEqual(-1, seqInfo.ParentId);
            Assert.AreEqual(0, seqInfo.Depth);
            Assert.IsFalse(seqInfo.IsLeaf);
            Assert.AreEqual(2, seqInfo.ChildIds.Length);

            // Check children
            var action1Info = structure.Nodes[action1.Id];
            Assert.AreEqual(sequence.Id, action1Info.ParentId);
            Assert.AreEqual(1, action1Info.Depth);
            Assert.IsTrue(action1Info.IsLeaf);

            var action2Info = structure.Nodes[action2.Id];
            Assert.AreEqual(sequence.Id, action2Info.ParentId);
            Assert.AreEqual(1, action2Info.Depth);
            Assert.IsTrue(action2Info.IsLeaf);
        }

        [Test]
        public void BuildStructure_NestedTree_ReturnsCorrectDepths()
        {
            // Arrange - Create a tree: Sequence -> Selector -> Action
            var action = new ActionBehaviour<TestContext>("LeafAction", _ => BehaviourStatus.Succeeded);
            var selector = new Selector<TestContext>("InnerSelector", new IBehaviour<TestContext>[] { action });
            var sequence = new Sequence<TestContext>("RootSequence", new IBehaviour<TestContext>[] { selector });

            // Act
            var structure = BehaviourTreeStructureBuilder.Build(sequence);

            // Assert
            Assert.AreEqual(0, structure.Nodes[sequence.Id].Depth);
            Assert.AreEqual(1, structure.Nodes[selector.Id].Depth);
            Assert.AreEqual(2, structure.Nodes[action.Id].Depth);

            Assert.AreEqual(sequence.Id, structure.Nodes[selector.Id].ParentId);
            Assert.AreEqual(selector.Id, structure.Nodes[action.Id].ParentId);
        }

        [Test]
        public void BuildStructure_WithDecorator_ReturnsCorrectStructure()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("InnerAction", _ => BehaviourStatus.Succeeded);
            var inverter = new Inverter<TestContext>("TestInverter", action);

            // Act
            var structure = BehaviourTreeStructureBuilder.Build(inverter);

            // Assert
            Assert.AreEqual(2, structure.Nodes.Count);

            var inverterInfo = structure.Nodes[inverter.Id];
            Assert.IsFalse(inverterInfo.IsLeaf);
            Assert.AreEqual(1, inverterInfo.ChildIds.Length);
            Assert.AreEqual(action.Id, inverterInfo.ChildIds[0]);

            var actionInfo = structure.Nodes[action.Id];
            Assert.AreEqual(inverter.Id, actionInfo.ParentId);
            Assert.IsTrue(actionInfo.IsLeaf);
        }

        [Test]
        public void GetPathToNode_ReturnsCorrectPath()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("Leaf", _ => BehaviourStatus.Succeeded);
            var selector = new Selector<TestContext>("Inner", new IBehaviour<TestContext>[] { action });
            var sequence = new Sequence<TestContext>("Root", new IBehaviour<TestContext>[] { selector });
            var structure = BehaviourTreeStructureBuilder.Build(sequence);

            // Act
            var path = structure.GetPathToNode(action.Id);

            // Assert
            Assert.AreEqual(3, path.Count);
            Assert.AreEqual(sequence.Id, path[0]);
            Assert.AreEqual(selector.Id, path[1]);
            Assert.AreEqual(action.Id, path[2]);
        }

        [Test]
        public void GetActiveLeafIds_NoRunningNodes_ReturnsEmpty()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("Action", _ => BehaviourStatus.Succeeded);

            // Act - Node is in Ready state
            var activeLeafIds = action.GetActiveLeafIds();

            // Assert
            Assert.IsEmpty(activeLeafIds);
        }

        [Test]
        public void GetActiveLeafIds_SingleRunningLeaf_ReturnsLeafId()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("Action", _ => BehaviourStatus.Running);
            var context = new TestContext();

            // Act - Tick to get Running status
            action.Tick(context);
            var activeLeafIds = action.GetActiveLeafIds();

            // Assert
            Assert.AreEqual(1, activeLeafIds.Count);
            Assert.AreEqual(action.Id, activeLeafIds[0]);
        }

        [Test]
        public void GetActiveLeafIds_SequenceWithRunningChild_ReturnsCorrectLeaf()
        {
            // Arrange
            var action1 = new ActionBehaviour<TestContext>("Action1", _ => BehaviourStatus.Running);
            var action2 = new ActionBehaviour<TestContext>("Action2", _ => BehaviourStatus.Succeeded);
            var sequence = new Sequence<TestContext>("Seq", new IBehaviour<TestContext>[] { action1, action2 });
            var context = new TestContext();

            // Act
            sequence.Tick(context); // First child returns Running
            var activeLeafIds = sequence.GetActiveLeafIds();

            // Assert
            Assert.AreEqual(1, activeLeafIds.Count);
            Assert.AreEqual(action1.Id, activeLeafIds[0]);
        }

        [Test]
        public void GetAllActiveNodeIds_ReturnsFullPath()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("Leaf", _ => BehaviourStatus.Running);
            var selector = new Selector<TestContext>("Inner", new IBehaviour<TestContext>[] { action });
            var sequence = new Sequence<TestContext>("Root", new IBehaviour<TestContext>[] { selector });
            var structure = BehaviourTreeStructureBuilder.Build(sequence);
            var context = new TestContext();

            // Act
            sequence.Tick(context);
            var activeLeafIds = sequence.GetActiveLeafIds();
            var allActiveIds = structure.GetAllActiveNodeIds(activeLeafIds);

            // Assert
            Assert.AreEqual(3, allActiveIds.Count);
            Assert.IsTrue(allActiveIds.Contains(sequence.Id));
            Assert.IsTrue(allActiveIds.Contains(selector.Id));
            Assert.IsTrue(allActiveIds.Contains(action.Id));
        }

        [Test]
        public void GetRunningNodes_ReturnsAllRunningNodesInPath()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("Leaf", _ => BehaviourStatus.Running);
            var selector = new Selector<TestContext>("Inner", new IBehaviour<TestContext>[] { action });
            var sequence = new Sequence<TestContext>("Root", new IBehaviour<TestContext>[] { selector });
            var context = new TestContext();

            // Act
            sequence.Tick(context);
            var runningNodes = sequence.GetRunningNodes();

            // Assert
            Assert.AreEqual(3, runningNodes.Count);
            Assert.AreEqual("Root", runningNodes[0].Name);
            Assert.AreEqual("Inner", runningNodes[1].Name);
            Assert.AreEqual("Leaf", runningNodes[2].Name);
        }

        [Test]
        public void TypeName_UsedInStructure()
        {
            // Arrange
            var action = new ActionBehaviour<TestContext>("TestAction", _ => BehaviourStatus.Succeeded);

            // Act
            var structure = BehaviourTreeStructureBuilder.Build(action);

            // Assert - Should use the cached TypeName
            var nodeInfo = structure.Nodes[action.Id];
            Assert.IsTrue(nodeInfo.TypeName.Contains("Action"));
        }

        private class TestContext { }
    }
}
