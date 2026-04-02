using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BehaviourTree.Blackboard;

namespace BehaviourTree.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80)]
    public class BlackboardBenchmarks
    {
        private Blackboard.Blackboard _blackboard = null!;
        private Blackboard.Blackboard _childBoard = null!;

        [GlobalSetup]
        public void Setup()
        {
            _blackboard = new Blackboard.Blackboard();
            for (int i = 0; i < 100; i++)
                _blackboard.Set($"key{i}", i);

            _childBoard = new Blackboard.Blackboard(_blackboard);
        }

        [Benchmark(Description = "Blackboard Set (existing key)")]
        public void SetExisting()
        {
            _blackboard.Set("key50", 999);
        }

        [Benchmark(Description = "Blackboard Get<int>")]
        public int GetInt()
        {
            return _blackboard.Get<int>("key50");
        }

        [Benchmark(Description = "Blackboard TryGet<int>")]
        public bool TryGetInt()
        {
            return _blackboard.TryGet<int>("key50", out _);
        }

        [Benchmark(Description = "Child Blackboard Get (fallthrough)")]
        public int ChildGet()
        {
            return _childBoard.Get<int>("key50");
        }

        [Benchmark(Description = "Blackboard Set with observer")]
        public void SetWithObserver()
        {
            _blackboard.Set("key0", 42);
        }

        [GlobalSetup(Target = nameof(SetWithObserver))]
        public void SetupObserver()
        {
            _blackboard = new Blackboard.Blackboard();
            _blackboard.Set("key0", 0);
            _blackboard.Observe("key0", (k, o, n) => { });
        }
    }
}
