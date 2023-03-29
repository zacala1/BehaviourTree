using System;

namespace BehaviourTree.Decorators
{
    public sealed class CooldownRenew<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        public long CooldownTimeInMilliseconds { get; private set; }
        private readonly Func<TContext, int> _getCooldownTimeInMilliseconds;
        private long _cooldownStartedTimestamp;

        public bool OnCooldown { get; private set; }

        public CooldownRenew(IBehaviour<TContext> child, Func<TContext, int> getCooldownTimeInMilliseconds)
            : this("Cooldown", child, getCooldownTimeInMilliseconds)
        {
        }

        public CooldownRenew(string name, IBehaviour<TContext> child, Func<TContext, int> getCooldownTimeInMilliseconds)
            : base(name, child)
        {
            if (getCooldownTimeInMilliseconds == null) throw new ArgumentNullException(nameof(getCooldownTimeInMilliseconds));
            _getCooldownTimeInMilliseconds = getCooldownTimeInMilliseconds;
        }

        protected override BehaviourStatus Update(TContext context)
        {
            return OnCooldown ? CooldownBehaviour(context) : RegularBehaviour(context);
        }

        private BehaviourStatus RegularBehaviour(TContext context)
        {
            var childStatus = Child.Tick(context);

            if (childStatus == BehaviourStatus.Succeeded)
            {
                EnterCooldown(context);
            }

            return childStatus;
        }

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

        private void ExitCooldown()
        {
            OnCooldown = false;
            _cooldownStartedTimestamp = 0;
        }

        private void EnterCooldown(TContext context)
        {
            OnCooldown = true;
            _cooldownStartedTimestamp = context.GetTimeStampInMilliseconds();
            CooldownTimeInMilliseconds = _getCooldownTimeInMilliseconds?.Invoke(context) ?? 0;
        }
    }
}
