using System;

namespace BehaviourTree
{
    public abstract class BaseBehaviour<TContext> : IBehaviour<TContext>
    {
        public string Name { get; }
        public BehaviourStatus Status { get; private set; } = BehaviourStatus.Ready;

        protected BaseBehaviour(string name)
        {
            Name = name;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public BehaviourStatus Tick(TContext context)
        {
            if (Status == BehaviourStatus.Ready)
            {
                OnInitialize();
            }

            Status = Update(context);

            if (Status == BehaviourStatus.Ready)
            {
                throw new InvalidOperationException("Ready status should not be returned by Behaviour Update Method");
            }

            if (Status != BehaviourStatus.Running)
            {
                OnTerminate(Status);
            }

            return Status;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void Reset()
        {
            if (Status == BehaviourStatus.Ready)
            {
                return;
            }

            DoReset(Status);
            Status = BehaviourStatus.Ready;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected abstract BehaviourStatus Update(TContext context);

        [System.Diagnostics.DebuggerStepThrough]
        protected virtual void OnTerminate(BehaviourStatus status)
        {
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected virtual void OnInitialize()
        {
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected virtual void DoReset(BehaviourStatus status)
        {
        }

<<<<<<< Updated upstream
=======
    public abstract class BaseBehaviour
    {
        private static int counter;
        public static event EventHandler<BehaviourTreeEventArgs> StatusEvent;
        
        public int Id { [System.Diagnostics.DebuggerStepThrough] get; }
        public string Name { [System.Diagnostics.DebuggerStepThrough] get; }

        public BehaviourStatus Status
        {
            [System.Diagnostics.DebuggerStepThrough] get;
            [System.Diagnostics.DebuggerStepThrough] protected set;
        } = BehaviourStatus.Ready;

        public BaseBehaviour(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Id = Interlocked.Increment(ref counter);
        }
        
        [System.Diagnostics.DebuggerStepThrough]
        protected static void OnSendEvent(BaseBehaviour sender, BehaviourTreeEventType eventType, BehaviourStatus status)
        {
            var args = new BehaviourTreeEventArgs(sender.Id, sender.Name, eventType, status);
            StatusEvent?.Invoke(sender, args);
        }
        
        #region IDisposable
        
        private bool disposed;
>>>>>>> Stashed changes
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        #endregion
    }
}