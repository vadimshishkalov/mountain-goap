// <copyright file="PoolManager.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Holds shared object pools used during A* planning. Pass a single instance to
    /// <see cref="Registry"/> so all agents in the same world reuse the same allocations.
    /// Each boolean parameter controls whether that pool is created and shared (<c>true</c>)
    /// or left <c>null</c> so each consumer creates its own (<c>false</c>).
    /// Passing <c>null</c> instead of a <see cref="PoolManager"/> has the same effect as
    /// setting all flags to <c>false</c>.
    /// </summary>
    public class PoolManager {
        /// <summary>Gets the shared node pool, or <c>null</c> if per-consumer creation was requested.</summary>
        public ActionNodePool? NodePool { get; }

        /// <summary>Gets the shared graph pool, or <c>null</c> if per-consumer creation was requested.</summary>
        internal ActionGraphPool? GraphPool { get; }

        /// <summary>Gets the shared plan pool, or <c>null</c> if per-consumer creation was requested.</summary>
        internal ActionPlanPool? PlanPool { get; }

        /// <summary>Gets the shared state pool, or <c>null</c> if per-consumer creation was requested.</summary>
        public StatePool? StatePool { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolManager"/> class.
        /// </summary>
        /// <param name="shareNodePool">When <c>true</c> (default), creates and shares one <see cref="ActionNodePool"/> across all consumers.</param>
        /// <param name="shareGraphPool">When <c>true</c> (default), creates and shares one <see cref="ActionGraphPool"/> across all consumers.</param>
        /// <param name="sharePlanPool">When <c>true</c> (default), creates and shares one <see cref="ActionPlanPool"/> across all consumers.</param>
        /// <param name="shareStatePool">When <c>true</c> (default), creates and shares one <see cref="StatePool"/> across all consumers.</param>
        public PoolManager(
            bool shareNodePool = true,
            bool shareGraphPool = true,
            bool sharePlanPool = true,
            bool shareStatePool = true) {
            NodePool = shareNodePool ? new ActionNodePool() : null;
            GraphPool = shareGraphPool ? new ActionGraphPool() : null;
            PlanPool = sharePlanPool ? new ActionPlanPool() : null;
            StatePool = shareStatePool ? new StatePool() : null;
        }
    }
}
