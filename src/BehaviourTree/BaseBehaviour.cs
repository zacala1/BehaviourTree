using BehaviourTree.Events;
using System;
using System.Diagnostics;
using System.Threading;

namespace BehaviourTree
{
    public abstract class BaseBehaviour<TContext> : BaseBehaviour, IBehaviour<TContext>
    {
        protected BaseBehaviour(string name) : base(name)
        {
        }

        [System.Diagnostics.DebuggerStepThrough]
        public BehaviourStatus Tick(TContext context)
        {
            if (Status == BehaviourStatus.Ready)
            {
                OnInitialize(context);
                SendBehaviourInfoEvent(this, BehaviourTreeNodeInfoEventType.Initialize, Status);
            }
#if DEBUG
            var timer = Stopwatch.StartNew();
#endif
            Status = Update(context);
            SendBehaviourInfoEvent(this, BehaviourTreeNodeInfoEventType.Update, Status);
#if DEBUG
            if (timer.ElapsedMilliseconds >= 80)
            {
                Debug.WriteLine($"[{DateTime.Now.ToString("yyyy/MM/dd/HH:mm:ss.ffff")}] Behavior Node is hanging. id={Id}, name={Name}, status={Status}, time={timer.ElapsedMilliseconds}");
            }
            timer.Stop();
#endif
            if (Status == BehaviourStatus.Ready)
            {
                throw new InvalidOperationException("Ready status should not be returned by Behaviour Update Method");
            }

            if (Status != BehaviourStatus.Running)
            {
                OnTerminate(Status);
                SendBehaviourInfoEvent(this, BehaviourTreeNodeInfoEventType.Terminate, Status);
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
            SendBehaviourInfoEvent(this, BehaviourTreeNodeInfoEventType.Reset, Status);
            Status = BehaviourStatus.Ready;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected abstract BehaviourStatus Update(TContext context);

        [System.Diagnostics.DebuggerStepThrough]
        protected virtual void OnTerminate(BehaviourStatus status)
        { }

        [System.Diagnostics.DebuggerStepThrough]
        protected virtual void OnInitialize(TContext context)
        { }

        [System.Diagnostics.DebuggerStepThrough]
        protected virtual void DoReset(BehaviourStatus status)
        { }
    }

    [System.Diagnostics.DebuggerDisplay("Node: Id = {Id}, Name = {Name}, Status = {Status}")]
    public abstract class BaseBehaviour : IDisposable
    {
        private static long BehaviorCounter = 0;

        public static event EventHandler<BehaviourTreeEventArgs> StatusChangeEvent;

        public int Id
        {
            [System.Diagnostics.DebuggerStepThrough]
            get;
        }

        public string Name
        {
            [System.Diagnostics.DebuggerStepThrough]
            get;
        }

        public BehaviourStatus Status
        {
            [System.Diagnostics.DebuggerStepThrough]
            get;
            [System.Diagnostics.DebuggerStepThrough]
            protected set;
        }

        protected BaseBehaviour(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
            Id = (int)Interlocked.Increment(ref BehaviorCounter);
            Name = name;
            Status = BehaviourStatus.Ready;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected static void SendBehaviourInfoEvent(BaseBehaviour sender, BehaviourTreeNodeInfoEventType treeNodeInfoEventType, BehaviourStatus status)
        {
            var arg = new BehaviourTreeEventArgs(sender.Id, status, treeNodeInfoEventType);
            StatusChangeEvent?.Invoke(sender, arg);
        }

        #region IDisposable

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}