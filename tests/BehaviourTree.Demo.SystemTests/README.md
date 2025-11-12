# BehaviourTree.Demo System Tests

이 프로젝트는 BehaviourTree.Demo의 모든 컴포넌트, 시스템, 그리고 게임 엔진의 기능을 검증하는 통합 시스템 테스트 프로젝트입니다.

## 테스트 범위

### 1. Components (컴포넌트)
- **HealthComponent**: 체력 관리 기능 테스트
  - 생성자 초기화
  - 체력 감소 (ReduceBy)
  - 체력 증가 (IncreaseBy)
  - 최대/최소값 경계 조건

- **StaminaComponent**: 스태미나 관리 기능 테스트
  - 생성자 초기화
  - 스태미나 감소/증가
  - 최대/최소값 경계 조건

- **InventoryComponent**: 인벤토리 관리 기능 테스트
  - 아이템 추가 (Add)
  - 아이템 제거 (Remove)
  - 아이템 보유 확인 (Has)
  - 아이템 개수 확인 (Count)
  - 여러 아이템 타입 관리

- **LootableComponent**: 루팅 가능한 아이템 기능 테스트
  - 생성자 초기화
  - 부분 루팅 (Loot)
  - 전체 루팅 (LootAll)
  - 수량 관리

### 2. GameEngine (게임 엔진)
- **Engine**: 엔진 핵심 기능 테스트
  - 엔티티 생성 및 관리
  - 시스템 추가/제거
  - Update 호출

- **Entity**: 엔티티 기능 테스트
  - 엔티티 생성
  - 컴포넌트 추가/제거
  - 컴포넌트 조회
  - 이벤트 발생 (ComponentAdded/ComponentRemoved)
  - 메서드 체이닝

- **EventSystem**: 이벤트 시스템 테스트
  - 이벤트 발행 (PublishEvent)
  - 이벤트 구독 (SubscribeToEvent)
  - 이벤트 구독 해제 (UnsubscribeFromEvent)
  - 여러 리스너 처리
  - 엔티티 추가/제거 자동 이벤트

### 3. Systems (시스템)
- **AiSystem**: AI 시스템 테스트
  - 시스템 초기화
  - 행동 트리 실행
  - 다중 엔티티 처리
  - 컨텍스트 풀링 (Context Pooling)
  - 컨텍스트 관리 (Rent/Return)
  - 행동 트리 상태 처리 (Success/Failure/Running)
  - 대규모 엔티티 처리 성능

### 4. Utility Classes (유틸리티 클래스)
- **RingBuffer**: 순환 버퍼 테스트
  - 초기화 및 용량 관리
  - Enqueue/Dequeue 동작
  - EnqueueOverwrite (오버라이트 모드)
  - TryPeek (peek 동작)
  - Clear 및 순환 동작
  - IEnumerable 구현
  - CopyTo 기능

- **ObjectPool**: 객체 풀링 테스트
  - 풀 초기화
  - Rent/Return 동작
  - 최대 크기 제한
  - Reset 콜백 실행
  - 객체 재사용
  - Clear 기능

## 테스트 실행 방법

### .NET CLI 사용
```bash
# 모든 타겟 프레임워크에서 테스트 실행
dotnet test

# 특정 프레임워크에서 테스트 실행
dotnet test --framework net8.0
dotnet test --framework net6.0
dotnet test --framework net48
```

### Visual Studio
1. Test Explorer를 엽니다 (Test > Test Explorer)
2. "Run All Tests" 버튼을 클릭합니다

### Rider
1. Unit Tests 창을 엽니다
2. 전체 테스트를 실행하거나 개별 테스트를 선택하여 실행합니다

## 프로젝트 구조

```
BehaviourTree.Demo.SystemTests/
├── Components/
│   ├── HealthComponentTests.cs
│   ├── StaminaComponentTests.cs
│   ├── InventoryComponentTests.cs
│   └── LootableComponentTests.cs
├── GameEngine/
│   ├── EngineTests.cs
│   ├── EntityTests.cs
│   ├── EventSystemTests.cs
│   ├── RingBufferTests.cs
│   └── ObjectPoolTests.cs
├── Systems/
│   └── AiSystemTests.cs
└── README.md
```

## 테스트 통계

- **총 테스트 수**: 110개 이상
- **테스트 커버리지**: 컴포넌트, 시스템, 및 게임 엔진 핵심 기능 포괄
- **타겟 프레임워크**: net8.0, net6.0, net48

## 의존성

- xUnit 2.4.2
- Microsoft.NET.Test.Sdk 17.6.0
- BehaviourTree.Demo 프로젝트 참조

## 참고 사항

이 테스트 프로젝트는 BehaviourTree.Demo의 전체 시스템을 검증하는 통합 테스트를 포함합니다:
- 컴포넌트 단위 테스트
- 게임 엔진 핵심 기능 테스트
- AI 시스템 통합 테스트
- 유틸리티 클래스 테스트

각 테스트는 독립적으로 실행 가능하며, 테스트 간에 상태를 공유하지 않습니다.
