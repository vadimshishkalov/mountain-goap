// <copyright file="StatePool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Concurrent;

    /// <summary>
    /// Per-agent pool for planning state objects. Eliminates GC pressure from per-node-expansion
    /// state allocations during the A* planning pass. Each agent owns one instance by default;
    /// callers may share a single pool across multiple agents. Thread-safe.
    /// </summary>
    public class StatePool : IStatePool {
        private readonly ConcurrentStack<PlanningNodeState> nodeStates = new();
        private readonly ConcurrentStack<PlanningBaseState> baseStates = new();

        PlanningNodeState IStatePool.RentNodeState(PlanningBaseState baseState) {
            if (nodeStates.TryPop(out var s)) {
                s.Reinitialize(baseState);
                return s;
            }
            return new PlanningNodeState(baseState, this);
        }

        void IStatePool.ReturnNodeState(PlanningNodeState state) => nodeStates.Push(state);

        PlanningBaseState IStatePool.RentBaseState(ConcurrentDictionary<string, object?> source) {
            var s = baseStates.TryPop(out var pooled) ? pooled : new PlanningBaseState(this);
            foreach (var kvp in source) { s.data[kvp.Key] = kvp.Value; s.keysCache.Add(kvp.Key); }
            return s;
        }

        void IStatePool.ReturnBaseState(PlanningBaseState state) => baseStates.Push(state);
    }
}
