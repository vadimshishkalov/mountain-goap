// <copyright file="ActionGraph.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Represents a traversable action graph.
    /// </summary>
    internal class ActionGraph {
        /// <summary>
        /// The set of action template nodes for the graph.
        /// </summary>
        internal List<ActionNode> ActionNodes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionGraph"/> class.
        /// </summary>
        /// <param name="actions">List of action templates to include in the graph.</param>
        /// <param name="baseState">Shared base state snapshot for this planning pass.</param>
        internal ActionGraph(List<Action> actions, IPlanningBaseState baseState) {
            foreach (var template in actions) {
                var permutations = template.GetPermutations(baseState);
                foreach (var permutation in permutations) ActionNodes.Add(new(new ExecutingAction(template, permutation), baseState.Snapshot()));
            }
        }

        /// <summary>
        /// Gets the list of neighbors for a node.
        /// </summary>
        /// <param name="node">Node for which to retrieve neighbors.</param>
        /// <returns>The set of action/state combinations that can be executed after the current action/state combination.</returns>
        internal IEnumerable<ActionNode> Neighbors(ActionNode node) {
            foreach (var templateNode in ActionNodes) {
                if (templateNode.Action is not null && templateNode.Action.IsPossible(node.State)) {
                    var newState = node.State.Snapshot();
                    var newAction = new ExecutingAction(templateNode.Action.Template, templateNode.Action.parameters.Copy());
                    var newNode = new ActionNode(newAction, newState);
                    newNode.Action?.ApplyEffects(newNode.State);
                    yield return newNode;
                }
            }
        }
    }
}
