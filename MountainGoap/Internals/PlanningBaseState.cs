// <copyright file="PlanningBaseState.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Immutable snapshot of agent state taken once at the start of a planning pass.
    /// Shared by reference across all planning nodes in a single pass — never copied again.
    /// Returned to the pool after the planning pass completes.
    /// </summary>
    internal class PlanningBaseState : IPlanningBaseState {
        private readonly IStatePool pool;
        internal readonly Dictionary<string, object?> data = new();
        internal readonly HashSet<string> keysCache = new();
        private readonly List<PlanningNodeState> children = new();
        private readonly List<PlanningNodeState> disposeBuffer = new();

        internal PlanningBaseState(IStatePool pool) {
            this.pool = pool;
        }

        /// <inheritdoc/>
        public object? this[string key] => data.TryGetValue(key, out var value) ? value : null;

        /// <inheritdoc/>
        public object? Get(string key) => data.TryGetValue(key, out var value) ? value : null;

        /// <inheritdoc/>
        public bool ContainsKey(string key) => data.ContainsKey(key);

        /// <inheritdoc/>
        public IEnumerable<string> Keys => keysCache;

        internal void RegisterChild(PlanningNodeState child) => children.Add(child);

        internal void UnregisterChild(PlanningNodeState child) => children.Remove(child);

        public IPlanningStepState Snapshot() {
            var child = pool.RentNodeState(this);
            children.Add(child);
            return child;
        }

        public void Dispose() {
            data.Clear();
            keysCache.Clear();
            disposeBuffer.AddRange(children);
            children.Clear();
            foreach (var child in disposeBuffer) child.Dispose();
            disposeBuffer.Clear();
            pool.ReturnBaseState(this);
        }
    }
}
