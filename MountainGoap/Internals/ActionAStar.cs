// <copyright file="ActionAStar.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System;
    using System.Collections.Generic;
    using Priority_Queue;

    /// <summary>
    /// Per-agent A* search over an action graph. This object is long-lived and owned by the
    /// agent's <see cref="Planner"/>. Call <see cref="Search"/> to run a planning pass; internal
    /// data structures are cleared and reused between passes. All lifecycle (node/action pooling)
    /// is delegated to the <see cref="ActionGraph"/> passed to <see cref="Search"/>.
    /// </summary>
    internal class ActionAStar {
        private readonly FastPriorityQueue<ActionNode> frontier = new(100000);
        private readonly Dictionary<ActionNode, float> costSoFar = new();
        private readonly Dictionary<ActionNode, int> stepsSoFar = new();
        private readonly Dictionary<ActionNode, ActionNode> cameFrom = new();
        private BaseGoal? currentGoal;

        /// <summary>
        /// Ordered list of actions in the best plan found by the last <see cref="Search"/> call.
        /// Empty if no plan was found. Valid until the next <see cref="Search"/> call.
        /// </summary>
        internal readonly List<ExecutingAction> Path = new();

        /// <summary>
        /// Cost of the plan in <see cref="Path"/>. Zero if no plan was found.
        /// </summary>
        internal float FinalCost { get; private set; }

        /// <summary>
        /// Runs A* search from <paramref name="start"/> toward <paramref name="goal"/>.
        /// On return, <see cref="Path"/> contains the action sequence (empty if unreachable)
        /// and <see cref="FinalCost"/> holds the total plan cost. Node lifecycle is owned by
        /// <paramref name="graph"/> — this method does not return any nodes to pools.
        /// </summary>
        internal void Search(ActionNode start, BaseGoal goal, ActionGraph graph,
                             IReadOnlyState baseState, float costMaximum, int stepMaximum) {
            Path.Clear();
            FinalCost = 0;
            currentGoal = goal;
            ActionNode? finalPoint = null;

            frontier.Clear();
            costSoFar.Clear();
            stepsSoFar.Clear();
            cameFrom.Clear();

            frontier.Enqueue(start, 0);
            cameFrom[start] = start;
            costSoFar[start] = 0;
            stepsSoFar[start] = 0;

            while (frontier.Count > 0) {
                var current = frontier.Dequeue();
                if (MeetsGoal(current, start)) {
                    finalPoint = current;
                    break;
                }
                foreach (var next in graph.Neighbors(current, baseState)) {
                    float newCost = costSoFar[current] + next.Cost(current.State);
                    int newStepCount = stepsSoFar[current] + 1;
                    if (newCost > costMaximum || newStepCount > stepMaximum) continue;
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next]) {
                        costSoFar[next] = newCost;
                        stepsSoFar[next] = newStepCount;
                        float priority = newCost + Heuristic(next, goal, current);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                        Agent.TriggerOnEvaluatedActionNode(next, cameFrom);
                    }
                }
            }

            if (finalPoint != null) {
                FinalCost = costSoFar[finalPoint];
                BuildPath(finalPoint);
            }

            costSoFar.Clear();
            stepsSoFar.Clear();
            cameFrom.Clear();
        }

        private void BuildPath(ActionNode finalPoint) {
            var cursor = finalPoint;
            while (cursor != null && cursor.Action != null && cameFrom.ContainsKey(cursor)) {
                Path.Add(cursor.Action);
                var next = cameFrom[cursor]; // capture before mutating hash
                cursor.Action = null;        // graph.Dispose() will skip null actions
                cursor = next;
            }
            Path.Reverse();
        }

        private static float Heuristic(ActionNode actionNode, BaseGoal goal, ActionNode current) {
            var cost = 0f;
            if (goal is Goal normalGoal) {
                normalGoal.DesiredState.Select(kvp => kvp.Key).ToList().ForEach(key => {
                    if (!actionNode.State.ContainsKey(key)) cost++;
                    else if (actionNode.State[key] == null && actionNode.State[key] != normalGoal.DesiredState[key]) cost++;
                    else if (actionNode.State[key] is object obj && !obj.Equals(normalGoal.DesiredState[key])) cost++;
                });
            }
            else if (goal is ExtremeGoal extremeGoal) {
                foreach (var kvp in extremeGoal.DesiredState) {
                    var valueDiff = 0f;
                    var valueDiffMultiplier = (actionNode?.Action?.StateCostDeltaMultiplier ?? Action.DefaultStateCostDeltaMultiplier).Invoke(actionNode?.Action, kvp.Key);
                    if (actionNode.State.ContainsKey(kvp.Key) && actionNode.State[kvp.Key] == null) {
                        cost += float.PositiveInfinity;
                        continue;
                    }
                    if (actionNode.State.ContainsKey(kvp.Key) && extremeGoal.DesiredState.ContainsKey(kvp.Key)) valueDiff = Convert.ToSingle(actionNode.State[kvp.Key]) - Convert.ToSingle(current.State[kvp.Key]);
                    if (!actionNode.State.ContainsKey(kvp.Key)) cost += float.PositiveInfinity;
                    else if (!current.State.ContainsKey(kvp.Key)) cost += float.PositiveInfinity;
                    else if (!kvp.Value && actionNode.State[kvp.Key] is object a && current.State[kvp.Key] is object b && IsLowerThanOrEquals(a, b)) cost += valueDiff * valueDiffMultiplier;
                    else if (kvp.Value && actionNode.State[kvp.Key] is object a2 && current.State[kvp.Key] is object b2 && IsHigherThanOrEquals(a2, b2)) cost -= valueDiff * valueDiffMultiplier;
                }
            }
            else if (goal is ComparativeGoal comparativeGoal) {
                foreach (var kvp in comparativeGoal.DesiredState) {
                    var valueDiff2 = 0f;
                    var valueDiffMultiplier = (actionNode?.Action?.StateCostDeltaMultiplier ?? Action.DefaultStateCostDeltaMultiplier).Invoke(actionNode?.Action, kvp.Key);
                    if (actionNode.State.ContainsKey(kvp.Key) && comparativeGoal.DesiredState.ContainsKey(kvp.Key)) valueDiff2 = Math.Abs(Convert.ToSingle(actionNode.State[kvp.Key]) - Convert.ToSingle(current.State[kvp.Key]));
                    if (!actionNode.State.ContainsKey(kvp.Key)) cost += float.PositiveInfinity;
                    else if (!current.State.ContainsKey(kvp.Key)) cost += float.PositiveInfinity;
                    else if (kvp.Value.Operator == ComparisonOperator.Undefined) cost += float.PositiveInfinity;
                    else if (kvp.Value.Operator == ComparisonOperator.Equals && actionNode.State[kvp.Key] is object obj && !obj.Equals(comparativeGoal.DesiredState[kvp.Key].Value)) cost += valueDiff2 * valueDiffMultiplier;
                    else if (kvp.Value.Operator == ComparisonOperator.LessThan && actionNode.State[kvp.Key] is object a && comparativeGoal.DesiredState[kvp.Key].Value is object b && !IsLowerThan(a, b)) cost += valueDiff2 * valueDiffMultiplier;
                    else if (kvp.Value.Operator == ComparisonOperator.GreaterThan && actionNode.State[kvp.Key] is object a2 && comparativeGoal.DesiredState[kvp.Key].Value is object b2 && !IsHigherThan(a2, b2)) cost += valueDiff2 * valueDiffMultiplier;
                    else if (kvp.Value.Operator == ComparisonOperator.LessThanOrEquals && actionNode.State[kvp.Key] is object a3 && comparativeGoal.DesiredState[kvp.Key].Value is object b3 && !IsLowerThanOrEquals(a3, b3)) cost += valueDiff2 * valueDiffMultiplier;
                    else if (kvp.Value.Operator == ComparisonOperator.GreaterThanOrEquals && actionNode.State[kvp.Key] is object a4 && comparativeGoal.DesiredState[kvp.Key].Value is object b4 && !IsHigherThanOrEquals(a4, b4)) cost += valueDiff2 * valueDiffMultiplier;
                }
            }
            return cost;
        }

        private static bool IsLowerThan(object a, object b) => Utils.IsLowerThan(a, b);
        private static bool IsHigherThan(object a, object b) => Utils.IsHigherThan(a, b);
        private static bool IsLowerThanOrEquals(object a, object b) => Utils.IsLowerThanOrEquals(a, b);
        private static bool IsHigherThanOrEquals(object a, object b) => Utils.IsHigherThanOrEquals(a, b);
        private bool MeetsGoal(ActionNode actionNode, ActionNode current) => Utils.MeetsGoal(currentGoal!, actionNode, current);
    }
}
