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

        private IReadOnlyActionIndex actionIndex = null!;
        private IActionNodePool nodePool = null!;

        internal ActionGraph(IActionGraphPool graphPool) {
            this.graphPool = graphPool;
        }

        /// <summary>
        /// Reinitializes this graph for a new planning pass. Called by <see cref="IActionGraphPool.Rent"/>.
        /// </summary>
        internal void Reinitialize(IReadOnlyActionIndex index, IActionNodePool nodePool) {
            actionIndex = index;
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
        /// Gets the neighbors for a node by iterating all action templates
        /// and checking <see cref="Action.IsPossible"/> per permutation.
        /// No indexing, no per-node action sets — just raw iteration every hop.
        /// </summary>
        internal IEnumerable<ActionNode> Neighbors(ActionNode node, IReadOnlyState baseState) {
            foreach (var template in actionIndex) {
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
