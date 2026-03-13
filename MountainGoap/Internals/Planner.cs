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
        private readonly IActionGraphPool graphPool = new ActionGraphPool();
        private readonly IActionPlanPool planPool = new ActionPlanPool();
        private readonly IActionNodePool nodePool;
        private readonly ActionCollection actions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Planner"/> class.
        /// </summary>
        /// <param name="actions">Indexed action collection for the owning agent.</param>
        /// <param name="nodePool">Pool for ActionNode and ExecutingAction objects.</param>
        internal Planner(ActionCollection actions, IActionNodePool nodePool) {
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
            ActionPlan? bestPlan = null;
            BaseGoal? bestGoal = null;
            var baseState = agent.State.Snapshot();
            var graph = graphPool.Rent(actions, nodePool);

            foreach (var goal in agent.Goals) {
                Agent.TriggerOnPlanningStartedForSingleGoal(agent, goal);
                var plan = planPool.Rent(nodePool);
                ActionNode start = graph.RentNode(null, baseState.Snapshot());
                astar.Search(start, goal, graph, plan, baseState, costMaximum, stepMaximum);
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
