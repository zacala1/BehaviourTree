namespace BehaviourTree.Decorators
{
    public sealed class Cooldown<TContext> : DecoratorBehaviour<TContext> where TContext : IClock
    {
        public readonly long CooldownTimeInMilliseconds;
        private long _cooldownStartedTimestamp;
        private bool _onCooldown;

        public bool OnCooldown => _onCooldown;

        public Cooldown(IBehaviour<TContext> child, int cooldownTimeInMilliseconds) : this("Cooldown", child, cooldownTimeInMilliseconds)
        {
        }

        public Cooldown(string name, IBehaviour<TContext> child, int cooldownTimeInMilliseconds) : base(name, child)
        {
            CooldownTimeInMilliseconds = cooldownTimeInMilliseconds;
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
    }
}
