// <copyright file="ActionNode.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using Priority_Queue;

    /// <summary>
    /// Represents an action node in an action graph.
    /// </summary>
    public class ActionNode : FastPriorityQueueNode {
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
        public IPlanningStepState State { get; set; }

        /// <summary>
        /// Gets or sets the action to be executed when the world is in the defined <see cref="State"/>.
        /// </summary>
        public ExecutingAction? Action { get; set; }

        /// <summary>
        /// Set of action templates that pass <see cref="Action.MightBePossible"/> for this node's
        /// state. Permanent field — lives with the node and is cleared when the node is returned
        /// to <see cref="ActionNodePool"/>. Empty on a freshly rented node (signals unseeded).
        /// Populated during <see cref="ActionGraph.Neighbors"/> of the parent node.
        /// </summary>
        internal HashSet<MountainGoap.Action> AvailableActions { get; } = new();

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
