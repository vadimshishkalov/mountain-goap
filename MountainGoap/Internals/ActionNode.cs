// <copyright file="ActionNode.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;
    using Priority_Queue;

    /// <summary>
    /// Represents an action node in an action graph.
    /// </summary>
    internal class ActionNode : FastPriorityQueueNode, IReadOnlyActionNode {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionNode"/> class.
        /// </summary>
        /// <param name="action">Action to be assigned to the node.</param>
        /// <param name="state">Planning state to be assigned to the node.</param>
        internal ActionNode(ExecutingAction? action, IPlanningStepState state) {
            Action = action;
            State = state;
        }

        /// <summary>
        /// Gets or sets the planning state of the world for this action node.
        /// </summary>
        internal IPlanningStepState State { get; set; }

        /// <summary>
        /// Gets or sets the action to be executed when the world is in the defined <see cref="State"/>.
        /// </summary>
        internal ExecutingAction? Action { get; set; }

        /// <inheritdoc/>
        IReadOnlyAction? IReadOnlyActionNode.Action => Action;

        /// <inheritdoc/>
        IReadOnlyState IReadOnlyActionNode.State => State;

        /// <summary>
        /// Templates confirmed possible by this node or ancestors.
        /// Used by <see cref="NeighborLookupMode.Aggressive"/>.
        /// </summary>
        internal HashSet<MountainGoap.Action> Possible { get; } = new();

        /// <summary>
        /// Templates not yet checked for this node's state. Promoted to
        /// <see cref="Possible"/> or discarded during expansion.
        /// Used by <see cref="NeighborLookupMode.Aggressive"/>.
        /// </summary>
        internal HashSet<MountainGoap.Action> Candidates { get; } = new();

        /// <summary>
        /// Cost to traverse this node.
        /// </summary>
        /// <param name="currentState">Current state after previous node is executed.</param>
        /// <returns>The cost of the action to be executed.</returns>
        internal float Cost(IReadOnlyState currentState) {
            if (Action == null) return float.MaxValue;
            return Action.GetCost(currentState);
        }
    }
}
