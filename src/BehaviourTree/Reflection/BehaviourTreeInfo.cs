using System.Collections.Generic;

namespace BehaviourTree.Reflection
{
    /// <summary>
    /// Provides reflection information about a behavior tree node and its hierarchy.
    /// Used for visualization, debugging, and runtime inspection of behavior trees.
    /// </summary>
    public sealed class BehaviourTreeInfo
    {
        /// <summary>
        /// Creates a new behavior tree info instance.
        /// </summary>
        /// <param name="name">Name of the behavior node</param>
        /// <param name="id">Unique identifier of the node</param>
        /// <param name="nodeType">Type classification of the node</param>
        internal BehaviourTreeInfo(string name, int id, TreeNodeType nodeType)
        {
            Name = name;
            Id = id;
            NodeType = nodeType;
            ChildrenTreeInfos = new List<BehaviourTreeInfo>();
        }

        /// <summary>
        /// Exposes the id of the behavior tree node
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Exposes the name of the behavior tree node.
        /// Setter added // Han Tae-kwang / 2022.03.17
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Exposes the node type such as composites, decorates, leaves
        /// </summary>
        public TreeNodeType NodeType { get; }

        /// <summary>
        /// Node expand/collapse status (used in UI) // Han Tae-kwang / 2022.03.17
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Exposes the specific type of the node
        /// </summary>
        public string NodeTypeSpecific { get; internal set; } = default!;

        /// <summary>
        /// Exposes the status of the behavior tree node
        /// </summary>
        public BehaviourStatus Status { get; internal set; }

        /// <summary>
        /// Exposes the depth of the node tree
        /// </summary>
        public int Depth { get; internal set; }

        /// <summary>
        /// Exposes the parent node of the behavior tree node
        /// </summary>
        public BehaviourTreeInfo? Parent { get; internal set; }

        /// <summary>
        /// Exposes the children nodes of the behavior tree node
        /// </summary>
        public IEnumerable<BehaviourTreeInfo> Children => ChildrenTreeInfos;

        internal IList<BehaviourTreeInfo> ChildrenTreeInfos { get; }
    }
}