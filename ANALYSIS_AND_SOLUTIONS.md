# Behavior Tree - 이벤트 시스템 및 그래프 시스템 분석 및 솔루션

## 📊 **이벤트 시스템 문제점 및 솔루션**

### ❌ **문제점 1: 정적 이벤트로 인한 메모리 누수**

**현재 구현:**
```csharp
// BaseBehaviour.cs
public static event EventHandler<BehaviourTreeEventArgs> StatusChangeEvent;
```

**문제:**
- static 이벤트는 구독 해제하지 않으면 객체가 GC되지 않음 (메모리 누수)
- 여러 트리가 같은 이벤트를 공유 → 트리별 격리 불가능
- 구독자가 트리보다 오래 살아있으면 weak reference 필요

**심각도:** 🔴 Critical

---

### ❌ **문제점 2: 이벤트 정보 부족**

**현재 구현:**
```csharp
public class BehaviourTreeEventArgs : EventArgs
{
    public int Id { get; }
    public BehaviourStatus Status { get; }
    public BehaviourTreeNodeInfoEventType EventType { get; }
}
```

**문제:**
- 노드 이름이 없어서 디버깅이 어려움
- 부모/자식 관계 정보 없음
- 실행 시간 정보 없음 (성능 모니터링 불가)
- Context 정보 전달 불가

**심각도:** 🟡 Medium

---

### ❌ **문제점 3: 성능 문제**

**문제:**
- 모든 노드가 매 틱마다 3번 이벤트 발생 (Initialize, Update, Terminate)
- 100개 노드 × 60 FPS = 초당 18,000 이벤트
- 이벤트 비활성화 옵션 없음
- 디버그 모드에서도 항상 발생

**심각도:** 🟡 Medium

---

### ❌ **문제점 4: 스레드 안전성**

**문제:**
- `StatusChangeEvent?.Invoke()` 는 thread-safe하지 않음
- 여러 스레드에서 트리 실행 시 race condition 가능
- 이벤트 핸들러 추가/제거 시 동기화 없음

**심각도:** 🟡 Medium (멀티스레드 사용 시 🔴 Critical)

---

## ✅ **솔루션: 개선된 이벤트 시스템**

### **Solution 1: 인스턴스 기반 이벤트 시스템**

```csharp
// 새로운 IBehaviourTreeObserver 인터페이스
public interface IBehaviourTreeObserver
{
    void OnNodeInitialize(BehaviourTreeNodeEvent nodeEvent);
    void OnNodeUpdate(BehaviourTreeNodeEvent nodeEvent);
    void OnNodeTerminate(BehaviourTreeNodeEvent nodeEvent);
    void OnNodeReset(BehaviourTreeNodeEvent nodeEvent);
}

// 향상된 이벤트 정보
public class BehaviourTreeNodeEvent
{
    public int NodeId { get; }
    public string NodeName { get; }
    public string NodeType { get; }
    public BehaviourStatus Status { get; }
    public BehaviourTreeNodeInfoEventType EventType { get; }
    public long ElapsedMilliseconds { get; }  // 실행 시간
    public int? ParentId { get; }  // 부모 노드 ID
    public int Depth { get; }  // 트리 깊이
}

// IBehaviour에 추가
public interface IBehaviour<in TContext>
{
    // 기존 멤버들...

    // 옵서버 등록/해제
    void AttachObserver(IBehaviourTreeObserver observer);
    void DetachObserver(IBehaviourTreeObserver observer);
}

// BaseBehaviour 수정
public abstract class BaseBehaviour<TContext> : BaseBehaviour, IBehaviour<TContext>
{
    private readonly List<IBehaviourTreeObserver> _observers = new List<IBehaviourTreeObserver>();
    private readonly object _observerLock = new object();

    public void AttachObserver(IBehaviourTreeObserver observer)
    {
        lock (_observerLock)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }
    }

    public void DetachObserver(IBehaviourTreeObserver observer)
    {
        lock (_observerLock)
        {
            _observers.Remove(observer);
        }
    }

    protected void NotifyObservers(BehaviourTreeNodeInfoEventType eventType, BehaviourStatus status, long elapsedMs)
    {
        // 관찰자가 없으면 즉시 반환 (성능 최적화)
        if (_observers.Count == 0) return;

        var nodeEvent = new BehaviourTreeNodeEvent(
            NodeId: Id,
            NodeName: Name,
            NodeType: GetType().Name,
            Status: status,
            EventType: eventType,
            ElapsedMilliseconds: elapsedMs,
            ParentId: null,  // CompositeBehaviour에서 설정
            Depth: 0  // 트리 빌드 시 설정
        );

        lock (_observerLock)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    switch (eventType)
                    {
                        case BehaviourTreeNodeInfoEventType.Initialize:
                            observer.OnNodeInitialize(nodeEvent);
                            break;
                        case BehaviourTreeNodeInfoEventType.Update:
                            observer.OnNodeUpdate(nodeEvent);
                            break;
                        case BehaviourTreeNodeInfoEventType.Terminate:
                            observer.OnNodeTerminate(nodeEvent);
                            break;
                        case BehaviourTreeNodeInfoEventType.Reset:
                            observer.OnNodeReset(nodeEvent);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // 옵서버 예외가 트리 실행을 막지 않도록
                    Debug.WriteLine($"Observer error: {ex.Message}");
                }
            }
        }
    }
}
```

**장점:**
- ✅ 메모리 누수 방지
- ✅ 트리별 격리
- ✅ Thread-safe
- ✅ 풍부한 이벤트 정보
- ✅ 성능 최적화 (관찰자 없으면 오버헤드 없음)

---

### **Solution 2: 하위 호환성 유지 + 새 시스템 추가**

기존 static 이벤트는 **Deprecated** 마킹하고 유지, 새 시스템 추가:

```csharp
public abstract class BaseBehaviour
{
    [Obsolete("Use AttachObserver/DetachObserver instead. Static events cause memory leaks.")]
    public static event EventHandler<BehaviourTreeEventArgs> StatusChangeEvent;

    // 새 시스템
    private readonly List<IBehaviourTreeObserver> _observers = new List<IBehaviourTreeObserver>();

    // ...
}
```

---

### **Solution 3: 조건부 이벤트 (성능 최적화)**

```csharp
public class BehaviourTreeConfig
{
    public static bool EnableEvents { get; set; } = false;  // 기본 비활성화
    public static bool EnableDetailedEvents { get; set; } = false;
}

protected void NotifyObservers(...)
{
    if (!BehaviourTreeConfig.EnableEvents) return;
    // ...
}
```

---

## 📊 **그래프 시스템 문제점 및 솔루션**

### ❌ **문제점 1: IClock 제약**

**현재 구현:**
```csharp
public static string Format<TContext>(IBehaviour<TContext> bt, FormatOptions option)
    where TContext : IClock
```

**문제:**
- TimeProvider를 사용하는 컨텍스트는 그래프 생성 불가능
- 불필요한 제약 (그래프 생성에 시간 정보 필요 없음)

**심각도:** 🔴 Critical

**솔루션:**
```csharp
// IClock 제약 제거
public static string Format<TContext>(IBehaviour<TContext> bt, FormatOptions option)
{
    // 그래프는 트리 구조만 필요하므로 IClock 불필요
}
```

---

### ❌ **문제점 2: 누락된 노드 타입**

**누락:**
- ActiveSelector, ActiveSequence
- Parallel (N-child)
- WaitRenew, CooldownRenew
- AsyncAction
- AfterSuccess, AfterFailed
- 모든 Renew 계열

**심각도:** 🟡 Medium

**솔루션:**
```csharp
private static string GetMarksign<TContext>(IBehaviour<TContext> obj)
{
    return obj switch
    {
        // 새 노드 추가
        ActiveSelector<TContext> => "[?A]",  // Active
        ActiveSequence<TContext> => "[->A]",
        Parallel<TContext> parallel => $"[={parallel.SuccessRequired}]",

        // Random 계열
        RandomSelector<TContext> => "[?R]",
        RandomSequence<TContext> => "[->R]",

        // Priority 계열
        PrioritySelector<TContext> => "[?P]",
        PrioritySequence<TContext> => "[->P]",

        // 일반
        Selector<TContext> => "[?]",
        Sequence<TContext> => "[->]",
        SimpleParallel<TContext> => "[=2]",

        // Leaf 노드
        Condition<TContext> => "(?)",
        ActionBehaviour<TContext> => "(!)",
        AsyncAction<TContext> => "(!A)",
        Wait<TContext> => "(~)",
        WaitRenew<TContext> => "(~R)",

        // Decorator 폴백
        DecoratorBehaviour<TContext> => $"<{GetDecoratorSymbol(obj)}>",

        _ => $"[{obj.GetType().Name}]"
    };
}

private static string GetDecoratorSymbol<TContext>(IBehaviour<TContext> obj)
{
    return obj switch
    {
        Retry<TContext> => "Retry",
        Repeater<TContext> => "Repeat",
        Inverter<TContext> => "!",
        Cooldown<TContext> => "CD",
        CooldownRenew<TContext> => "CDR",
        TimeLimiter<TContext> => "TL",
        RateLimiter<TContext> => "RL",
        UntilSuccess<TContext> => "US",
        UntilFailed<TContext> => "UF",
        Succeeder<TContext> => "✓",
        Failer<TContext> => "✗",
        AfterSuccess<TContext> => "→S",
        AfterFailed<TContext> => "→F",
        _ => obj.GetType().Name
    };
}
```

---

### ❌ **문제점 3: Dynamic Dispatch 성능 저하**

**현재 구현:**
```csharp
private static void RenderBehaviourTree<TContext>(StringBuilder text, int depth, IBehaviour<TContext> behaviour)
{
    RenderBehaviourTree(text, depth, (dynamic)behaviour);  // ❌ 느림
}
```

**문제:**
- Dynamic dispatch는 리플렉션 사용 → 느림
- 런타임 오류 가능성

**솔루션 1: Pattern Matching (C# 7.0+)**
```csharp
private static void RenderBehaviourTree<TContext>(StringBuilder text, int depth, IBehaviour<TContext> behaviour)
{
    switch (behaviour)
    {
        case CompositeBehaviour<TContext> composite:
            RenderComposite(text, depth, composite);
            break;
        case DecoratorBehaviour<TContext> decorator:
            RenderDecorator(text, depth, decorator);
            break;
        default:
            RenderLeaf(text, depth, behaviour);
            break;
    }
}
```

**솔루션 2: Visitor Pattern**
```csharp
public interface IBehaviourVisitor<TContext>
{
    void Visit(CompositeBehaviour<TContext> composite);
    void Visit(DecoratorBehaviour<TContext> decorator);
    void Visit(BaseBehaviour<TContext> leaf);
}

// IBehaviour에 추가
public interface IBehaviour<in TContext>
{
    void Accept<TVisitor>(TVisitor visitor) where TVisitor : IBehaviourVisitor<TContext>;
}
```

---

## 🎯 **우선순위별 수정 권장사항**

### 🔥 **즉시 수정 필요** (Critical)
1. ✅ 그래프 시스템의 IClock 제약 제거
2. ✅ 누락된 노드 타입 추가
3. ⚠️ static 이벤트 Deprecated 마킹

### ⚠️ **곧 수정 필요** (High)
4. 새로운 Observer 패턴 이벤트 시스템 추가
5. Dynamic dispatch를 pattern matching으로 교체

### 📌 **개선 권장** (Medium)
6. 조건부 이벤트 시스템 추가 (성능 최적화)
7. 트리 시각화 개선 (색상, 상태 표시)

---

## 📝 **Breaking Changes 분석**

### **이벤트 시스템 변경**
- **Breaking**: static 이벤트 제거 시 → 기존 코드 깨짐
- **Solution**: Deprecated 마킹 후 새 시스템 추가 → 하위 호환성 유지

### **그래프 시스템 변경**
- **Non-breaking**: IClock 제약 제거 → 기존 코드 동작 유지
- **Non-breaking**: 누락 노드 추가 → 기존 코드 동작 유지
- **Non-breaking**: Dynamic → Pattern matching → 내부 구현 변경

---

## 🧪 **테스트 전략**

### **이벤트 시스템**
```csharp
[Test]
public void Observer_ShouldNotCauseMemoryLeak()
{
    var tree = CreateTree();
    var observer = new TestObserver();
    tree.AttachObserver(observer);
    tree.DetachObserver(observer);

    // GC 후 observer가 해제되는지 확인
    WeakReference weakRef = new WeakReference(tree);
    tree = null;
    GC.Collect();
    Assert.IsFalse(weakRef.IsAlive);
}

[Test]
public void Observer_ShouldBeThreadSafe()
{
    var tree = CreateTree();
    var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
    {
        var observer = new TestObserver();
        tree.AttachObserver(observer);
        tree.Tick(context);
        tree.DetachObserver(observer);
    }));

    Task.WaitAll(tasks.ToArray());
    // 예외 없이 완료되어야 함
}
```

### **그래프 시스템**
```csharp
[Test]
public void Graph_ShouldSupportAllNodeTypes()
{
    var tree = BuildComplexTree();  // 모든 노드 타입 포함
    var graph = BehaviourTreeGraph.Format(tree, FormatOptions.Plantuml);

    // 모든 노드가 표현되는지 확인
    Assert.Contains("[?A]", graph);  // ActiveSelector
    Assert.Contains("[->A]", graph);  // ActiveSequence
    Assert.Contains("[=", graph);  // Parallel
}

[Test]
public void Graph_ShouldWorkWithoutIClock()
{
    var tree = BuildTree<NonClockContext>();
    var graph = BehaviourTreeGraph.Format(tree, FormatOptions.Plantuml);

    Assert.IsNotNull(graph);
    Assert.IsNotEmpty(graph);
}
```
