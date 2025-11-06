using BehaviourTree.Behaviours;
using BehaviourTree.Composites;
using BehaviourTree.Decorators;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BehaviourTree.FluentBuilder
{
    public static class FluentBuilderExtensions
    {
        /// <summary>
        /// 자식 노드로 하위 행동트리를 생성한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="subBehaviour">하위 행동트리</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Subtree<TContext>(
            this FluentBuilder<TContext> builder,
            IBehaviour<TContext> subBehaviour)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushLeaf(() => subBehaviour);
        }

        /// <summary>
        /// <see cref="Behaviours.Condition{TContext}"/> 노드를 생성한다.
        /// Condition 노드는 조건의 결과가 참이면 <see cref="BehaviourStatus.Succeeded"/>,
        /// 거짓이면 <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="condition">자식 노드에 작용할 조건 수식</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Condition<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, bool> condition)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushLeaf(() => new Condition<TContext>(name, condition));
        }

        /// <summary>
        /// <see cref="ActionBehaviour{TContext}"/> 노드를 생성한다.
        /// Do 노드는 action을 수행하여 그 결과를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="action">자식 노드에서 실행할 액션</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Do<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, BehaviourStatus> action)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushLeaf(() => new ActionBehaviour<TContext>(name, action));
        }

        /// <summary>
        /// <see cref="Behaviours.Wait{TContext}"/> 노드를 생성한다.
        /// Wait 노드는 지정한 대기 시간만큼 대기한다.
        /// 대기 중에는 <see cref="BehaviourStatus.Running"/>, 대기 종료 후 <see cref="BehaviourStatus.Succeeded"/>을 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="waitTimeInMilliseconds">대기 시간 (milliseconds)</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Wait<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            int waitTimeInMilliseconds)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushLeaf(() => new Wait<TContext>(name, waitTimeInMilliseconds));
        }

        /// <summary>
        /// <see cref="Composites.PrioritySelector{TContext}"/> 노드를 생성한다.
        /// 동작시 항상 첫째 자식노드부터 시작하며,
        /// 자식 노드 중 하나가 성공하면 실행을 종료하고 <see cref="BehaviourStatus.Succeeded"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> PrioritySelector<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new PrioritySelector<TContext>(name, children));
        }

        /// <summary>
        /// <see cref="Composites.PrioritySelector{TContext}"/> 노드를 생성한다.
        /// 동작시 항상 첫째 자식노드부터 실행하며,
        /// 자식 노드가 하나라도 실패하면 실행을 종료하고 <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> PrioritySequence<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new PrioritySequence<TContext>(name, children));
        }

        /// <summary>
        /// <see cref="Composites.Selector{TContext}"/> 노드를 생성한다.
        /// 자식 노드의 index를 기억하여 index부터 순서대로 실행한다.
        /// 자식 노드 중 하나가 성공하면 실행을 종료하고 <see cref="BehaviourStatus.Succeeded"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Selector<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new Selector<TContext>(name, children));
        }

        /// <summary>
        /// <see cref="Composites.Sequence{TContext}"/> 노드를 생성한다.
        /// 자식 노드의 index를 기억하여 index부터 순서대로 실행한다.
        /// 자식 노드가 하나라도 실패하면 실행을 종료하고 <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Sequence<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new Sequence<TContext>(name, children));
        }

        /// <summary>
        /// <see cref="Composites.RandomSequence{TContext}"/> 노드를 생성한다.
        /// 자식 노드를 무작위로 실행한다.
        /// 자식 노드가 하나라도 실패하면 실행을 종료하고 <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="randomProvider">난수 생성기</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> RandomSequence<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            IRandomProvider randomProvider = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new RandomSequence<TContext>(name, children, randomProvider));
        }

        /// <summary>
        /// <see cref="Composites.RandomSequence{TContext}"/> 노드를 생성한다.
        /// 자식 노드를 무작위로 실행한다.
        /// 자식 노드 중 하나가 성공하면 실행을 종료하고 <see cref="BehaviourStatus.Succeeded"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="randomProvider">난수 생성기</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> RandomSelector<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            IRandomProvider randomProvider = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new RandomSelector<TContext>(name, children, randomProvider));
        }

        /// <summary>
        /// <see cref="Composites.SimpleParallel{TContext}"/> 노드를 생성한다.
        /// 자식 노드를 2개만 가질 수 있다. 병렬 노드 처리 정책에 따라 종료 조건이 달라진다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="policy">병렬 노드 처리 정책</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> SimpleParallel<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            SimpleParallelPolicy policy = SimpleParallelPolicy.BothMustSucceed)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new SimpleParallel<TContext>(name, policy, children[0], children[1]));
        }

        /// <summary>
        /// <see cref="Decorators.AutoReset{TContext}"/> 노드를 생성한다.
        /// 자식 노드가 진행을 완료하면 결과에 상관없이 자식 노드 상태를 Reset한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> AutoReset<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new AutoReset<TContext>(name, child));
        }

        /// <summary>
        /// <see cref="Decorators.Cooldown{TContext}"/> 노드를 생성한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="cooldownTimeInMilliseconds">쿨다운 시간(milliseconds)</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Cooldown<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            int cooldownTimeInMilliseconds)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new Cooldown<TContext>(name, child, cooldownTimeInMilliseconds));
        }

        /// <summary>
        /// <see cref="Failer{TContext}"/> 노드를 생성한다.
        /// 자식 노드의 결과가 성공이든 실패든 상관없이 <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> AlwaysFail<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new Failer<TContext>(name, child));
        }

        /// <summary>
        /// <see cref="Succeeder{TContext}"/> 노드를 생성한다.
        /// 자식 노드의 결과가 성공이든 실패든 상관없이 <see cref="BehaviourStatus.Succeeded"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> AlwaysSucceed<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new Succeeder<TContext>(name, child));
        }

        /// <summary>
        /// <see cref="Inverter{TContext}"/> 노드를 생성한다.
        /// 자식 노드의 결과가 실패면 <see cref="BehaviourStatus.Succeeded"/>를 반환하고,
        /// 자식 노드의 결과가 성공이면 <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Invert<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new Inverter<TContext>(name, child));
        }

        /// <summary>
        /// <see cref="RateLimiter{TContext}"/> 노드를 생성한다.
        /// 지연 시간 후에 자식 노드를 실행한다. 지연 시간 중에는 <see cref="BehaviourStatus.Running"/>을 반환하고,
        /// 이후에는 자식노드의 결과를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="intervalInMilliseconds">지연 시간(milliseconds)</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> LimitCallRate<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            int intervalInMilliseconds)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new RateLimiter<TContext>(name, child, intervalInMilliseconds));
        }

        /// <summary>
        /// <see cref="Repeater{TContext}"/> 노드를 생성한다.
        /// 자식 노드의 결과가 <paramref name="repeatCount"/> 횟수 만큼 성공하면 <see cref="BehaviourStatus.Succeeded"/>을 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="repeatCount">반복 횟수</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Repeat<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            int repeatCount)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new Repeater<TContext>(name, child, repeatCount));
        }

        /// <summary>
        /// <see cref="Decorators.Retry{TContext}"/> 노드를 생성한다.
        /// 자식 노드의 결과가 실패이면, <paramref name="retryCount"/> 횟수 만큼 반복해서 <see cref="BehaviourStatus.Running"/>을 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="retryCount">반복 횟수</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Retry<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            int retryCount)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new Retry<TContext>(name, child, retryCount));
        }

        /// <summary>
        /// <see cref="Repeater{TContext}"/> 노드를 생성한다.
        /// 자식 노드가 <paramref name="timeLimitInMilliseconds"/>만큼 경과할 동안 성공하지 못하면
        /// <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="timeLimitInMilliseconds">제한 시간(milliseconds)</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> TimeLimit<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            int timeLimitInMilliseconds)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new TimeLimiter<TContext>(name, child, timeLimitInMilliseconds));
        }

        /// <summary>
        /// <see cref="Decorators.UntilSuccess{TContext}"/> 노드를 생성한다.
        /// 자식 노드가 성공할 때까지 재시도한다. 실패하면 <paramref name="countdown"/>만큼 재시도한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="countdown">실패시 재시도할 횟수</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> UntilSuccess<TContext>(
            this FluentBuilder<TContext> builder,
            string name, int countdown = default)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new UntilSuccess<TContext>(name, child, countdown));
        }

        /// <summary>
        /// <see cref="Decorators.UntilFailed{TContext}"/> 노드를 생성한다.
        /// 자식 노드가 실패할 때까지 재시도한다. 성공하면 <paramref name="countdown"/>만큼 재시도한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="countdown">실패시 재시도할 횟수</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> UntilFailed<TContext>(
            this FluentBuilder<TContext> builder,
            string name, int countdown = default)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new UntilFailed<TContext>(name, child, countdown));
        }

        /// <summary>
        /// <see cref="Decorators.UntilSuccess{TContext}"/> 노드를 생성한다.
        /// 자식 노드가 성공할 때까지 재시도한다. 실패하면 <paramref name="countdown"/>만큼 재시도한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="countdown">실패시 재시도할 횟수</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> UntilSuccessWithinTimeout<TContext>(
            this FluentBuilder<TContext> builder,
            string name, long timeoutInMilliseconds = default,
            Action<TContext> timeoutAction = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new UntilSuccessWithinTimeout<TContext>(name, child, timeoutInMilliseconds, timeoutAction));
        }

        /// <summary>
        /// <see cref="CSP.Foundation.BehaviourTree.Decorators.Random{TContext}"/> 노드를 생성한다.
        /// 확률적으로 자식 노드를 실행하거나 <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// 생성된 난수가 <paramref name="threshold"/>값보다 커야 자식 노드를 실행한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="threshold">문턱값(0.0 ~ 1.0 사이값)</param>
        /// <param name="randomProvider">난수 생성자</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Random<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            double threshold,
            IRandomProvider randomProvider = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushDecorate(child => new Random<TContext>(name, child, threshold, randomProvider));
        }

        /// <summary>
        /// <see cref="AsyncAction{TContext}"/> 노드를 생성한다.
        /// 비동기 함수를 실행하고 기다린다. 타임아웃 시간만큼 결과가 발생하지 않으면 Cancel한다.
        /// 비동기 함수의 context를 사용시 리소스 경쟁상태를 주의하여야 한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="action">비동기 액션</param>
        /// <param name="timeout">타임 아웃 시간</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> DoAsync<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, CancellationToken, Task<BehaviourStatus>> action,
            TimeSpan timeout = default)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (action is null) throw new ArgumentNullException(nameof(action));
            return builder.PushLeaf(() => new AsyncAction<TContext>(name, action, timeout));
        }

        /// <summary>
        /// <see cref="WaitRenew{TContext}"/> 노드를 생성한다.
        /// Wait 노드는 초기화시에 <paramref name="getWaitTimeInMilliseconds"/>에 의해서 갱신된다.
        /// 대기 중에는 <see cref="BehaviourStatus.Running"/>, 대기 종료 후 <see cref="BehaviourStatus.Succeeded"/>을 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="getWaitTimeInMilliseconds">대기 시간을 갱신하는 델리게이트(milliseconds)</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Wait<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, long> getWaitTimeInMilliseconds)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (getWaitTimeInMilliseconds is null) throw new ArgumentNullException(nameof(getWaitTimeInMilliseconds));
            return builder.PushLeaf(() => new WaitRenew<TContext>(name, getWaitTimeInMilliseconds));
        }

        /// <summary>
        /// <see cref="TimeLimiterRenew{TContext}"/> 노드를 생성한다.
        /// 노드가 초기화될 때, <paramref name="getTimeLimitInMilliseconds"/>애 의해서 제한 시간을 갱신한다.
        /// 자식 노드가 제한 시간을 경과할 동안 성공하지 못하면 <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="getTimeLimitInMilliseconds">제한 시간을 갱신하는 델리게이트(milliseconds)</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> TimeLimit<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, long> getTimeLimitInMilliseconds)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (getTimeLimitInMilliseconds is null) throw new ArgumentNullException(nameof(getTimeLimitInMilliseconds));
            return builder.PushDecorate(child => new TimeLimiter<TContext>(name, child, getTimeLimitInMilliseconds));
        }

        /// <summary>
        /// <see cref="CooldownRenew{TContext}"/> 노드를 생성한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="getCooldownTimeInMilliseconds">쿨다운 시간을 갱신하는 델리게이트(milliseconds)</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Cooldown<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, long> getCooldownTimeInMilliseconds)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (getCooldownTimeInMilliseconds is null) throw new ArgumentNullException(nameof(getCooldownTimeInMilliseconds));
            return builder.PushDecorate(child => new CooldownRenew<TContext>(name, child, getCooldownTimeInMilliseconds));
        }

        /// <summary>
        /// <see cref="RateLimiterRenew{TContext}"/> 노드를 생성한다.
        /// 지연 시간 후에 자식 노드를 실행한다. 노드가 초기화될 때, <paramref name="getIntervalInMilliseconds"/>애 의해서 지연 시간을 갱신한다.
        /// 지연 시간 중에는 <see cref="BehaviourStatus.Running"/>을 반환하고,
        /// 이후에는 자식노드의 결과를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="getIntervalInMilliseconds">지연 시간(milliseconds)</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> LimitCallRate<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, long> getIntervalInMilliseconds)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (getIntervalInMilliseconds is null) throw new ArgumentNullException(nameof(getIntervalInMilliseconds));
            return builder.PushDecorate(child => new RateLimiter<TContext>(name, child, getIntervalInMilliseconds));
        }

        /// <summary>
        /// <see cref="RepeaterRenew{TContext}"/> 노드를 생성한다.
        /// 자식 노드의 결과가 반복 횟수 만큼 성공하면 <see cref="BehaviourStatus.Succeeded"/>을 반환한다.
        /// 노드가 초기화될 때, <paramref name="getRepeatCount"/>애 의해서 반복 횟수를 갱신한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="getRepeatCount">반복 횟수를 갱신하는 델리게이트</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Repeat<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, int> getRepeatCount)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (getRepeatCount is null) throw new ArgumentNullException(nameof(getRepeatCount));
            return builder.PushDecorate(child => new Repeater<TContext>(name, child, getRepeatCount));
        }

        /// <summary>
        /// <see cref="CSP.Foundation.BehaviourTree.Decorators.RetryRenew{TContext}"/> 노드를 생성한다.
        /// 자식 노드의 결과가 실패이면, <paramref name="getRetryCount"/> 횟수 만큼 반복해서 <see cref="BehaviourStatus.Running"/>을 반환한다.
        /// 노드가 초기화될 때, <paramref name="getRetryCount"/>애 의해서 반복 횟수를 갱신한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="getRetryCount">반복 횟수를 갱신하는 델리게이트</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Retry<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, int> getRetryCount)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (getRetryCount is null) throw new ArgumentNullException(nameof(getRetryCount));
            return builder.PushDecorate(child => new Retry<TContext>(name, child, getRetryCount));
        }

        /// <summary>
        /// <see cref="Decorators.UntilSuccess{TContext}"/> 노드를 생성한다.
        /// 자식 노드가 성공할 때까지 재시도한다. 실패하면 <paramref name="getCountdown"/>만큼 재시도한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="getCountdown">실패시 재시도할 횟수</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> UntilSuccess<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, int> getCountdown)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (getCountdown is null) throw new ArgumentNullException(nameof(getCountdown));
            return builder.PushDecorate(child => new UntilSuccess<TContext>(name, child, getCountdown));
        }

        /// <summary>
        /// <see cref="Decorators.UntilFailed{TContext}"/> 노드를 생성한다.
        /// 자식 노드가 실패할 때까지 재시도한다. 성공하면 <paramref name="getCountdown"/>만큼 재시도한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="getCountdown">실패시 재시도할 횟수</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> UntilFailed<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, int> getCountdown)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (getCountdown is null) throw new ArgumentNullException(nameof(getCountdown));
            return builder.PushDecorate(child => new UntilFailed<TContext>(name, child, getCountdown));
        }

        public static FluentBuilder<TContext> AfterFailed<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Action<TContext> actionAfterFailed)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (actionAfterFailed is null) throw new ArgumentNullException(nameof(actionAfterFailed));
            return builder.PushDecorate(child => new AfterFailed<TContext>(name, child, actionAfterFailed));
        }

        public static FluentBuilder<TContext> AfterSuccess<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Action<TContext> actionAfterSuccess)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (actionAfterSuccess is null) throw new ArgumentNullException(nameof(actionAfterSuccess));
            return builder.PushDecorate(child => new AfterSuccess<TContext>(name, child, actionAfterSuccess));
        }

        /// <summary>
        /// <see cref="Decorators.UntilSuccessWithinTimeout{TContext}"/> 노드를 생성한다.
        /// 자식 노드가 성공할 때까지 재시도한다. 실패하면 <paramref name="getTimeoutInMilliseconds"/>만큼 재시도한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="getTimeoutInMilliseconds">실패시 재시도할 횟수</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Decorate 노드이다.
        /// Decorate 노드는 하나의 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> UntilSuccessWithinTimeout<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            Func<TContext, long> getTimeoutInMilliseconds,
            Action<TContext> timeoutAction = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (getTimeoutInMilliseconds is null) throw new ArgumentNullException(nameof(getTimeoutInMilliseconds));
            return builder.PushDecorate(child => new UntilSuccessWithinTimeout<TContext>(name, child, getTimeoutInMilliseconds, timeoutAction));
        }

        /// <summary>
        /// <see cref="Composites.ActiveSelector{TContext}"/> 노드를 생성한다.
        /// PrioritySelector의 별칭으로, 동작시 항상 첫째 자식노드부터 시작하며 매 틱마다 재평가한다.
        /// 자식 노드 중 하나가 성공하면 실행을 종료하고 <see cref="BehaviourStatus.Succeeded"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> ActiveSelector<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new ActiveSelector<TContext>(name, children));
        }

        /// <summary>
        /// <see cref="Composites.ActiveSequence{TContext}"/> 노드를 생성한다.
        /// PrioritySequence의 별칭으로, 동작시 항상 첫째 자식노드부터 시작하며 매 틱마다 재평가한다.
        /// 자식 노드가 하나라도 실패하면 실행을 종료하고 <see cref="BehaviourStatus.Failed"/>를 반환한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> ActiveSequence<TContext>(
            this FluentBuilder<TContext> builder,
            string name)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new ActiveSequence<TContext>(name, children));
        }

        /// <summary>
        /// <see cref="Composites.Parallel{TContext}"/> 노드를 생성한다.
        /// 모든 자식 노드를 병렬로 실행하며, 정책에 따라 종료 조건이 달라진다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="policy">병렬 노드 처리 정책</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Parallel<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            ParallelPolicy policy = ParallelPolicy.RequireAll)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new Parallel<TContext>(name, policy, children));
        }

        /// <summary>
        /// <see cref="Composites.Parallel{TContext}"/> 노드를 생성한다.
        /// 모든 자식 노드를 병렬로 실행하며, N개의 자식이 성공해야 성공한다.
        /// </summary>
        /// <param name="builder">행동트리 빌더</param>
        /// <param name="name">노드의 표기할 이름</param>
        /// <param name="successRequired">성공 필요한 자식 노드 개수</param>
        /// <typeparam name="TContext">행동트리에서 사용하는 context</typeparam>
        /// <returns>적용 완료된 행동트리 빌더</returns>
        /// <remarks>
        /// 위 노드의 타입은 Composite 노드이다.
        /// Composite 노드는 여러 자식을 가질 수 있으며, 반드시 마지막에 End()를 호출해야 한다.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Parallel<TContext>(
            this FluentBuilder<TContext> builder,
            string name,
            int successRequired)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushComposite(children => new Parallel<TContext>(name, successRequired, children));
        }
    }
}