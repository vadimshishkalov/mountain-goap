// <copyright file="IPlanningBaseState.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Internal interface for the immutable base-layer snapshot taken at the start of each planning pass.
    /// Provides a <see cref="Snapshot"/> factory that produces the first planning node state,
    /// and a <see cref="Return"/> method to release the snapshot back to the pool when the pass is done.
    /// </summary>
    internal interface IPlanningBaseState : IReadOnlyState, IDisposable {
        /// <summary>
        /// Creates the first mutable planning node state rooted at this base snapshot.
        /// Registers the child so it is automatically disposed when this base state is disposed.
        /// </summary>
        IPlanningStepState Snapshot();
    }
}
