namespace BehaviourTree
{
    /// <summary>
    /// Interface for nodes that can signal their parent to re-evaluate
    /// when an observed condition changes (LowerPriority abort support).
    /// </summary>
    public interface IAbortableObserver
    {
        /// <summary>
        /// Returns true if this node's condition has changed and wants
        /// the parent composite to re-evaluate from the beginning.
        /// </summary>
        bool IsAbortRequested { get; }

        /// <summary>
        /// Clears the abort request flag after the parent has processed it.
        /// </summary>
        void ClearAbortRequest();
    }
}
