namespace BehaviourTree.Reflection
{
    public enum TreeNodeType
    {
        Composite,
        Composite_Parallel,
        Composite_Sequence,
        Composite_Selector,
        Decorate,
        Leaf,
        Leaf_Action,
        Leaf_Condition,
        Leaf_Wait
    }
}
