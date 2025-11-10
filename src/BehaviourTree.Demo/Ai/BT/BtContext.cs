using BehaviourTree.Demo.GameEngine;

namespace BehaviourTree.Demo.Ai.BT
{
    public sealed class BtContext : IClock
    {
        private long _timeStampInMilliseconds;

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
            _timeStampInMilliseconds = timeStampInMilliseconds;
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
            _timeStampInMilliseconds = 0;
        }

        public long GetTimeStampInMilliseconds()
        {
            return _timeStampInMilliseconds;
        }
    }
}