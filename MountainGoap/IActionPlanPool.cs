// <copyright file="IActionPlanPool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Pool for <see cref="ActionPlan"/> instances. One plan is rented per A* search
    /// and either given to the agent (best plan) or returned via <see cref="ActionPlan.Dispose"/>.
    /// </summary>
    internal interface IActionPlanPool {
        /// <summary>
        /// Rents an <see cref="ActionPlan"/> initialized for a new search.
        /// </summary>
        ActionPlan Rent(IActionNodePool nodePool);

        /// <summary>
        /// Returns a disposed <see cref="ActionPlan"/> to the pool for reuse.
        /// </summary>
        void Return(ActionPlan plan);
    }
}
