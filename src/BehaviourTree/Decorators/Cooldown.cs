using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that enforces a cooldown period after the child succeeds.
    /// During cooldown, returns Failed without executing the child.
    /// </summary>
    /// <typeparam name="TContext">Type of context used during execution</typeparam>
    public sealed class Cooldown<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, long>? _getCooldownTimeInMilliseconds;
        private long _cooldownTimeInMilliseconds;
        private long _cooldownStartedTimestamp;
        private bool _onCooldown;

        /// <summary>
        /// Gets the cooldown duration in milliseconds.
        /// </summary>
        public long CooldownTimeInMilliseconds => _cooldownTimeInMilliseconds;

        /// <summary>
        /// Gets whether this node is currently on cooldown.
        /// </summary>
        public bool OnCooldown => _onCooldown;

        /// <summary>
        /// Creates a cooldown decorator with dynamic cooldown time.
        /// </summary>
        /// <param name="child">Child node to execute</param>
        /// <param name="getCooldownTimeInMilliseconds">Function to get cooldown duration from context</param>
        public Cooldown(IBehaviour<TContext> child, Func<TContext, long> getCooldownTimeInMilliseconds)
            : this("Cooldown", child, getCooldownTimeInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a cooldown decorator with dynamic cooldown time and specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child node to execute</param>
        /// <param name="getCooldownTimeInMilliseconds">Function to get cooldown duration from context</param>
        /// <exception cref="ArgumentNullException">Thrown when getCooldownTimeInMilliseconds is null</exception>
        public Cooldown(string name, IBehaviour<TContext> child, Func<TContext, long> getCooldownTimeInMilliseconds)
            : base(name, child)
        {
            _getCooldownTimeInMilliseconds = getCooldownTimeInMilliseconds ?? throw new ArgumentNullException(nameof(getCooldownTimeInMilliseconds));
        }

        /// <summary>
        /// Creates a cooldown decorator with fixed cooldown time.
        /// </summary>
        /// <param name="child">Child node to execute</param>
        /// <param name="cooldownTimeInMilliseconds">Fixed cooldown duration in milliseconds</param>
        public Cooldown(IBehaviour<TContext> child, int cooldownTimeInMilliseconds)
            : this("Cooldown", child, cooldownTimeInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a cooldown decorator with fixed cooldown time and specified name.
        /// </summary>
        /// <param name="name">Node name for debugging</param>
        /// <param name="child">Child node to execute</param>
        /// <param name="cooldownTimeInMilliseconds">Fixed cooldown duration in milliseconds</param>
        public Cooldown(string name, IBehaviour<TContext> child, int cooldownTimeInMilliseconds)
            : base(name, child)
        {
            _cooldownTimeInMilliseconds = cooldownTimeInMilliseconds;
        }

        /// <summary>
        /// Core update logic for this node.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            return _onCooldown ? CooldownBehaviour(context) : RegularBehaviour(context);
        }

        [System.Diagnostics.DebuggerStepThrough]
        private BehaviourStatus RegularBehaviour(TContext context)
        {
            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Succeeded)
            {
                EnterCooldown(context);
            }

            return childStatus;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private BehaviourStatus CooldownBehaviour(TContext context)
        {
            var currentTimeStamp = GetCurrentTimestamp(context);

            var elapsedMilliseconds = currentTimeStamp - _cooldownStartedTimestamp;

            if (elapsedMilliseconds >= _cooldownTimeInMilliseconds)
            {
                ExitCooldown();

                return RegularBehaviour(context);
            }

            return BehaviourStatus.Failed;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private long GetCurrentTimestamp(TContext context)
        {
            // Try to get timestamp from context if it implements IClock (backward compatibility)
            if (context is IClock clock)
            {
                return clock.GetTimeStampInMilliseconds();
            }

            // Otherwise use global TimeProvider
            return TimeProvider.GetTimestampInMilliseconds();
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void ExitCooldown()
        {
            _onCooldown = false;
            _cooldownStartedTimestamp = 0;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void EnterCooldown(TContext context)
        {
            _onCooldown = true;
            _cooldownStartedTimestamp = GetCurrentTimestamp(context);
        }

        /// <summary>
        /// Called on first tick.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            if (_getCooldownTimeInMilliseconds != null)
            {
                _cooldownTimeInMilliseconds = _getCooldownTimeInMilliseconds.Invoke(context);
            }
        }
    }
}