// <copyright file="IReadOnlyActionNode.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Read-only view of a planning action node, exposed via <see cref="EvaluatedActionNodeEvent"/>.
    /// </summary>
    public interface IReadOnlyActionNode {
        /// <summary>Gets the action associated with this node, or null for the start node.</summary>
        IReadOnlyAction? Action { get; }

        /// <summary>Gets a read-only view of the planning state at this node.</summary>
        IReadOnlyState State { get; }
    }
}
