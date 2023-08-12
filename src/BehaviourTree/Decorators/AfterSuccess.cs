using System;

namespace BehaviourTree.Decorators
{
    public sealed class AfterSuccess<TContext> : DecoratorBehaviour<TContext>
    {
        private BehaviourStatus childStatus;
        private readonly Action<TContext> _action;

        public AfterSuccess(IBehaviour<TContext> child, Action<TContext> action) : this("AfterSuccess", child, action)
        {
        }

        public AfterSuccess(string name, IBehaviour<TContext> child, Action<TContext> action) : base(name, child)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            if (childStatus != BehaviourStatus.Failed &&
                childStatus != BehaviourStatus.Succeeded)
            {
                childStatus = Child.Tick(context);
            }

            if (childStatus == BehaviourStatus.Succeeded)
            {
                _action.Invoke(context);
            }

            return childStatus;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            childStatus = BehaviourStatus.Ready;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            childStatus = BehaviourStatus.Ready;
            base.DoReset(status);
        }
    }
}
