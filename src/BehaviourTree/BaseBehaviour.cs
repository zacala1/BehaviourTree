using System;
using System.Threading;

namespace BehaviourTree
{
    public abstract class BaseBehaviour<TContext> : BaseBehaviour, IBehaviour<TContext>
    {
        protected BaseBehaviour(string name) : base(name)
        {
            
        }

        public BehaviourStatus Tick(TContext context)
        {
            if (Status == BehaviourStatus.Ready)
            {
                OnInitialize();
                OnSendEvent(this, BehaviourTreeEventType.Initialize, BehaviourStatus.Ready);
            }

            Status = Update(context);
            OnSendEvent(this, BehaviourTreeEventType.Update, Status);

            if (Status == BehaviourStatus.Ready)
            {
                throw new InvalidOperationException("Ready status should not be returned by Behaviour Update Method");
            }

            if (Status != BehaviourStatus.Running)
            {
                OnTerminate(Status);
                OnSendEvent(this, BehaviourTreeEventType.Terminate, Status);
            }

            return Status;
        }

        public void Reset()
        {
            if (Status == BehaviourStatus.Ready)
            {
                return;
            }

            DoReset(Status);
            OnSendEvent(this, BehaviourTreeEventType.Reset, Status);
            Status = BehaviourStatus.Ready;
        }

        protected abstract BehaviourStatus Update(TContext context);

        protected virtual void OnTerminate(BehaviourStatus status)
        {
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void DoReset(BehaviourStatus status)
        {
        }
    }

    public abstract class BaseBehaviour
    {
        private static int counter;
        public static event EventHandler<BehaviourTreeEventArgs> StatusEvent;
        
        public int Id { get; } = Interlocked.Increment(ref counter);
        public string Name { get; }
        public BehaviourStatus Status { get; protected set; } = BehaviourStatus.Ready;

        public BaseBehaviour(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        
        protected static void OnSendEvent(BaseBehaviour sender, BehaviourTreeEventType eventType, BehaviourStatus status)
        {
            var args = new BehaviourTreeEventArgs()
            {
                Id = sender.Id,
                Name = sender.Name,
                Type = eventType,
                Status = status
            };
            StatusEvent?.Invoke(sender, args);
        }
        
        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}