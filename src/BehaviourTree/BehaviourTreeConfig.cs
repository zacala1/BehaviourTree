using System;

namespace BehaviourTree
{
    /// <summary>
    /// Global configuration settings for behavior tree execution and monitoring.
    /// These settings allow runtime control over debugging features and performance optimizations.
    /// </summary>
    /// <remarks>
    /// PERFORMANCE OPTIMIZATION:
    /// Setting EnableEvents = false provides maximum performance by completely eliminating
    /// observer notification overhead. This is recommended for production builds where
    /// debugging/monitoring is not needed.
    ///
    /// Memory and CPU Impact:
    /// - EnableEvents = false: Zero overhead, no function calls, no allocations
    /// - EnableEvents = true, no observers attached: Minimal overhead (early return check)
    /// - EnableEvents = true, observers attached: Full event system active
    ///
    /// Thread Safety:
    /// These properties are designed for initialization-time configuration only.
    /// Changing values during tree execution is NOT thread-safe and may cause
    /// inconsistent behavior. Set these values once at application startup.
    /// </remarks>
    public static class BehaviourTreeConfig
    {
        private static bool _enableEvents = false;
        private static bool _enableDetailedTiming = false;
        private static bool _enableSlowNodeWarnings = true;

        /// <summary>
        /// Gets or sets whether the observer event system is enabled globally.
        /// Default: false (disabled for maximum performance)
        /// </summary>
        /// <remarks>
        /// PERFORMANCE IMPACT:
        /// - false: Zero observer notification overhead (recommended for production)
        /// - true: Observer notifications active (use for debugging/monitoring)
        ///
        /// WHEN TO ENABLE:
        /// - Development/debugging: Enable to receive detailed node execution events
        /// - Testing: Enable to verify behavior tree execution flow
        /// - Production: Keep disabled unless monitoring is required
        ///
        /// EXAMPLE USAGE:
        /// <code>
        /// // Enable events for debugging
        /// #if DEBUG
        ///     BehaviourTreeConfig.EnableEvents = true;
        /// #endif
        /// </code>
        /// </remarks>
        public static bool EnableEvents
        {
            get => _enableEvents;
            set => _enableEvents = value;
        }

        /// <summary>
        /// Gets or sets whether detailed execution timing is collected for every node.
        /// Default: false (timing only collected when observers are attached)
        /// </summary>
        /// <remarks>
        /// PERFORMANCE IMPACT:
        /// - false: Stopwatch created only when HasObservers == true
        /// - true: Stopwatch created for every node tick (slower)
        ///
        /// This setting is only meaningful when EnableEvents = true.
        ///
        /// WHEN TO ENABLE:
        /// - Profiling: Enable to collect detailed timing data for every node
        /// - Performance analysis: Identify slow nodes in the behavior tree
        /// - Normal debugging: Keep disabled, timing is collected when observers attached
        ///
        /// OVERHEAD:
        /// For a tree with 100 nodes running at 60 FPS:
        /// - false: ~0 Stopwatch allocations/sec (none if no observers)
        /// - true: ~6,000 Stopwatch allocations/sec
        /// </remarks>
        public static bool EnableDetailedTiming
        {
            get => _enableDetailedTiming;
            set => _enableDetailedTiming = value;
        }

        /// <summary>
        /// Gets or sets whether slow node warnings are logged in DEBUG builds.
        /// Default: true (warnings enabled in DEBUG mode)
        /// </summary>
        /// <remarks>
        /// This setting only affects DEBUG builds. In RELEASE builds, slow node
        /// detection is completely compiled out regardless of this setting.
        ///
        /// WHEN TO DISABLE:
        /// - Intentionally slow nodes: Disable to reduce log noise
        /// - Performance testing: Disable to avoid Debug.WriteLine overhead
        /// - Normal debugging: Keep enabled to catch performance issues
        ///
        /// PERFORMANCE IMPACT:
        /// - Minimal in DEBUG builds (only adds a comparison and conditional log)
        /// - Zero in RELEASE builds (compiled out via #if DEBUG)
        /// </remarks>
        public static bool EnableSlowNodeWarnings
        {
            get => _enableSlowNodeWarnings;
            set => _enableSlowNodeWarnings = value;
        }

        /// <summary>
        /// Resets all configuration settings to their default values.
        /// </summary>
        /// <remarks>
        /// This is primarily useful for testing scenarios where you need
        /// to ensure a clean configuration state between test runs.
        /// </remarks>
        public static void ResetToDefaults()
        {
            _enableEvents = false;
            _enableDetailedTiming = false;
            _enableSlowNodeWarnings = true;
        }

        /// <summary>
        /// Configures settings optimized for production use (maximum performance).
        /// Disables all debugging and monitoring features.
        /// </summary>
        public static void ConfigureForProduction()
        {
            _enableEvents = false;
            _enableDetailedTiming = false;
            _enableSlowNodeWarnings = false;
        }

        /// <summary>
        /// Configures settings optimized for development/debugging.
        /// Enables events and warnings but keeps detailed timing conditional.
        /// </summary>
        public static void ConfigureForDevelopment()
        {
            _enableEvents = true;
            _enableDetailedTiming = false;  // Only time when observers attached
            _enableSlowNodeWarnings = true;
        }

        /// <summary>
        /// Configures settings optimized for performance profiling.
        /// Enables all timing and monitoring features.
        /// </summary>
        public static void ConfigureForProfiling()
        {
            _enableEvents = true;
            _enableDetailedTiming = true;  // Always collect timing data
            _enableSlowNodeWarnings = true;
        }
    }
}
