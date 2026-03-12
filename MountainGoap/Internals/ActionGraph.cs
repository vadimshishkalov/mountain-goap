// <copyright file="ActionGraph.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Per-planning-pass scope object. Owns all <see cref="ActionNode"/> and
    /// <see cref="ExecutingAction"/> objects created during the pass.
    /// Call <see cref="Dispose"/> when the pass completes to cascade-return all rented
    /// objects to their pools and return this graph to the <see cref="IActionGraphPool"/>.
    /// </summary>
    internal class ActionGraph : IDisposable {
        private readonly IActionGraphPool graphPool;
        private readonly List<ActionNode> rentedNodes = new();
        private List<Action> actions = new();
        private IActionNodePool nodePool = null!;

        internal ActionGraph(IActionGraphPool graphPool) {
            this.graphPool = graphPool;
        }

        /// <summary>
        /// Reinitializes this graph for a new planning pass. Called by <see cref="IActionGraphPool.Rent"/>.
        /// </summary>
        internal void Reinitialize(List<Action> actions, IActionNodePool nodePool) {
            this.actions = actions;
            this.nodePool = nodePool;
        }

        /// <summary>
        /// Rents a node from the pool, registers it with this graph, and returns it.
        /// </summary>
        internal ActionNode RentNode(ExecutingAction? action, IPlanningStepState state) {
            var node = nodePool.RentNode(action, state);
            rentedNodes.Add(node);
            return node;
        }

        /// <summary>
        /// Gets the list of neighbors for a node. Permutations are generated from
        /// <paramref name="baseState"/> (consistent across the whole planning pass);
        /// availability is checked against <paramref name="node"/>'s full layered state.
        /// Each yielded node is tracked by this graph and returned on <see cref="Dispose"/>.
        /// </summary>
        internal IEnumerable<ActionNode> Neighbors(ActionNode node, IReadOnlyState baseState) {
            foreach (var template in actions) {
                foreach (var parameters in template.GetPermutations(baseState)) {
                    var action = nodePool.RentAction(template, parameters);
                    if (action.IsPossible(node.State)) {
                        var newState = node.State.Snapshot();
                        var newNode = RentNode(action, newState);
                        newNode.Action?.ApplyEffects(newNode.State);
                        yield return newNode;
                    }
                    else {
                        nodePool.ReturnAction(action);
                    }
                }
            }
        }

        /// <summary>
        /// Returns all rented nodes and their non-null actions to the node pool,
        /// then returns this graph to the graph pool.
        /// </summary>
        public void Dispose() {
            foreach (var node in rentedNodes) {
                if (node.Action != null) nodePool.ReturnAction(node.Action);
                nodePool.ReturnNode(node);
            }
            rentedNodes.Clear();
            graphPool.Return(this);
        }
    }
}
