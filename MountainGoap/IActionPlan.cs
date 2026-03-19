// <copyright file="IActionPlan.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Read-only view of a planned action sequence. Passed to public events and exposed via
    /// <see cref="Agent.CurrentActionSequences"/> so callers can inspect the plan without
    /// being able to dispose or mutate it.
    /// </summary>
    public interface IActionPlan {
        /// <summary>
        /// Ordered list of actions to be executed.
        /// </summary>
        IReadOnlyList<IReadOnlyAction> Steps { get; }
    }
}
