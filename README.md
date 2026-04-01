# BehaviourTree

A flexible, extensible C# behavior tree library with generic context support, fluent builder API, blackboard system, event-driven execution, and comprehensive node types for AI and decision-making systems.

## Origin

This library is based on [Eraclys/BehaviourTree](https://github.com/Eraclys/BehaviourTree). Enhancements include:
- Blackboard system with hierarchical scoping and observer pattern
- Observer abort mechanism (Self/LowerPriority/Both) for reactive AI
- Event-driven ObservingSelector for efficient large-scale tree evaluation
- Async node improvements (sync completion, diagnostics, external cancellation)
- Tree serialization and deserialization (JSON + NodeDescriptor registry)
- Efficient debugging API for UI visualization
- Flexible time provider system (no forced IClock interface)
- Reactive/Priority nodes for dynamic behavior
- N-child Parallel node with flexible policies
- Thread-safe random number generation
- Comprehensive test coverage (850+ tests)

## Installation

```
Install-Package BehaviourTree
Install-Package BehaviourTree.Graph  # Optional: visualization support
```

## Features

- **Generic Context**: Use any context type with your behavior trees
- **Blackboard System**: Key-value data store with hierarchical scoping and change observers
- **Observer Abort**: BlackboardCondition with Self/LowerPriority/Both abort modes
- **Event-Driven Selector**: ObservingSelector only re-evaluates on observed condition changes
- **Async Nodes**: AsyncAction and AsyncCondition with sync completion optimization, diagnostics, and external cancellation
- **Tree Serialization**: JSON serializer + NodeDescriptor-based deserializer with action/condition registry
- **Fluent Builder API**: Intuitive tree construction with method chaining
- **Comprehensive Node Library**: Composites, decorators, and leaf nodes included
- **Flexible Time Provider**: No forced IClock interface, backward compatible
- **Reactive/Priority Nodes**: Dynamic behavior with re-evaluation
- **Parallel Execution**: N-child parallel node with flexible success policies
- **Debugging API**: Efficient tree structure and active node tracking for UI visualization
- **Thread-Safe**: Safe for concurrent access where needed
- **Well-Tested**: 850+ tests with extensive coverage
- **Graph Visualization**: PlantUML and Dan Abad format export (separate package)

## Quick Start

```csharp
using BehaviourTree;
using BehaviourTree.FluentBuilder;

// Define your context
public class AIContext
{
    public Vector3 Position { get; set; }
    public GameObject Target { get; set; }
}

// Build a behavior tree
var behaviourTree = FluentBuilder.Create<AIContext>()
    .Sequence("patrol-sequence")
        .Condition("has-target", ctx => ctx.Target != null)
        .Selector("combat")
            .Do("attack", ctx => AttackTarget(ctx))
            .Do("retreat", ctx => Retreat(ctx))
        .End()
    .End()
    .Build();

// Execute the tree
var context = new AIContext();
var status = behaviourTree.Tick(context);
```

## Blackboard System

The blackboard provides a shared key-value store for behavior tree nodes with change notification support.

### Basic Usage

```csharp
using BehaviourTree.Blackboard;

var blackboard = new Blackboard();
blackboard.Set("targetId", 42);
blackboard.Set("health", 100.0);

var targetId = blackboard.Get<int>("targetId");
if (blackboard.TryGet<double>("health", out var health))
{
    Console.WriteLine($"Health: {health}");
}
```

### Hierarchical Scoping (Subtree Isolation)

```csharp
var globalBoard = new Blackboard();
globalBoard.Set("difficulty", "hard");

// Child board: writes are local, reads fall through to parent
var subtreeBoard = new Blackboard(globalBoard);
subtreeBoard.Get<string>("difficulty"); // "hard" (from parent)
subtreeBoard.Set("localVar", 123);     // only in child
```

### Observer Pattern (Change Detection)

```csharp
var handle = blackboard.Observe("enemyVisible", (key, oldVal, newVal) =>
{
    Console.WriteLine($"{key} changed: {oldVal} -> {newVal}");
});

blackboard.Set("enemyVisible", true); // triggers callback

handle.Dispose(); // stop observing
```

## Observer Abort (BlackboardCondition)

Inspired by Unreal Engine's behavior tree decorators, `BlackboardCondition` monitors blackboard keys and aborts execution when conditions change.

```csharp
using BehaviourTree.Decorators;

// Self abort: stop shooting when ammo runs out
var shootBehaviour = new BlackboardCondition<MyContext>(
    child: shootAction,
    getBlackboard: ctx => ctx.Blackboard,
    key: "hasAmmo",
    condition: val => val is bool b && b,
    abortMode: AbortMode.Self);

// LowerPriority abort: interrupt patrol when enemy appears
var attackBehaviour = new BlackboardCondition<MyContext>(
    child: attackAction,
    getBlackboard: ctx => ctx.Blackboard,
    key: "enemyVisible",
    condition: val => val is bool b && b,
    abortMode: AbortMode.LowerPriority);
```

**Abort Modes:**
- `None` - Condition only checked on entry
- `Self` - Aborts own running child when condition becomes false
- `LowerPriority` - Signals parent composite to re-evaluate when condition becomes true
- `Both` - Self + LowerPriority combined

## Event-Driven Selector (ObservingSelector)

Unlike `PrioritySelector` which re-evaluates all children every tick, `ObservingSelector` stays on the current running child and only interrupts when a higher-priority `IAbortableObserver` child signals a condition change.

```csharp
using BehaviourTree.Composites;

var tree = new ObservingSelector<MyContext>(
    new BlackboardCondition<MyContext>(attackAction, getBoard, "enemyVisible",
        val => val is bool b && b, AbortMode.LowerPriority),
    patrolAction  // stays running until enemy becomes visible
);
```

**Performance benefit:** For trees with many branches, avoids O(branches) re-evaluation per tick.

## Async Nodes

### AsyncAction

```csharp
var asyncNode = new AsyncAction<MyContext>("load-data",
    async (ctx, token) =>
    {
        var data = await LoadDataAsync(token);
        return BehaviourStatus.Succeeded;
    },
    cancelCondition: ctx => ctx.ShouldAbort,
    timeout: TimeSpan.FromSeconds(5),
    externalToken: appShutdownToken);

// After tick:
if (asyncNode.WasCancelled) { /* timeout or cancel condition triggered */ }
if (asyncNode.LastException != null) { /* async task faulted */ }
```

**Improvements over traditional async:**
- Synchronously completed tasks return result on the same tick (no wasted frame)
- `WasCancelled` property distinguishes cancellation from normal failure
- `LastException` exposes fault information for debugging
- External `CancellationToken` support for application shutdown
- Non-blocking cleanup (no `task.Wait()`)

### AsyncCondition

```csharp
var asyncCondition = new AsyncCondition<MyContext>("check-server",
    async (ctx, token) =>
    {
        var result = await ctx.Server.PingAsync(token);
        return result.IsAlive;
    },
    timeout: TimeSpan.FromSeconds(2));
// Returns Succeeded if true, Failed if false, Running while awaiting
```

## Tree Serialization

### Serialize to JSON

```csharp
using BehaviourTree.Serialization;

string json = BehaviourTreeSerializer.ToJson(tree);
```

### Deserialize from NodeDescriptor

```csharp
var deserializer = new BehaviourTreeDeserializer<MyContext>()
    .RegisterAction("attack", ctx => Attack(ctx))
    .RegisterAction("patrol", ctx => Patrol(ctx))
    .RegisterCondition("hasTarget", ctx => ctx.Target != null);

var tree = deserializer.Build(new NodeDescriptor
{
    Type = "Selector", Category = "composite", Name = "Root",
    Children = new[]
    {
        new NodeDescriptor
        {
            Type = "Sequence", Category = "composite",
            Children = new[]
            {
                new NodeDescriptor { Type = "Condition", Category = "leaf", ConditionRef = "hasTarget" },
                new NodeDescriptor { Type = "Action", Category = "leaf", ActionRef = "attack" }
            }
        },
        new NodeDescriptor { Type = "Action", Category = "leaf", ActionRef = "patrol" }
    }
});
```

## Builder Patterns

The fluent builder supports two syntax styles:

### Traditional Chaining (Manual End)

```csharp
var tree = FluentBuilder.Create<AIContext>()
    .Sequence("patrol-sequence")
        .Condition("has-target", ctx => ctx.Target != null)
        .Selector("combat")
            .Do("attack", ctx => AttackTarget(ctx))
            .Do("retreat", ctx => Retreat(ctx))
        .End()
    .End()
    .Build();
```

### Lambda-Based (Auto-Indentation)

```csharp
var tree = FluentBuilder.Create<AIContext>()
    .Sequence("patrol-sequence", seq => {
        seq.Condition("has-target", ctx => ctx.Target != null);
        seq.Selector("combat", sel => {
            sel.Do("attack", ctx => AttackTarget(ctx));
            sel.Do("retreat", ctx => Retreat(ctx));
        });
    })
    .Build();
```

## Core Concepts

### Behavior Status

Every node returns one of four statuses:

- **Ready**: Node hasn't been ticked yet
- **Running**: Node is executing and needs more ticks to complete
- **Succeeded**: Node completed successfully
- **Failed**: Node completed unsuccessfully

### Node Lifecycle

1. **Initialize**: Called on first tick (when status is Ready)
2. **Update**: Called every tick, returns the current status
3. **Terminate**: Called when node completes (Success or Failure)
4. **Reset**: Resets node back to Ready status

### Debugging API

```csharp
using BehaviourTree.Debugging;

// Build tree structure once at initialization
var structure = BehaviourTreeStructure.Build(behaviourTree);

// Each tick: Get only active leaf node IDs
var activeLeafIds = behaviourTree.GetActiveLeafIds();

// Derive full active path from structure
var allActiveNodeIds = structure.GetAllActiveNodeIds(activeLeafIds);
```

## Time Provider Setup (Optional)

```csharp
// Option 1: Context with IClock
public class MyContext : IClock
{
    public long GetTimeStampInMilliseconds() => DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
}

// Option 2: Global TimeProvider (recommended)
BehaviourTree.TimeProvider.GetTimestampInMilliseconds = () => myGameEngine.CurrentTimeMs;
```

## Node Types Reference

### Leaf Nodes

| Node | Description |
|------|-------------|
| `Action` | Executes a function, returns its status |
| `Condition` | Evaluates boolean predicate → Success/Failed |
| `Wait` | Waits N ms (fixed or dynamic via `Func<TContext, long>`) |
| `AsyncAction` | Async task with timeout, cancel condition, external token |
| `AsyncCondition` | Async predicate → Success if true, Failed if false |

### Composite Nodes

| Node | Description |
|------|-------------|
| `Sequence` | Execute in order, fail on first failure |
| `Selector` | Execute in order, succeed on first success |
| `PrioritySequence` | Reactive sequence, re-evaluates from start each tick |
| `PrioritySelector` | Reactive selector, re-evaluates from start each tick |
| `ObservingSelector` | Event-driven selector, re-evaluates only on abort signal |
| `Parallel` | Execute all children, configurable success policy |
| `SimpleParallel` | Optimized 2-child parallel |
| `RandomSequence` | Shuffled order sequence |
| `RandomSelector` | Shuffled order selector |

### Decorator Nodes

| Node | Description |
|------|-------------|
| `Inverter` | Flips Success ↔ Failed |
| `Succeeder` | Always returns Success |
| `Failer` | Always returns Failed |
| `Retry` | Retries on failure up to N times |
| `Repeater` | Repeats N times on success |
| `UntilSuccess` | Repeats until child succeeds |
| `UntilFailed` | Repeats until child fails |
| `Guard` | Checks condition every tick, aborts child if false |
| `BlackboardCondition` | Monitors blackboard key with abort modes |
| `Cooldown` | Enforces cooldown period after success |
| `TimeLimiter` | Fails if child exceeds time limit |
| `RateLimiter` | Caches child result for interval |
| `Random` | Probabilistic execution (threshold 0.0-1.0) |
| `AutoReset` | Auto-resets child after completion |
| `AfterSuccess` | Callback after child succeeds |
| `AfterFailed` | Callback after child fails |
| `SubTree` | References another tree for modular composition |

## Testing

```bash
dotnet test src/BehaviourTree.sln
```

850+ tests covering all node types, blackboard, serialization, observer abort, async nodes, and edge cases.

## License

See LICENSE file for details.

## Credits

Based on [Eraclys/BehaviourTree](https://github.com/Eraclys/BehaviourTree).
