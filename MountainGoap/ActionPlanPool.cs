// <copyright file="ActionPlanPool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Concurrent;

    /// <summary>
    /// Stack-backed pool for <see cref="ActionPlan"/> instances.
    /// </summary>
    internal class ActionPlanPool : IActionPlanPool {
        private readonly ConcurrentStack<ActionPlan> pool = new();

        ActionPlan IActionPlanPool.Rent(IActionNodePool nodePool) {
            var plan = pool.TryPop(out var p) ? p : new ActionPlan(this);
            plan.Reinitialize(nodePool);
            return plan;
        }

        void IActionPlanPool.Return(ActionPlan plan) => pool.Push(plan);
    }
}
