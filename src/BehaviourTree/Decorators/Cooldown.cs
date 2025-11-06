using System;

namespace BehaviourTree.Decorators
{
    public sealed class Cooldown<TContext> : DecoratorBehaviour<TContext>
    {
        private readonly Func<TContext, long> _getCooldownTimeInMilliseconds;
        private long _cooldownTimeInMilliseconds;
        private long _cooldownStartedTimestamp;
        private bool _onCooldown;

        public long CooldownTimeInMilliseconds => _cooldownTimeInMilliseconds;
        public bool OnCooldown => _onCooldown;

        public Cooldown(IBehaviour<TContext> child, Func<TContext, long> getCooldownTimeInMilliseconds)
            : this("Cooldown", child, getCooldownTimeInMilliseconds)
        {
        }

        public Cooldown(string name, IBehaviour<TContext> child, Func<TContext, long> getCooldownTimeInMilliseconds)
            : base(name, child)
        {
            _getCooldownTimeInMilliseconds = getCooldownTimeInMilliseconds ?? throw new ArgumentNullException(nameof(getCooldownTimeInMilliseconds));
        }

        public Cooldown(IBehaviour<TContext> child, int cooldownTimeInMilliseconds)
            : this("Cooldown", child, cooldownTimeInMilliseconds)
        {
        }

        public Cooldown(string name, IBehaviour<TContext> child, int cooldownTimeInMilliseconds)
            : base(name, child)
        {
            _cooldownTimeInMilliseconds = cooldownTimeInMilliseconds;
        }

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