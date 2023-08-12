using System;
using System.Threading;
using System.Threading.Tasks;

namespace BehaviourTree.Behaviours
{
    public class AsyncAction<TContext> : BaseBehaviour<TContext>
    {
        private readonly Func<TContext, CancellationToken, Task<BehaviourStatus>> action;
        private readonly TimeSpan timeout;
        private readonly Func<TContext, bool> cancelCondition;
        private Task<BehaviourStatus> task;
        private CancellationTokenSource cts;

        public AsyncAction(Func<TContext, CancellationToken, Task<BehaviourStatus>> action,
            TimeSpan timeout = default) : this("ActionAsync", action, timeout)
        {
        }

        public AsyncAction(string name, Func<TContext, CancellationToken, Task<BehaviourStatus>> action,
            TimeSpan timeout = default) : this(name, action, null, timeout)
        {
        }

        public AsyncAction(string name,
            Func<TContext, CancellationToken, Task<BehaviourStatus>> action,
            Func<TContext, bool> cancelCondition,
            TimeSpan timeout = default) : base(name)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            this.action = action;
            this.cancelCondition = cancelCondition;
            this.timeout = timeout;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override BehaviourStatus Update(TContext context)
        {
            if (task == null)
            {
                try
                {
                    cts = new CancellationTokenSource(timeout);
                    task = action.Invoke(context, cts.Token);
                    return BehaviourStatus.Running;
                }
                catch
                {
                    return BehaviourStatus.Failed;
                }
            }

            if (task.IsCompleted)
            {
                try
                {
                    return task.Result;
                }
                catch
                {
                    return BehaviourStatus.Failed;
                }
            }

            if (cancelCondition?.Invoke(context) ?? false)
            {
                cts.Cancel();
                return BehaviourStatus.Failed;
            }

            return BehaviourStatus.Running;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void OnTerminate(BehaviourStatus status)
        {
            DoReset(status);
            base.OnTerminate(status);
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override void DoReset(BehaviourStatus status)
        {
            base.DoReset(status);
            try
            {
                cts?.Cancel();
                task?.Wait();
            }
            catch
            {
                // ignored
            }
            finally
            {
                cts?.Dispose();
                task?.Dispose();
                cts = null;
                task = null;
            }
        }
    }
}