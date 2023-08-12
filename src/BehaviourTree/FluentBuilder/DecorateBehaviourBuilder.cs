using System;

namespace BehaviourTree.FluentBuilder
{
    public sealed class DecorateBehaviourBuilder<TContext> : BehaviourBuilder<TContext>
    {
        public DecorateBehaviourBuilder()
        {
        }

        public CreateDecorateBehaviour<TContext> Factory { get; set; }

        private BehaviourBuilder<TContext> _child;
        public BehaviourBuilder<TContext> Child
        {
            get { return _child; }
            set
            {
                if (_child != null) throw new ArgumentException("Must have only one child", nameof(value));
                _child = value;
            }
        }

        public override IBehaviour<TContext> Build()
        {
            var behaviours = Child?.Build();

            return Factory(behaviours);
        }
    }
}