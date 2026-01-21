namespace BehaviourTree
{
    /// <summary>
    /// Core interface for all behavior tree nodes.
    /// Provides lifecycle management (Tick, Reset).
    /// </summary>
    public interface IBehaviour<in TContext>
    {
        /// <summary>
        /// Unique identifier for this behavior node.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Human-readable name for this behavior node.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Current execution status of this behavior node.
        /// </summary>
        BehaviourStatus Status { get; }

        /// <summary>
        /// Executes one tick of this behavior node.
        /// </summary>
        /// <param name="context">Context object containing shared state</param>
        /// <returns>The resulting status after this tick</returns>
        BehaviourStatus Tick(TContext context);

        /// <summary>
        /// Resets this behavior node back to Ready status.
        /// </summary>
        void Reset();
    }
}
