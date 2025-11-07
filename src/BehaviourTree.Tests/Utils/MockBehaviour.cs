namespace BehaviourTree.Tests.Utils
{
    internal sealed class MockBehaviour : BaseBehaviour<MockContext>
    {
        public MockBehaviour() : base("MockBehaviour")
        {
        }

        public MockBehaviour(BehaviourStatus returnStatus) : base("MockBehaviour")
        {
            ReturnStatus = returnStatus;
        }

        public int InitializeCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int TerminateCallCount { get; private set; }
        public int ResetCallCount { get; set; }

        public BehaviourStatus TerminateStatus { get; private set; }
        public BehaviourStatus ResetStatus { get; private set; }
        public BehaviourStatus ReturnStatus { get; set; }

        protected override BehaviourStatus Update(MockContext context)
        {
            UpdateCallCount++;
            return ReturnStatus;
        }

        protected override void OnTerminate(BehaviourStatus status)
        {
            TerminateCallCount++;
            TerminateStatus = status;
        }

        protected override void DoReset(BehaviourStatus status)
        {
            ResetStatus = status;
            ResetCallCount++;
        }

        protected override void OnInitialize(MockContext context)
        {
            InitializeCallCount++;
        }
    }
}
