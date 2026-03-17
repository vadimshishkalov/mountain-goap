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

        // Reusable temp buffer for index lookups inside Neighbors(). Cleared before each use
        // and fully consumed before the next yield return — safe to reuse in a generator.
        private readonly HashSet<Action> deltaSetTemp = new();

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
        /// Gets the neighbors for a node. For the start node (empty AvailableActions),
        /// all index candidates are loaded directly — <see cref="Action.IsPossible"/>
        /// handles filtering per permutation (it already includes static precondition checks).
        /// Child nodes inherit the parent's available set and update only the entries whose
        /// precondition keys overlap with the applied action's postcondition keys.
        /// </summary>
        internal IEnumerable<ActionNode> Neighbors(ActionNode node, IReadOnlyState baseState) {
            // Seed AvailableActions for the start node (empty signals unseeded).
            // No MightBePossible pre-filter — IsPossible already covers static preconditions.
            if (node.AvailableActions.Count == 0) {
                actionIndex.GetCandidates(baseState.Keys, node.AvailableActions);
            }

            foreach (var template in node.AvailableActions) {
                foreach (var permutation in template.GetPermutations(baseState)) {
                    var action = nodePool.RentAction(template, permutation);
                    if (action.IsPossible(node.State)) {
                        var newState = node.State.Snapshot();
                        var newNode = RentNode(action, newState);
                        newNode.Action?.ApplyEffects(newNode.State);

                        // Child inherits parent's available set then applies delta updates.
                        newNode.AvailableActions.UnionWith(node.AvailableActions);

                        if (template.HasStateMutator) {
                            // stateMutator writes unknown keys — re-evaluate all base candidates.
                            actionIndex.GetCandidates(baseState.Keys, deltaSetTemp);
                        }
                        else {
                            actionIndex.GetCandidates(template.PostconditionKeys, deltaSetTemp);
                        }

                        newNode.AvailableActions.UnionWith(deltaSetTemp);
                        deltaSetTemp.Clear();

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
