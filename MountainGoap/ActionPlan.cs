// <copyright file="ActionPlan.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Poolable wrapper for an ordered sequence of actions produced by planning.
    /// Owned by <see cref="Planner"/> during planning (returned to pool if not the best plan),
    /// then transferred to the agent and executed. Exposed publicly as <see cref="IActionPlan"/>;
    /// dispose/mutation methods are internal to prevent misuse from user callbacks.
    /// </summary>
    public class ActionPlan : IActionPlan {
        private readonly IActionPlanPool planPool;
        private IActionNodePool nodePool = null!;

        /// <summary>
        /// Ordered list of actions to execute. Internal — external callers use the
        /// <see cref="IActionPlan.Steps"/> read-only view.
        /// </summary>
        internal readonly List<ExecutingAction> Steps = new();

        /// <inheritdoc/>
        IReadOnlyList<ExecutingAction> IActionPlan.Steps => Steps;

        internal ActionPlan(IActionPlanPool planPool) {
            this.planPool = planPool;
        }

        /// <summary>
        /// Reinitializes this plan for a new use. Called by the pool before renting.
        /// </summary>
        internal void Reinitialize(IActionNodePool nodePool) {
            this.nodePool = nodePool;
            Steps.Clear();
        }

        /// <summary>
        /// Returns the action at <paramref name="index"/> to the node pool and removes it from Steps.
        /// </summary>
        internal void ReturnStep(int index) {
            nodePool.ReturnAction(Steps[index]);
            Steps.RemoveAt(index);
        }

        /// <summary>
        /// Returns all remaining actions to the node pool, clears Steps, and returns this plan
        /// to the plan pool for reuse.
        /// </summary>
        internal void Dispose() {
            foreach (var action in Steps) nodePool.ReturnAction(action);
            Steps.Clear();
            planPool.Return(this);
        }
    }
}
