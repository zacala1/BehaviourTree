using System;

namespace BehaviourTree.Decorators
{
    /// <summary>
    /// Decorator that enforces a cooldown period after child succeeds, renewed on initialization.
    /// Returns failure during cooldown, otherwise executes child normally.
    /// </summary>
    /// <typeparam name="TContext">Context type used in the behavior tree</typeparam>
    public sealed class CooldownRenew<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, long> _getCooldownTimeInMilliseconds;
        private long _cooldownTimeInMilliseconds;
        private long _cooldownStartedTimestamp;
        private bool _onCooldown;

        /// <summary>
        /// Gets the cooldown duration in milliseconds.
        /// </summary>
        public long CooldownTimeInMilliseconds => _cooldownTimeInMilliseconds;

        /// <summary>
        /// Gets whether the decorator is currently on cooldown.
        /// </summary>
        public bool OnCooldown => _onCooldown;

        /// <summary>
        /// Creates a new CooldownRenew decorator with a dynamic cooldown time.
        /// </summary>
        /// <param name="child">Child behavior to apply cooldown to</param>
        /// <param name="getCooldownTimeInMilliseconds">Function to get cooldown time from context</param>
        public CooldownRenew(IBehaviour<TContext> child, Func<TContext, long> getCooldownTimeInMilliseconds) : this("Cooldown", child, getCooldownTimeInMilliseconds)
        {
        }

        /// <summary>
        /// Creates a new CooldownRenew decorator with a dynamic cooldown time and custom name.
        /// </summary>
        /// <param name="name">Display name of the node</param>
        /// <param name="child">Child behavior to apply cooldown to</param>
        /// <param name="getCooldownTimeInMilliseconds">Function to get cooldown time from context</param>
        /// <exception cref="ArgumentNullException">Thrown when getCooldownTimeInMilliseconds is null</exception>
        public CooldownRenew(string name, IBehaviour<TContext> child, Func<TContext, long> getCooldownTimeInMilliseconds) : base(name, child)
        {
            _getCooldownTimeInMilliseconds = getCooldownTimeInMilliseconds ?? throw new ArgumentNullException(nameof(getCooldownTimeInMilliseconds));
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            return OnCooldown ? CooldownBehaviour(context) : RegularBehaviour(context);
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

            if (elapsedMilliseconds >= CooldownTimeInMilliseconds)
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

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            _cooldownTimeInMilliseconds = _getCooldownTimeInMilliseconds.Invoke(context);
        }
    }
}