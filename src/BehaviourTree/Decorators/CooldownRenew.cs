using System;

namespace BehaviourTree.Decorators
{
    public sealed class CooldownRenew<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        private readonly Func<TContext, long> _getCooldownTimeInMilliseconds;
        private long _cooldownTimeInMilliseconds;
        private long _cooldownStartedTimestamp;
        private bool _onCooldown;
        
        public long CooldownTimeInMilliseconds => _cooldownTimeInMilliseconds;

        public bool OnCooldown => _onCooldown;

        public CooldownRenew(IBehaviour<TContext> child, Func<TContext, long> getCooldownTimeInMilliseconds) : this("Cooldown", child, getCooldownTimeInMilliseconds)
        {
        }

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
            var currentTimeStamp = context.GetTimeStampInMilliseconds();

            var elapsedMilliseconds = currentTimeStamp - _cooldownStartedTimestamp;

            if (elapsedMilliseconds >= CooldownTimeInMilliseconds)
            {
                ExitCooldown();

                return RegularBehaviour(context);
            }

            return BehaviourStatus.Failed;
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
            _cooldownStartedTimestamp = context.GetTimeStampInMilliseconds();
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnInitialize(TContext context)
        {
            _cooldownTimeInMilliseconds = _getCooldownTimeInMilliseconds.Invoke(context);
        }
    }
}
