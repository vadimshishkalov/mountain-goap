// <copyright file="ActionGraphPool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Concurrent;

    /// <summary>
    /// Stack-backed pool for <see cref="ActionGraph"/> instances.
    /// </summary>
    internal class ActionGraphPool : IActionGraphPool {
        private readonly ConcurrentStack<ActionGraph> pool = new();

        ActionGraph IActionGraphPool.Rent(IReadOnlyActionIndex index, IActionNodePool nodePool, NeighborLookupMode mode) {
            var graph = pool.TryPop(out var g) ? g : new ActionGraph(this);
            graph.Reinitialize(index, nodePool, mode);
            return graph;
        }

        void IActionGraphPool.Return(ActionGraph graph) => pool.Push(graph);
    }
}
