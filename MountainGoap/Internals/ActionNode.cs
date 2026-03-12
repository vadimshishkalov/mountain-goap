// <copyright file="ActionNode.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;
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

#pragma warning disable S3875 // "operator==" should not be overloaded on reference types
        /// <summary>
        /// Overrides the equality operator on ActionNodes.
        /// </summary>
        /// <param name="node1">First node to be compared.</param>
        /// <param name="node2">Second node to be compared.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public static bool operator ==(ActionNode? node1, ActionNode? node2) {
            if (node1 is null) return node2 is null;
            if (node2 is null) return node1 is null;
            if (node1.Action == null || node2.Action == null) return (node1.Action == node2.Action) && node1.StateMatches(node2);
            return node1.Action.Equals(node2.Action) && node1.StateMatches(node2);
        }
#pragma warning restore S3875 // "operator==" should not be overloaded on reference types

        /// <summary>
        /// Overrides the inequality operator on ActionNodes.
        /// </summary>
        /// <param name="node1">First node to be compared.</param>
        /// <param name="node2">Second node to be compared.</param>
        /// <returns>True if unequal, otherwise false.</returns>
        public static bool operator !=(ActionNode? node1, ActionNode? node2) {
            if (node1 is null) return node2 is not null;
            if (node2 is null) return node1 is not null;
            if (node1.Action is not null) return !node1.Action.Equals(node2.Action) || !node1.StateMatches(node2);
            return node2.Action is null;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) {
            if (obj is not ActionNode item) return false;
            return this == item;
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            var hashCode = 629302477;
            if (Action != null) hashCode = (hashCode * -1521134295) + EqualityComparer<ExecutingAction>.Default.GetHashCode(Action);
            else hashCode *= -1521134295;
            hashCode = (hashCode * -1521134295) + EqualityComparer<IPlanningStepState>.Default.GetHashCode(State);
            return hashCode;
        }

        /// <summary>
        /// Cost to traverse this node.
        /// </summary>
        /// <param name="currentState">Current state after previous node is executed.</param>
        /// <returns>The cost of the action to be executed.</returns>
        internal float Cost(IReadOnlyState currentState) {
            if (Action == null) return float.MaxValue;
            return Action.GetCost(currentState);
        }

        private bool StateMatches(ActionNode otherNode) {
            foreach (var key in State.Keys) {
                if (!otherNode.State.ContainsKey(key)) return false;
                var val = State[key];
                var otherVal = otherNode.State[key];
                if (val == null && val != otherVal) return false;
                if (val == null && val == otherVal) continue;
                if (val is object obj && !obj.Equals(otherVal)) return false;
            }
            foreach (var key in otherNode.State.Keys) {
                if (!State.ContainsKey(key)) return false;
                var val = otherNode.State[key];
                var thisVal = State[key];
                if (val == null && val != thisVal) return false;
                if (val == null && val == thisVal) continue;
                if (val is object obj && !obj.Equals(thisVal)) return false;
            }
            return true;
        }
    }
}
