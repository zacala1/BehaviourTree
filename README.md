# BehaviourTree

A flexible, extensible C# behavior tree library with generic context support, fluent builder API, and comprehensive node types for AI and decision-making systems.

## Origin

This library is based on [Eraclys/BehaviourTree](https://github.com/Eraclys/BehaviourTree). Enhancements include:
- Efficient debugging API for UI visualization
- Flexible time provider system (no forced IClock interface)
- Reactive/Priority nodes for dynamic behavior
- N-child Parallel node with flexible policies
- Thread-safe random number generation
- Enhanced error handling and validation
- Comprehensive test coverage
- Complete English documentation

## Installation

```
Install-Package BehaviourTree
Install-Package BehaviourTree.Graph  # Optional: visualization support
```

## Features

- **Generic Context**: Use any context type with your behavior trees
- **Extensible**: Easy to create custom node types
- **Fluent Builder API**: Intuitive tree construction with method chaining
- **Comprehensive Node Library**: Composites, decorators, and leaf nodes included
- **Flexible Time Provider**: No forced IClock interface, backward compatible
- **Reactive/Priority Nodes**: Dynamic behavior with re-evaluation
- **Parallel Execution**: N-child parallel node with flexible success policies
- **Debugging API**: Efficient tree structure and active node tracking for UI visualization
- **Thread-Safe**: Safe for concurrent access where needed
- **Well-Tested**: Extensive test coverage
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

The lambda-based syntax provides automatic indentation support in IDEs and eliminates manual `End()` calls:

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

**Lambda-based benefits**:
- IDE automatically indents nested blocks
- No need to manually call `End()`
- Clearer visual hierarchy
- Less error-prone (can't forget `End()`)

**Supported nodes**: `Sequence`, `Selector`, `ActiveSequence`, `ActiveSelector`, `Parallel`, `Retry`, `Repeat`, `Invert`, `TimeLimit`

Both styles can be mixed:

```csharp
.Sequence("root", seq => {
    seq.Condition("check", ctx => true);

    // Traditional syntax within lambda
    seq.Selector("traditional")
        .Do("action1", ctx => Status.Failed)
        .Do("action2", ctx => Status.Succeeded)
    .End();
})
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

Efficiently track active nodes for UI visualization with minimal overhead:

```csharp
using BehaviourTree.Debugging;

// 1. Build tree structure once at initialization (O(n) where n = total nodes)
var structure = BehaviourTreeStructureBuilder.Build(behaviourTree);

// 2. Each tick: Get only active leaf node IDs (O(running leaves) - typically 1-5)
var activeLeafIds = behaviourTree.GetActiveLeafIds();

// 3. Derive full active path from structure (no per-tick tree traversal needed)
var allActiveNodeIds = structure.GetAllActiveNodeIds(activeLeafIds);

// Access node info for UI display
foreach (var nodeId in allActiveNodeIds)
{
    var nodeInfo = structure.Nodes[nodeId];
    Console.WriteLine($"{nodeInfo.TypeName}: {nodeInfo.Name} (depth={nodeInfo.Depth})");
}
```

**Why this design?**
- Traditional observer patterns fire O(executed nodes) events per tick
- This API sends only O(running leaves) data - parent path is derived from pre-built structure
- Structure is built once; per-tick overhead is minimal

## Time Provider Setup (Optional)

Time-based nodes (Wait, Cooldown, TimeLimit, RateLimiter) can work in two ways:

### Option 1: Context with IClock (backward compatible)
```csharp
public class MyContext : IClock
{
    public long GetTimeStampInMilliseconds() => DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
}
```

### Option 2: Global TimeProvider (recommended)
```csharp
// At application startup:
BehaviourTree.TimeProvider.GetTimestampInMilliseconds = () => myGameEngine.CurrentTimeMs;

// Now your context doesn't need IClock
public class MyContext
{
    // No IClock required!
}
```

## Node Types Reference

### Leaf Nodes

#### Action
Executes an action function and returns its status.

```csharp
builder.Do("attack-enemy", context =>
{
    if (AttackEnemy(context.Target))
        return BehaviourStatus.Succeeded;
    return BehaviourStatus.Failed;
})
```

#### Condition
Evaluates a boolean condition, returns Success if true, Failed if false.

```csharp
builder.Condition("has-ammo", ctx => ctx.AmmoCount > 0)
```

#### Wait
Waits for specified milliseconds, returns Running while waiting, then Success.

```csharp
builder.Wait("wait-3-seconds", 3000)
```

#### AsyncAction
Executes an async task, returns Running while task executes.

```csharp
var asyncNode = new AsyncAction<MyContext>("load-data", async (ctx, token) =>
{
    await LoadDataAsync(token);
    return BehaviourStatus.Succeeded;
});
```

### Composite Nodes

Composite nodes have multiple children and execute them according to specific rules.

#### Sequence
Executes children in order. Succeeds if all succeed, fails on first failure.

```csharp
builder.Sequence("open-door-sequence")
    .Do("walk-to-door", ctx => WalkToDoor(ctx))
    .Do("unlock-door", ctx => UnlockDoor(ctx))
    .Do("open-door", ctx => OpenDoor(ctx))
.End()
```

**Use case**: Execute steps in order, all must succeed.

#### Selector
Executes children in order. Succeeds on first success, fails if all fail.

```csharp
builder.Selector("find-weapon")
    .Do("use-equipped-weapon", ctx => UseEquippedWeapon(ctx))
    .Do("find-nearby-weapon", ctx => FindNearbyWeapon(ctx))
    .Do("craft-weapon", ctx => CraftWeapon(ctx))
.End()
```

**Use case**: Try alternatives until one succeeds.

#### PrioritySequence (ActiveSequence)
Like Sequence but re-evaluates from beginning every tick. Earlier children can interrupt later running children if their status changes.

```csharp
builder.ActiveSequence("patrol-with-safety")
    .Condition("safe-to-patrol", ctx => !ctx.UnderAttack)  // Re-checked every tick
    .Do("patrol", ctx => Patrol(ctx))                       // Interrupted if becomes unsafe
.End()
```

**Use case**: Ensure prerequisites stay valid while executing actions.

#### PrioritySelector (ActiveSelector)
Like Selector but re-evaluates from beginning every tick. Higher priority children can interrupt lower priority running children.

```csharp
builder.ActiveSelector("dynamic-behavior")
    .Condition("emergency", ctx => ctx.Health < 10)    // Highest priority
    .Do("flee", ctx => Flee(ctx))                      // High priority
    .Do("normal-behavior", ctx => NormalBehavior(ctx)) // Low priority - interrupted if higher priority becomes valid
.End()
```

**Use case**: Dynamic priority switching, higher priority tasks interrupt lower ones.

#### Parallel
Executes all children simultaneously. Success policy determines when parallel node succeeds/fails.

```csharp
// Require ALL children to succeed
builder.Parallel("move-and-shoot", ParallelPolicy.RequireAll)
    .Do("move-to-target", ctx => MoveToTarget(ctx))
    .Do("shoot-at-target", ctx => ShootAtTarget(ctx))
.End()

// Require at least ONE child to succeed
builder.Parallel("try-all", ParallelPolicy.RequireOne)
    .Do("option1", ctx => TryOption1(ctx))
    .Do("option2", ctx => TryOption2(ctx))
    .Do("option3", ctx => TryOption3(ctx))
.End()

// Require N children to succeed (2 out of 3)
var parallel = new Parallel<MyContext>("majority", 2, child1, child2, child3);
```

**Policies**:
- `ParallelPolicy.RequireAll`: All children must succeed
- `ParallelPolicy.RequireOne`: At least one child must succeed
- Custom N: Specify exact number required (e.g., 2 out of 5)

#### RandomSequence / RandomSelector
Shuffles child order before executing (Sequence or Selector logic).

```csharp
builder.RandomSelector("try-random-door")
    .Do("try-door-1", ctx => TryDoor1(ctx))
    .Do("try-door-2", ctx => TryDoor2(ctx))
    .Do("try-door-3", ctx => TryDoor3(ctx))
.End()
```

#### SimpleParallel
Optimized two-child parallel with simple policies. For more than 2 children, use Parallel.

### Decorator Nodes

Decorators modify the behavior of a single child node.

#### Inverter
Inverts child result: Success ↔ Failed, Running unchanged.

```csharp
builder.Invert("not-has-target")
    .Condition("has-target", ctx => ctx.Target != null)
.End()
// Returns Failed when has target, Success when no target
```

#### AlwaysSucceed / AlwaysFail
Forces child to always return Success/Failed regardless of actual result.

```csharp
builder.AlwaysSucceed("try-optional-action")
    .Do("optional", ctx => OptionalAction(ctx))  // Won't fail sequence even if it fails
.End()
```

#### Retry
Retries failed child up to N times before giving up.

```csharp
builder.Retry("retry-connection", 3)  // Try up to 3 times
    .Do("connect", ctx => ConnectToServer(ctx))
.End()
```

#### Repeat
Repeats child N times, succeeds when all repetitions succeed.

```csharp
builder.Repeat("attack-3-times", 3)
    .Do("attack", ctx => Attack(ctx))
.End()
```

#### UntilSuccess / UntilFailed
Repeats child until it succeeds/fails (infinite retry).

```csharp
builder.UntilSuccess("keep-trying")
    .Do("difficult-action", ctx => TryDifficultAction(ctx))
.End()
```

#### Cooldown
Succeeds immediately, then returns Failed for cooldown duration.

```csharp
builder.Cooldown("ability-cooldown", 5000)  // 5 second cooldown
    .Do("use-ability", ctx => UseAbility(ctx))
.End()
```

#### TimeLimit
Fails child if it doesn't complete within time limit.

```csharp
builder.TimeLimit("quick-action", 2000)  // Must complete in 2 seconds
    .Do("action", ctx => DoAction(ctx))
.End()
```

#### RateLimiter (Cache)
Caches child result for specified duration (avoids re-execution).

```csharp
builder.LimitCallRate("expensive-check", 1000)  // Cache for 1 second
    .Condition("expensive-check", ctx => ExpensiveCheck(ctx))
.End()
```

#### Random
Executes child with specified probability (0.0 to 1.0).

```csharp
builder.Random("random-action", 0.3)  // 30% chance
    .Do("rare-action", ctx => RareAction(ctx))
.End()
```

#### AutoReset
Automatically resets child when it completes.

#### AfterSuccess / AfterFailed
Executes callback after child succeeds/fails.

```csharp
var afterSuccess = new AfterSuccess<MyContext>("log-success", childNode,
    ctx => Console.WriteLine("Action succeeded!"));
```

#### SubTree
Embeds another behavior tree as a child node.

```csharp
var subTree = BuildSubTree();
builder.Subtree("sub-behavior", subTree)
```

## Graph Visualization

The `BehaviourTree.Graph` package provides tree visualization in multiple formats.

### PlantUML Format

```csharp
using BehaviourTree.Graph;

var tree = FluentBuilder.Create<MyContext>()
    .Selector("Root")
        .Condition("HasTarget", ctx => ctx.Target != null)
        .Do("Attack", ctx => Attack(ctx))
    .End()
    .Build();

// Generate PlantUML mindmap
string plantuml = BehaviourTreeGraph.Format(tree, BehaviourTreeGraph.FormatOptions.PlantUML);
// Output:
// @startmindmap
// * **[?]** Root '1'
// ** **(?)** HasTarget '2'
// ** **(!)** Attack '3'
// @endmindmap
```

### Dan Abad Format

A compact text-based tree notation:

```csharp
string danAbad = BehaviourTreeGraph.Format(tree, BehaviourTreeGraph.FormatOptions.DanAbad);
// Output:
// ? Root
// |    (HasTarget)
// |    [Attack]
```

### Format Symbols

| Symbol | PlantUML | Dan Abad | Description |
|--------|----------|----------|-------------|
| Selector | `[?]` | `?` | Try children until one succeeds |
| Sequence | `[->]` | `->` | Execute children in order |
| Parallel | `[=N/M]` | `=N` | Execute N of M children |
| Condition | `(?)` | `(name)` | Boolean check |
| Action | `(!)` | `[name]` | Execute action |
| Inverter | `<!>` | `// Invert` | Invert result |

## Advanced Examples

### AI Enemy Behavior

```csharp
var enemyAI = FluentBuilder.Create<EnemyContext>()
    .ActiveSelector("main-behavior")
        // Emergency: Health critical
        .Sequence("flee-if-critical")
            .Condition("health-critical", ctx => ctx.Health < 20)
            .Do("flee", ctx => Flee(ctx))
        .End()

        // High Priority: Combat
        .Sequence("combat-sequence")
            .Condition("has-target", ctx => ctx.Target != null)
            .Condition("in-range", ctx => InRange(ctx.Target))
            .Selector("combat-actions")
                .Do("attack", ctx => Attack(ctx))
                .Do("use-ability", ctx => UseAbility(ctx))
            .End()
        .End()

        // Medium Priority: Patrol
        .Sequence("patrol")
            .Condition("no-target", ctx => ctx.Target == null)
            .RandomSelector("patrol-points")
                .Do("patrol-1", ctx => PatrolPoint1(ctx))
                .Do("patrol-2", ctx => PatrolPoint2(ctx))
                .Do("patrol-3", ctx => PatrolPoint3(ctx))
            .End()
        .End()

        // Low Priority: Idle
        .Do("idle", ctx => Idle(ctx))
    .End()
    .Build();
```

### Robust Network Operation

```csharp
var networkOperation = FluentBuilder.Create<NetworkContext>()
    .Sequence("robust-network-call")
        .Retry("retry-connection", 3)
            .TimeLimit("connection-timeout", 5000)
                .Do("connect", ctx => ConnectToServer(ctx))
            .End()
        .End()

        .Retry("retry-fetch", 3)
            .TimeLimit("fetch-timeout", 10000)
                .Do("fetch-data", ctx => FetchData(ctx))
            .End()
        .End()

        .Do("process-data", ctx => ProcessData(ctx))
    .End()
    .Build();
```

## API Reference

### BaseBehaviour Methods

- `BehaviourStatus Tick(TContext context)`: Execute one iteration
- `void Reset()`: Reset to Ready status

### Debugging API

- `BehaviourTreeStructureBuilder.Build(root)`: Build tree structure for visualization
- `root.GetActiveLeafIds()`: Get IDs of currently running leaf nodes
- `root.GetRunningNodes()`: Get snapshots of all running nodes in path
- `structure.GetAllActiveNodeIds(leafIds)`: Derive full active path from leaf IDs
- `structure.GetPathToNode(nodeId)`: Get path from root to specific node

### FluentBuilder Extensions

All composite and decorator nodes have fluent builder extensions:
- Composites: `Sequence()`, `Selector()`, `ActiveSequence()`, `ActiveSelector()`, `Parallel()`, `RandomSequence()`, `RandomSelector()`, `SimpleParallel()`
- Decorators: `Retry()`, `Repeat()`, `Invert()`, `Cooldown()`, `TimeLimit()`, `RateLimiter()`, `Random()`, `AlwaysSucceed()`, `AlwaysFail()`, `UntilSuccess()`, `UntilFailed()`, `AutoReset()`
- Leaves: `Do()`, `Condition()`, `Wait()`, `Subtree()`

## Recent Improvements

### Fixed Bugs
- **RateLimiter**: Fixed potential issues with nullable timestamp arithmetic
- **RandomProvider**: Made thread-safe with lock synchronization
- **BehaviourTreeRunner**: Fixed CancellationTokenSource resource leak
- **FluentBuilder**: Added validation for leaf nodes without parent
- **Condition/ActionBehaviour**: Added null validation for delegates
- **Wait**: Added validation for negative time values
- **Random**: Corrected error message (was 0-100, now 0.0-1.0)

### New Features
- **Lambda-Based Builder Pattern**: Automatic IDE indentation and no manual `End()` calls
- **Debugging API**: Efficient tree structure and active node tracking for UI visualization
- **TimeProvider**: Flexible global time source, no IClock constraint required
- **ActiveSelector/ActiveSequence**: Reactive nodes for dynamic priority switching
- **Parallel Node**: N-child parallel with flexible policies (RequireAll, RequireOne, RequireN)
- **Comprehensive Tests**: Added 370+ tests for all node types and edge cases

### Performance Improvements
- **Cache-Aligned State**: Hot fields in 64-byte aligned struct for CPU cache optimization
- **Type Name Caching**: Cached GetType().Name in constructors to avoid reflection overhead
- **Debugging API Efficiency**: O(running leaves) per tick instead of O(executed nodes)
- **Composite Node Optimization**: Direct array access with cached length
- **Lock-Free Operations**: Interlocked operations for ID generation and token management
- **Thread-Safe Random**: Lock synchronization for System.Random
- **IClock Check Caching**: Static type check at class load time instead of per-tick runtime check
- **In-Place Shuffle**: RandomSequence/RandomSelector use index array shuffling (no allocation on reset)
- **Sentinel Values**: Time-based nodes use sentinel values instead of nullable long (no boxing)

### Documentation
- All comments translated to English
- Comprehensive XML documentation for all public APIs
- Enhanced README with examples and use cases
- Inline code comments for complex logic

## Testing

The library includes comprehensive test coverage:

```bash
dotnet test BehaviourTree.sln
```

Tests cover:
- All composite nodes (Sequence, Selector, Parallel, etc.)
- All decorator nodes (Retry, Repeat, Cooldown, etc.)
- All leaf nodes (Action, Condition, Wait, AsyncAction)
- Debugging API (structure building, active node tracking)
- Edge cases (empty trees, null values, timeouts)
- Thread safety scenarios

## Contributing

Contributions are welcome! Please ensure:
- All tests pass
- New features include tests
- Code follows existing style
- XML documentation for public APIs

## License

See LICENSE file for details.

## Credits

Based on [Eraclys/BehaviourTree](https://github.com/Eraclys/BehaviourTree).
