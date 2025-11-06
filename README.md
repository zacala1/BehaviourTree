# BehaviourTree

[![Build status](https://ci.appveyor.com/api/projects/status/ad6prnywckev6s4b?svg=true)](https://ci.appveyor.com/api/projects/status/ad6prnywckev6s4b?svg=true)


## Installation
 
```
Install-Package BehaviourTree
```

## Demo
https://www.youtube.com/watch?v=OeVo2l-O0vU

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/OeVo2l-O0vU/0.jpg)](https://www.youtube.com/watch?v=OeVo2l-O0vU)

## Features

 - Generic context
 - Extensible
 - Fluent Builder
 - Basic node types included
 - Flexible time provider (no forced IClock interface)
 - Reactive/Priority nodes for dynamic behavior
 - N-child Parallel node with flexible policies
 - Tree visualizer (Coming soon)

## Usage (FluentBuilder)

``` cs    
var behaviourTree = FluentBuilder.Create<MyContext>()
    .Sequence("root")
        .Do("walk to door", WalkToDoorFunc)
        .Selector("open door sequence")
            .Do("open door", OpenDoorFunc)
            .Sequence("locked door sequence")
                .Do("unlock door", UnlockDoorFunc)
                .Do("open door", OpenDoorFunc)
            .End()
            .Do("smash door", SmashDoorFunc)
        .End()
        .Do("walk through door", WalkThroughDoorFunc)
        .Do("close door", CloseDoorFunc)
    .End()
    .Build();
```

## Time Provider Setup (Optional)

Time-based nodes (Wait, Cooldown) can work in two ways:

**Option 1: Use Context with IClock interface (backward compatible)**
``` cs
public class MyContext : IClock
{
    public long GetTimeStampInMilliseconds() => /* your time source */;
}
```

**Option 2: Configure global TimeProvider (recommended)**
``` cs
// At application startup:
BehaviourTree.TimeProvider.GetTimestampInMilliseconds = () => myGameEngine.CurrentTimeMs;

// Now your context doesn't need to implement IClock
public class MyContext
{
    // No IClock required!
}
```

## Node Types

### Leaves

#### Action
``` cs    
builder.Do("my-action", context => BehaviourStatus.Succeeded)
```

#### Wait
``` cs    
builder.Wait("my-wait", 3000) // 3 seconds
```

#### Condition
``` cs    
builder.Condition("my-condition", context => true)
```

### Composites

#### Sequence
``` cs    
builder.Sequence("my-sequence")
    .Do("action1", context => BehaviourStatus.Succeeded)
    .Do("action2", context => BehaviourStatus.Succeeded)
    .Do("action3", context => BehaviourStatus.Succeeded)
    ...
.End()
```

#### Selector
``` cs    
builder.Selector("my-selector")
    .Do("action1", context => BehaviourStatus.Failed)
    .Do("action2", context => BehaviourStatus.Succeeded)
    .Do("action3", context => BehaviourStatus.Succeeded)
    ...
.End()
```

#### RandomSequence
``` cs    
builder.RandomSequence("my-random-sequence")
    .Do("action1", context => BehaviourStatus.Succeeded)
    .Do("action2", context => BehaviourStatus.Succeeded)
    .Do("action3", context => BehaviourStatus.Succeeded)
    ...
.End()
```

#### RandomSelector
``` cs    
builder.RandomSelector("my-random-selector")
    .Do("action1", context => BehaviourStatus.Failed)
    .Do("action2", context => BehaviourStatus.Succeeded)
    .Do("action3", context => BehaviourStatus.Succeeded)
    ...
.End()
```

#### PrioritySequence (Reactive Sequence)
``` cs
// Re-evaluates from the beginning every tick
// Allows earlier children to interrupt later running children
// Use case: Ensure prerequisites stay valid while executing later steps
builder.PrioritySequence("my-priority-sequence")
    .Condition("prerequisite-check", context => PrerequisitesMet(context))
    .Do("action1", context => BehaviourStatus.Succeeded)
    .Do("action2", context => BehaviourStatus.Succeeded)
    ...
.End()
```

#### PrioritySelector (Reactive Selector)
``` cs
// Re-evaluates from the beginning every tick
// Allows higher priority children to interrupt lower priority running children
// Use case: Higher priority tasks can interrupt lower priority ones
builder.PrioritySelector("my-priority-selector")
    .Do("high-priority", context => BehaviourStatus.Failed)
    .Do("medium-priority", context => BehaviourStatus.Running)
    .Do("low-priority", context => BehaviourStatus.Succeeded)
    ...
.End()
```

#### ActiveSequence / ActiveSelector
``` cs
// Aliases for PrioritySequence and PrioritySelector
// Same reactive behavior with more descriptive names
builder.ActiveSequence("my-active-sequence") // = PrioritySequence
builder.ActiveSelector("my-active-selector") // = PrioritySelector
```

#### SimpleParallel (2 children only)
``` cs
public enum SimpleParallelPolicy
{
    BothMustSucceed,
    OnlyOneMustSucceed
}

var policy = SimpleParallelPolicy.BothMustSucceed;

builder.SimpleParallel("my-parallel", policy)
    .Do("action1", context => BehaviourStatus.Running)
    .Do("action2", context => BehaviourStatus.Running)
.End()
```

#### Parallel (N children)
``` cs
// NEW: Supports any number of children with flexible policies

// Require all children to succeed
var parallel1 = new Parallel<MyContext>(ParallelPolicy.RequireAll,
    child1, child2, child3);

// Require at least one child to succeed
var parallel2 = new Parallel<MyContext>(ParallelPolicy.RequireOne,
    child1, child2, child3);

// Require exactly N children to succeed
var parallel3 = new Parallel<MyContext>(2,  // 2 out of 3 must succeed
    child1, child2, child3);

// Note: Use FluentBuilder extensions for this node (coming soon)
```

### Decorators

#### Cooldown
``` cs    
builder.Cooldown("my-cooldown", 4000) // 4 seconds
    .Do("action1", context => BehaviourStatus.Failed)
.End()
```

#### Failer
``` cs    
builder.AlwaysFail("my-failer")
    .Do("action1", context => BehaviourStatus.Succeeded)
.End()
```

#### Succeeder
``` cs    
builder.AlwaysSucceed("my-succeeder")
    .Do("action1", context => BehaviourStatus.Failed)
.End()
```

#### Inverter
``` cs    
builder.Invert("my-inverter")
    .Do("action1", context => BehaviourStatus.Failed)
.End()
```

#### RateLimiter (Cache)
``` cs    
builder.LimitCallRate("my-rate-limiter", 1000) // 1 second
    .Do("action1", context => BehaviourStatus.Failed)
.End()
```

#### Repeat
``` cs
builder.Repeat("my-repeater", 5)
    .Do("action1", context => BehaviourStatus.Failed)
.End()
```

#### Retry
``` cs
// Retries a failed child up to N times
builder.Retry("my-retry", 3)  // retry up to 3 times
    .Do("action1", context => BehaviourStatus.Failed)
.End()
```

#### TimeLimit
``` cs    
builder.TimeLimit("my-time-limit", 5000) // has 5 seconds to complete or will fail
    .Do("action1", context => BehaviourStatus.Running)
.End()
```

#### UntilSuccess
``` cs    
builder.UntilSuccess("my-until-success")
    .Do("action1", context => BehaviourStatus.Failed)
.End()
```

#### UntilFailed
``` cs    
builder.UntilFailed("my-until-failed")
    .Do("action1", context => BehaviourStatus.Succeeded)
.End()
```

#### Random
``` cs    
builder.Random("my-random", 0.6) // will call child 60% of the time
    .Do("action1", context => BehaviourStatus.Succeeded)
.End()
```

#### SubTree
``` cs
builder.SubTree("my-sub-tree", otherBehaviourTree)
```

## Recent Improvements

### Fixed Issues
- **PrioritySequence/Selector**: Now properly re-evaluates from the beginning every tick (reactive behavior)
- **Retry Decorator**: Now correctly resets child before retrying
- **ActionBehaviour**: Removed hardcoded debug file paths

### New Features
- **TimeProvider**: Global time provider removes IClock constraint on contexts
  - Backward compatible with existing IClock implementations
  - Flexible time source configuration
  - Easier testing with mock time
- **Parallel Node**: N-child parallel execution with flexible policies
  - `ParallelPolicy.RequireAll`: All children must succeed
  - `ParallelPolicy.RequireOne`: At least one child must succeed
  - `ParallelPolicy.RequireN`: Specify exact number of required successes
- **ActiveSelector/ActiveSequence**: Descriptive aliases for Priority nodes
- **Performance**: Magic numbers replaced with named constants

### Breaking Changes
**None** - All changes are backward compatible. Existing code will continue to work.

