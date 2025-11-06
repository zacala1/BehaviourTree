# BehaviourTree

A flexible, extensible C# behavior tree library with generic context support, fluent builder API, and comprehensive node types for AI and decision-making systems.

## Origin

This library is based on the original work from [milanm/BehaviourTree](https://github.com/milanm/BehaviourTree). Significant improvements and enhancements have been made including:
- Instance-based observer pattern for memory-leak-free event monitoring
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
```

## Features

- **Generic Context**: Use any context type with your behavior trees
- **Extensible**: Easy to create custom node types
- **Fluent Builder API**: Intuitive tree construction with method chaining
- **Comprehensive Node Library**: Composites, decorators, and leaf nodes included
- **Flexible Time Provider**: No forced IClock interface, backward compatible
- **Reactive/Priority Nodes**: Dynamic behavior with re-evaluation
- **Parallel Execution**: N-child parallel node with flexible success policies
- **Observer Pattern**: Memory-leak-free event monitoring system
- **Thread-Safe**: Safe for concurrent access where needed
- **Well-Tested**: Extensive test coverage

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

### Observer Pattern

Attach observers to monitor node execution without memory leaks:

```csharp
public class MyObserver : IBehaviourTreeObserver
{
    public void OnNodeInitialize(BehaviourTreeNodeEvent nodeEvent) { }
    public void OnNodeUpdate(BehaviourTreeNodeEvent nodeEvent) { }
    public void OnNodeTerminate(BehaviourTreeNodeEvent nodeEvent) { }
    public void OnNodeReset(BehaviourTreeNodeEvent nodeEvent) { }
}

var observer = new MyObserver();
behaviourTree.AttachObserver(observer);
// Later...
behaviourTree.DetachObserver(observer);
```

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
Two-child parallel with simple policies (legacy, use Parallel for new code).

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
- `void AttachObserver(IBehaviourTreeObserver observer)`: Attach observer
- `void DetachObserver(IBehaviourTreeObserver observer)`: Detach observer
- `void Dispose()`: Clean up resources

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
- **Observer Pattern**: Instance-based event system replaces static events (no memory leaks)
- **Enhanced Events**: `BehaviourTreeNodeEvent` includes elapsed time, node type, depth, parent ID
- **TimeProvider**: Flexible global time source, no IClock constraint required
- **ActiveSelector/ActiveSequence**: Reactive nodes for dynamic priority switching
- **Parallel Node**: N-child parallel with flexible policies (RequireAll, RequireOne, RequireN)
- **Comprehensive Tests**: Added 200+ tests for all node types and edge cases

### Performance Improvements
- Thread-safe random number generation
- Optimized nullable arithmetic in time-based decorators
- Pattern matching in graph visualization (30-50% faster)
- Reduced allocations in observer notifications

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
- Observer pattern functionality
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

Based on original work by [milanm/BehaviourTree](https://github.com/milanm/BehaviourTree).
Enhanced and maintained with significant improvements to functionality, performance, and reliability.
