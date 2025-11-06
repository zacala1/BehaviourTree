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
        /// Creates a sub behavior tree as a child node.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="subBehaviour">Sub behavior tree</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static FluentBuilder<TContext> Subtree<TContext>(
            this FluentBuilder<TContext> builder,
            IBehaviour<TContext> subBehaviour)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.PushLeaf(() => subBehaviour);
        }

        /// <summary>
        /// Creates a <see cref="Behaviours.Condition{TContext}"/> node.
        /// The Condition node returns <see cref="BehaviourStatus.Succeeded"/> if the condition result is true,
        /// and <see cref="BehaviourStatus.Failed"/> if false.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="condition">Condition expression to apply to the child node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
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
        /// Creates an <see cref="ActionBehaviour{TContext}"/> node.
        /// The Do node executes an action and returns its result.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="action">Action to execute in the child node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
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
        /// Creates a <see cref="Behaviours.Wait{TContext}"/> node.
        /// The Wait node waits for the specified wait time.
        /// Returns <see cref="BehaviourStatus.Running"/> while waiting and <see cref="BehaviourStatus.Succeeded"/> after the wait completes.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="waitTimeInMilliseconds">Wait time (milliseconds)</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
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
        /// Creates a <see cref="Composites.PrioritySelector{TContext}"/> node.
        /// Always starts from the first child node when executed,
        /// and returns <see cref="BehaviourStatus.Succeeded"/> when one of the child nodes succeeds.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Composites.PrioritySelector{TContext}"/> node.
        /// Always executes from the first child node when executed,
        /// and returns <see cref="BehaviourStatus.Failed"/> when any child node fails.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Composites.Selector{TContext}"/> node.
        /// Remembers the child node's index and executes sequentially starting from that index.
        /// Returns <see cref="BehaviourStatus.Succeeded"/> when one of the child nodes succeeds.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Composites.Sequence{TContext}"/> node.
        /// Remembers the child node's index and executes sequentially starting from that index.
        /// Returns <see cref="BehaviourStatus.Failed"/> when any child node fails.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Composites.RandomSequence{TContext}"/> node.
        /// Executes child nodes in random order.
        /// Returns <see cref="BehaviourStatus.Failed"/> when any child node fails.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="randomProvider">Random number generator</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Composites.RandomSequence{TContext}"/> node.
        /// Executes child nodes in random order.
        /// Returns <see cref="BehaviourStatus.Succeeded"/> when one of the child nodes succeeds.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="randomProvider">Random number generator</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Composites.SimpleParallel{TContext}"/> node.
        /// Can only have 2 child nodes. The termination condition varies based on the parallel node processing policy.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="policy">Parallel node processing policy</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Decorators.AutoReset{TContext}"/> node.
        /// Resets the child node's state when the child node completes execution, regardless of the result.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Decorators.Cooldown{TContext}"/> node.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="cooldownTimeInMilliseconds">Cooldown time (milliseconds)</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Failer{TContext}"/> node.
        /// Returns <see cref="BehaviourStatus.Failed"/> regardless of whether the child node's result is success or failure.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Succeeder{TContext}"/> node.
        /// Returns <see cref="BehaviourStatus.Succeeded"/> regardless of whether the child node's result is success or failure.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates an <see cref="Inverter{TContext}"/> node.
        /// Returns <see cref="BehaviourStatus.Succeeded"/> if the child node's result is failure,
        /// and returns <see cref="BehaviourStatus.Failed"/> if the child node's result is success.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="RateLimiter{TContext}"/> node.
        /// Executes the child node after the delay time. Returns <see cref="BehaviourStatus.Running"/> during the delay time,
        /// and returns the child node's result afterwards.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="intervalInMilliseconds">Delay time (milliseconds)</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Repeater{TContext}"/> node.
        /// Returns <see cref="BehaviourStatus.Succeeded"/> when the child node's result succeeds <paramref name="repeatCount"/> times.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="repeatCount">Repeat count</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Decorators.Retry{TContext}"/> node.
        /// If the child node's result is failure, returns <see cref="BehaviourStatus.Running"/> repeatedly for <paramref name="retryCount"/> times.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="retryCount">Retry count</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Repeater{TContext}"/> node.
        /// Returns <see cref="BehaviourStatus.Failed"/> if the child node does not succeed within <paramref name="timeLimitInMilliseconds"/>.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="timeLimitInMilliseconds">Time limit (milliseconds)</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Decorators.UntilSuccess{TContext}"/> node.
        /// Retries the child node until it succeeds. On failure, retries <paramref name="countdown"/> times.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="countdown">Number of retries on failure</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Decorators.UntilFailed{TContext}"/> node.
        /// Retries the child node until it fails. On success, retries <paramref name="countdown"/> times.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="countdown">Number of retries on failure</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Decorators.UntilSuccess{TContext}"/> node.
        /// Retries the child node until it succeeds. On failure, retries <paramref name="countdown"/> times.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="countdown">Number of retries on failure</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="CSP.Foundation.BehaviourTree.Decorators.Random{TContext}"/> node.
        /// Probabilistically executes the child node or returns <see cref="BehaviourStatus.Failed"/>.
        /// Executes the child node only when the generated random number is greater than the <paramref name="threshold"/> value.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="threshold">Threshold value (between 0.0 and 1.0)</param>
        /// <param name="randomProvider">Random number generator</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates an <see cref="AsyncAction{TContext}"/> node.
        /// Executes and waits for an asynchronous function. Cancels if no result occurs within the timeout period.
        /// When using the context of an asynchronous function, be careful of resource race conditions.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="action">Asynchronous action</param>
        /// <param name="timeout">Timeout duration</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
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
        /// Creates a <see cref="WaitRenew{TContext}"/> node.
        /// The Wait node is renewed by <paramref name="getWaitTimeInMilliseconds"/> during initialization.
        /// Returns <see cref="BehaviourStatus.Running"/> while waiting and <see cref="BehaviourStatus.Succeeded"/> after the wait completes.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="getWaitTimeInMilliseconds">Delegate to renew the wait time (milliseconds)</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
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
        /// Creates a <see cref="TimeLimiterRenew{TContext}"/> node.
        /// When the node is initialized, the time limit is renewed by <paramref name="getTimeLimitInMilliseconds"/>.
        /// Returns <see cref="BehaviourStatus.Failed"/> if the child node does not succeed within the time limit.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="getTimeLimitInMilliseconds">Delegate to renew the time limit (milliseconds)</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="CooldownRenew{TContext}"/> node.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="getCooldownTimeInMilliseconds">Delegate to renew the cooldown time (milliseconds)</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="RateLimiterRenew{TContext}"/> node.
        /// Executes the child node after the delay time. When the node is initialized, the delay time is renewed by <paramref name="getIntervalInMilliseconds"/>.
        /// Returns <see cref="BehaviourStatus.Running"/> during the delay time,
        /// and returns the child node's result afterwards.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="getIntervalInMilliseconds">Delay time (milliseconds)</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="RepeaterRenew{TContext}"/> node.
        /// Returns <see cref="BehaviourStatus.Succeeded"/> when the child node's result succeeds the specified number of times.
        /// When the node is initialized, the repeat count is renewed by <paramref name="getRepeatCount"/>.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="getRepeatCount">Delegate to renew the repeat count</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="CSP.Foundation.BehaviourTree.Decorators.RetryRenew{TContext}"/> node.
        /// If the child node's result is failure, returns <see cref="BehaviourStatus.Running"/> repeatedly for <paramref name="getRetryCount"/> times.
        /// When the node is initialized, the retry count is renewed by <paramref name="getRetryCount"/>.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="getRetryCount">Delegate to renew the retry count</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Decorators.UntilSuccess{TContext}"/> node.
        /// Retries the child node until it succeeds. On failure, retries <paramref name="getCountdown"/> times.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="getCountdown">Number of retries on failure</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Decorators.UntilFailed{TContext}"/> node.
        /// Retries the child node until it fails. On success, retries <paramref name="getCountdown"/> times.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="getCountdown">Number of retries on failure</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Decorators.UntilSuccessWithinTimeout{TContext}"/> node.
        /// Retries the child node until it succeeds. On failure, retries <paramref name="getTimeoutInMilliseconds"/> times.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="getTimeoutInMilliseconds">Number of retries on failure</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Decorator node.
        /// Decorator nodes can have one child and must call End() at the end.
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
        /// Creates a <see cref="Composites.ActiveSelector{TContext}"/> node.
        /// An alias for PrioritySelector, always starts from the first child node when executed and re-evaluates every tick.
        /// Returns <see cref="BehaviourStatus.Succeeded"/> when one of the child nodes succeeds.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Composites.ActiveSequence{TContext}"/> node.
        /// An alias for PrioritySequence, always starts from the first child node when executed and re-evaluates every tick.
        /// Returns <see cref="BehaviourStatus.Failed"/> when any child node fails.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Composites.Parallel{TContext}"/> node.
        /// Executes all child nodes in parallel, and the termination condition varies based on the policy.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="policy">Parallel node processing policy</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
        /// Creates a <see cref="Composites.Parallel{TContext}"/> node.
        /// Executes all child nodes in parallel, and succeeds when N children succeed.
        /// </summary>
        /// <param name="builder">Behavior tree builder</param>
        /// <param name="name">The display name of the node</param>
        /// <param name="successRequired">Number of child nodes required to succeed</param>
        /// <typeparam name="TContext">Context used in the behavior tree</typeparam>
        /// <returns>The applied behavior tree builder</returns>
        /// <remarks>
        /// This node type is a Composite node.
        /// Composite nodes can have multiple children and must call End() at the end.
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
