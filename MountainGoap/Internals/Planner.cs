// <copyright file="Planner.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Per-agent planner. Owns a persistent <see cref="ActionAStar"/> and
    /// <see cref="ActionGraphPool"/> that are reused across planning passes.
    /// A fresh <see cref="ActionGraph"/> is rented from the pool for each pass and
    /// disposed at the end, cascading all node and action returns.
    /// </summary>
    internal class Planner {
        private readonly ActionAStar astar = new();
        private readonly IActionGraphPool graphPool = new ActionGraphPool();
        private readonly IActionNodePool nodePool;
        private readonly List<Action> actions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Planner"/> class.
        /// </summary>
        /// <param name="actions">Immutable list of action templates for the owning agent.</param>
        /// <param name="nodePool">Pool for ActionNode and ExecutingAction objects.</param>
        internal Planner(List<Action> actions, IActionNodePool nodePool) {
            this.actions = actions;
            this.nodePool = nodePool;
        }

        /// <summary>
        /// Makes a plan to achieve the agent's goals.
        /// </summary>
        /// <param name="agent">Agent using the planner.</param>
        /// <param name="costMaximum">Maximum allowable cost for a plan.</param>
        /// <param name="stepMaximum">Maximum allowable steps for a plan.</param>
        internal void Plan(Agent agent, float costMaximum, int stepMaximum) {
            Agent.TriggerOnPlanningStarted(agent);
            float bestPlanUtility = 0;
            List<ExecutingAction>? bestPath = null;
            BaseGoal? bestGoal = null;
            var baseState = agent.State.Snapshot();
            var graph = graphPool.Rent(actions, nodePool);

            foreach (var goal in agent.Goals) {
                Agent.TriggerOnPlanningStartedForSingleGoal(agent, goal);
                ActionNode start = graph.RentNode(null, baseState.Snapshot());
                astar.Search(start, goal, graph, baseState, costMaximum, stepMaximum);
                float cost = astar.FinalCost;
                float utility = (astar.Path.Count > 0 && cost > 0) ? goal.Weight / cost : 0;
                if (astar.Path.Count > 0 && cost == 0) Agent.TriggerOnPlanningFinishedForSingleGoal(agent, goal, 0);
                else Agent.TriggerOnPlanningFinishedForSingleGoal(agent, goal, utility);
                if (astar.Path.Count > 0 && utility > bestPlanUtility) {
                    bestPlanUtility = utility;
                    bestGoal = goal;
                    bestPath = new List<ExecutingAction>(astar.Path);
                }
            }

            graph.Dispose();
            baseState.Dispose();

            if (bestPath != null && bestGoal != null) {
                agent.CurrentActionSequences.Add(bestPath);
                Agent.TriggerOnPlanUpdated(agent, bestPath);
                agent.IsBusy = true;
                Agent.TriggerOnPlanningFinished(agent, bestGoal, bestPlanUtility);
            }
            else {
                Agent.TriggerOnPlanningFinished(agent, null, 0);
            }

            agent.IsPlanning = false;
        }
    }
}
