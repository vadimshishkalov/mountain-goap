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

        // Reusable temp buffers inside Neighbors()/ExpandTemplate()/BuildNeighbor().
        private readonly HashSet<Action> passedTemp = new();
        private readonly HashSet<Action> failedTemp = new();
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
        /// Gets the neighbors for a node. Candidates are checked via
        /// <see cref="Action.IsPossible"/> and promoted or discarded.
        /// Possible templates are trusted and expanded directly.
        /// Delta invalidates Possible templates back into Candidates.
        /// </summary>
        internal IEnumerable<ActionNode> Neighbors(ActionNode node, IReadOnlyState baseState) {
            // Seed candidates for the start node.
            if (node.Candidates.Count == 0 && node.Possible.Count == 0) {
                actionIndex.GetCandidates(baseState.Keys, node.Candidates);
            }

            passedTemp.Clear();
            failedTemp.Clear();

            // Candidates: check IsPossible, promote to Possible or discard.
            foreach (var template in node.Candidates) {
                foreach (var neighbor in ExpandCandidate(template, node, baseState))
                    yield return neighbor;
            }

            // Possible: trusted, expand all permutations directly.
            foreach (var template in node.Possible) {
                foreach (var neighbor in ExpandPossible(template, node, baseState))
                    yield return neighbor;
            }

            passedTemp.Clear();
            failedTemp.Clear();
        }

        private IEnumerable<ActionNode> ExpandCandidate(Action template, ActionNode node, IReadOnlyState baseState) {
            foreach (var parameters in template.GetPermutations(baseState)) {
                var action = nodePool.RentAction(template, parameters);
                if (action.IsPossible(node.State)) {
                    passedTemp.Add(template);
                    yield return BuildNeighbor(action, template, node, baseState);
                }
                else {
                    nodePool.ReturnAction(action);
                }
            }
            if (!passedTemp.Contains(template)) failedTemp.Add(template);
        }

        private IEnumerable<ActionNode> ExpandPossible(Action template, ActionNode node, IReadOnlyState baseState) {
            foreach (var parameters in template.GetPermutations(baseState)) {
                var action = nodePool.RentAction(template, parameters);
                if (template.HasStateChecker && !action.IsPossible(node.State)) {
                    nodePool.ReturnAction(action);
                    continue;
                }
                yield return BuildNeighbor(action, template, node, baseState);
            }
        }

        private ActionNode BuildNeighbor(ExecutingAction action, Action template, ActionNode node, IReadOnlyState baseState) {
            var newState = node.State.Snapshot();
            var newNode = RentNode(action, newState);
            newNode.Action?.ApplyEffects(newNode.State);

            // Child.Possible = parent's Possible + confirmed from Candidates.
            newNode.Possible.UnionWith(node.Possible);
            newNode.Possible.UnionWith(passedTemp);

            // Child.Candidates = unchecked parent Candidates.
            newNode.Candidates.UnionWith(node.Candidates);
            newNode.Candidates.ExceptWith(passedTemp);
            newNode.Candidates.ExceptWith(failedTemp);

            // Delta: get affected templates, invalidate Possible → Candidates.
            if (template.HasStateMutator) {
                actionIndex.GetCandidates(baseState.Keys, deltaSetTemp);
            }
            else {
                actionIndex.GetCandidates(template.PostconditionKeys, deltaSetTemp);
            }

            foreach (var delta in deltaSetTemp) {
                newNode.Possible.Remove(delta);
                newNode.Candidates.Add(delta);
            }
            deltaSetTemp.Clear();

            return newNode;
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
