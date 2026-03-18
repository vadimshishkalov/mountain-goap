// <copyright file="IReadOnlyGoal.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Read-only view of a goal, exposed via planning event callbacks.
    /// </summary>
    public interface IReadOnlyGoal {
        /// <summary>Gets the name of the goal.</summary>
        string Name { get; }

        /// <summary>Gets the relative weight (priority) of the goal.</summary>
        float Weight { get; }
    }
}
