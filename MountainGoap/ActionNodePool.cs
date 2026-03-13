// <copyright file="ActionNodePool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Thread-safe pool for ActionNode and ExecutingAction objects created during A* planning.
    /// A single instance may be shared across agents so that agents with the same action set
    /// draw from the same object reservoir. If no pool is provided to a Planner, a new instance
    /// is created automatically.
    /// </summary>
    public class ActionNodePool : IActionNodePool {
        private readonly ConcurrentStack<ActionNode> nodes = new();
        private readonly ConcurrentStack<ExecutingAction> actions = new();

        ActionNode IActionNodePool.RentNode(ExecutingAction? action, IPlanningStepState state) {
            if (nodes.TryPop(out var node)) {
                node.Action = action;
                node.State = state;
                return node;
            }
            return new ActionNode(action, state);
        }

        void IActionNodePool.ReturnNode(ActionNode node) {
            node.Action = null;
            node.Possible.Clear();
            node.Candidates.Clear();
            nodes.Push(node);
        }

        ExecutingAction IActionNodePool.RentAction(Action template, Dictionary<string, object?> parameters) {
            if (actions.TryPop(out var ea)) {
                ea.Reinitialize(template, parameters);
                return ea;
            }
            return new ExecutingAction(template, parameters.Copy());
        }

        void IActionNodePool.ReturnAction(ExecutingAction action) => actions.Push(action);
    }
}
