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

        // Filled once per planning pass from base state keys; reused across nodes.
        private readonly HashSet<Action> baseCandidates = new();

        // Union buffer for nodes that have a delta; cleared and refilled per such node.
        // Safe to reuse because ActionAStar.Search drives only one Neighbors() generator at a time.
        private readonly HashSet<Action> candidateBuffer = new();

        private IReadOnlyActionIndex actionIndex = null!;
        private IActionNodePool nodePool = null!;
        private bool baseFilled;

        internal ActionGraph(IActionGraphPool graphPool) {
            this.graphPool = graphPool;
        }

        /// <summary>
        /// Reinitializes this graph for a new planning pass. Called by <see cref="IActionGraphPool.Rent"/>.
        /// </summary>
        internal void Reinitialize(IReadOnlyActionIndex index, IActionNodePool nodePool) {
            actionIndex = index;
            this.nodePool = nodePool;
            baseCandidates.Clear();
            baseFilled = false;
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
        /// Gets the list of neighbors for a node. Candidates are narrowed via the precondition
        /// index: base-state candidates are filled into a reusable HashSet once per planning pass;
        /// delta candidates are added to a second reusable buffer per delta node. No collections
        /// are allocated in the hot path.
        /// </summary>
        internal IEnumerable<ActionNode> Neighbors(ActionNode node, IReadOnlyState baseState) {
            if (!baseFilled) {
                actionIndex.GetCandidates(baseState.Keys, baseCandidates);
                baseFilled = true;
            }

            HashSet<Action> candidates;
            if (node.State is PlanningNodeState ps && ps.HasDelta) {
                candidateBuffer.Clear();
                candidateBuffer.UnionWith(baseCandidates);
                actionIndex.GetCandidates(ps.DeltaKeys, candidateBuffer);
                candidates = candidateBuffer;
            }
            else {
                candidates = baseCandidates;
            }

            foreach (var template in candidates) {
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
