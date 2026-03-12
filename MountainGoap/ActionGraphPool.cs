// <copyright file="ActionGraphPool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Stack-backed pool for <see cref="ActionGraph"/> instances.
    /// </summary>
    internal class ActionGraphPool : IActionGraphPool {
        private readonly Stack<ActionGraph> pool = new();

        ActionGraph IActionGraphPool.Rent(List<Action> actions, IActionNodePool nodePool) {
            var graph = pool.TryPop(out var g) ? g : new ActionGraph(this);
            graph.Reinitialize(actions, nodePool);
            return graph;
        }

        void IActionGraphPool.Return(ActionGraph graph) => pool.Push(graph);
    }
}
