// <copyright file="Planner.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Per-agent planner. Owns a persistent <see cref="ActionAStar"/>,
    /// <see cref="ActionGraphPool"/>, and <see cref="ActionPlanPool"/> that are reused across
    /// planning passes. A fresh <see cref="ActionGraph"/> and one <see cref="ActionPlan"/> per
    /// goal are rented for each pass and disposed at the end.
    /// </summary>
    internal class Planner {
        private readonly ActionAStar astar = new();
        private readonly IActionGraphPool graphPool;
        private readonly IActionPlanPool planPool;
        private readonly IActionNodePool nodePool;
        private readonly IReadOnlyActionIndex actions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Planner"/> class.
        /// </summary>
        /// <param name="actions">Indexed action collection for the owning agent.</param>
        /// <param name="poolManager">Shared pool manager. Null pools fall back to per-instance creation.</param>
        internal Planner(IReadOnlyActionIndex actions, PoolManager? poolManager) {
            this.actions = actions;
            nodePool = poolManager?.NodePool ?? new ActionNodePool();
            graphPool = poolManager?.GraphPool ?? new ActionGraphPool();
            planPool = poolManager?.PlanPool ?? new ActionPlanPool();
        }

        /// <summary>
        /// Makes a plan to achieve the agent's goals. Reads behavioural limits from
        /// <see cref="Agent.Configuration"/>.
        /// </summary>
        /// <param name="agent">Agent using the planner.</param>
        internal void Plan(Agent agent) {
            var config = agent.Configuration;
            Agent.TriggerOnPlanningStarted(agent);
            float bestPlanUtility = 0;
            ActionPlan? bestPlan = null;
            IReadOnlyGoal? bestGoal = null;
            var baseState = agent.State.Snapshot();
            var graph = graphPool.Rent(actions, nodePool, config.NeighborLookupMode);

            foreach (var goal in agent.Goals) {
                Agent.TriggerOnPlanningStartedForSingleGoal(agent, goal);
                var plan = planPool.Rent(nodePool);
                ActionNode start = graph.RentNode(null, baseState.Snapshot());
                astar.Search(start, goal, graph, plan, baseState, config.CostMaximum, config.StepMaximum);
                float cost = astar.FinalCost;
                float utility = (plan.Steps.Count > 0 && cost > 0) ? goal.Weight / cost : 0;
                if (plan.Steps.Count > 0 && cost == 0) Agent.TriggerOnPlanningFinishedForSingleGoal(agent, goal, 0);
                else Agent.TriggerOnPlanningFinishedForSingleGoal(agent, goal, utility);
                if (plan.Steps.Count > 0 && utility > bestPlanUtility) {
                    bestPlan?.Dispose(); // return previous best to pool
                    bestPlanUtility = utility;
                    bestGoal = goal;
                    bestPlan = plan;
                }
                else {
                    plan.Dispose(); // return unchosen plan to pool
                }
            }

            graph.Dispose();
            baseState.Dispose();

            if (bestPlan != null && bestGoal != null) {
                agent.AddActionSequence(bestPlan);
                Agent.TriggerOnPlanUpdated(agent, bestPlan);
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
