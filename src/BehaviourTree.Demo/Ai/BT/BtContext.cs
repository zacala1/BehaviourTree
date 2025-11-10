using BehaviourTree.Demo.GameEngine;

namespace BehaviourTree.Demo.Ai.BT
{
    /// <summary>
    /// Behavior tree context optimized for high-performance real-time AI.
    /// Uses direct field access to minimize virtual call overhead.
    /// </summary>
    public sealed class BtContext : IClock
    {
        /// <summary>
        /// Direct field access for timestamp (avoids virtual call overhead).
        /// Use this field directly in hot paths instead of GetTimeStampInMilliseconds().
        /// </summary>
        public long TimeStampInMilliseconds;

        public BtContext()
        {
        }

        public BtContext(Entity agent, Engine engine, long timeStampInMilliseconds)
        {
            Initialize(agent, engine, timeStampInMilliseconds);
        }

        public Entity Agent { get; private set; } = null!;

        public Engine Engine { get; private set; } = null!;

        /// <summary>
        /// Initializes or reinitializes the context for pooling.
        /// </summary>
        public void Initialize(Entity agent, Engine engine, long timeStampInMilliseconds)
        {
            TimeStampInMilliseconds = timeStampInMilliseconds;
            Agent = agent;
            Engine = engine;
        }

        /// <summary>
        /// Resets the context state for returning to the pool.
        /// </summary>
        public void Reset()
        {
            Agent = null!;
            Engine = null!;
            TimeStampInMilliseconds = 0;
        }

        /// <summary>
        /// IClock interface implementation (for generic constraints).
        /// For performance-critical code, use TimeStampInMilliseconds field directly.
        /// </summary>
        public long GetTimeStampInMilliseconds()
        {
            return TimeStampInMilliseconds;
        }
    }
}