using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using BehaviourTree.FluentBuilder;

namespace BehaviourTree.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net80)]
    public class TreeTickBenchmarks
    {
        private sealed class BenchContext : IClock
        {
            public long Time { get; set; }
            public long GetTimeStampInMilliseconds() => Time;
        }

        private IBehaviour<BenchContext> _flatSelector = null!;
        private IBehaviour<BenchContext> _deepSequence = null!;
        private IBehaviour<BenchContext> _complexTree = null!;
        private IBehaviour<BenchContext> _parallelTree = null!;
        private BenchContext _context = null!;

        [GlobalSetup]
        public void Setup()
        {
            _context = new BenchContext { Time = 0 };

            // 10-child flat selector (first always fails, last succeeds)
            var selectorChildren = new IBehaviour<BenchContext>[10];
            for (int i = 0; i < 9; i++)
                selectorChildren[i] = new Condition<BenchContext>(_ => false);
            selectorChildren[9] = new Condition<BenchContext>(_ => true);
            _flatSelector = new Selector<BenchContext>("FlatSelector", selectorChildren);

            // 10-level deep sequence (all succeed)
            _deepSequence = BuildDeepSequence(10);

            // Complex tree: PrioritySelector with 5 branches, each Sequence of 3
            _complexTree = BuildComplexTree();

            // Parallel with 5 children
            var parallelChildren = new IBehaviour<BenchContext>[5];
            for (int i = 0; i < 5; i++)
                parallelChildren[i] = new ActionBehaviour<BenchContext>($"action{i}", _ => BehaviourStatus.Succeeded);
            _parallelTree = new Parallel<BenchContext>(ParallelPolicy.RequireAll, parallelChildren);
        }

        [Benchmark(Description = "Flat Selector (10 children)")]
        public BehaviourStatus FlatSelector()
        {
            _flatSelector.Reset();
            return _flatSelector.Tick(_context);
        }

        [Benchmark(Description = "Deep Sequence (depth=10)")]
        public BehaviourStatus DeepSequence()
        {
            _deepSequence.Reset();
            return _deepSequence.Tick(_context);
        }

        [Benchmark(Description = "Complex PrioritySelector (5x3)")]
        public BehaviourStatus ComplexTree()
        {
            _complexTree.Reset();
            return _complexTree.Tick(_context);
        }

        [Benchmark(Description = "Parallel (5 children, RequireAll)")]
        public BehaviourStatus ParallelAll()
        {
            _parallelTree.Reset();
            return _parallelTree.Tick(_context);
        }

        private static IBehaviour<BenchContext> BuildDeepSequence(int depth)
        {
            if (depth == 0)
                return new ActionBehaviour<BenchContext>("leaf", _ => BehaviourStatus.Succeeded);

            return new Sequence<BenchContext>("seq",
                new ActionBehaviour<BenchContext>("action", _ => BehaviourStatus.Succeeded),
                BuildDeepSequence(depth - 1));
        }

        private static IBehaviour<BenchContext> BuildComplexTree()
        {
            var branches = new IBehaviour<BenchContext>[5];
            for (int i = 0; i < 5; i++)
            {
                var isLast = i == 4;
                branches[i] = new Sequence<BenchContext>($"branch{i}",
                    new Condition<BenchContext>($"cond{i}", _ => isLast),
                    new ActionBehaviour<BenchContext>($"check{i}", _ => BehaviourStatus.Succeeded),
                    new ActionBehaviour<BenchContext>($"act{i}", _ => BehaviourStatus.Succeeded));
            }
            return new PrioritySelector<BenchContext>("Root", branches);
        }
    }
}
