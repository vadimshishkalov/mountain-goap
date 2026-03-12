// <copyright file="IPlanningStepState.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// State view used during planning. Writes go to a per-node delta layer; the shared base is never modified.
    /// Snapshot() creates a child node state by copying the delta and sharing the base reference.
    /// </summary>
    public interface IPlanningStepState : IState {
        /// <summary>
        /// Creates a new planning node state that inherits the current state as its starting point.
        /// Only the delta layer is copied; the base layer is shared by reference.
        /// </summary>
        IPlanningStepState Snapshot();

        /// <summary>
        /// Returns this state to the pool for reuse. The default no-op is safe for custom implementations
        /// that do not use a pool.
        /// </summary>
        void Return() { }
    }
}
