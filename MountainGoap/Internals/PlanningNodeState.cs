// <copyright file="PlanningNodeState.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Per-node planning state. Reads fall back from delta to shared base; writes go to delta only.
    /// Snapshot() rents a child state from the pool and copies the current delta into it.
    /// Objects are returned to the pool after the A* loop completes.
    /// </summary>
    internal class PlanningNodeState : IPlanningStepState, IDisposable {
        private readonly IStatePool pool;
        private readonly Dictionary<string, object?> delta = new();
        private PlanningBaseState baseState;

        internal PlanningNodeState(PlanningBaseState baseState, IStatePool pool) {
            this.baseState = baseState;
            this.pool = pool;
        }

        /// <inheritdoc/>
        public object? this[string key] => delta.TryGetValue(key, out var value) ? value : baseState.Get(key);

        /// <inheritdoc/>
        public object? Get(string key) {
            if (delta.TryGetValue(key, out var value)) return value;
            return baseState.Get(key);
        }

        /// <inheritdoc/>
        public void Set(string key, object? value) => delta[key] = value;

        /// <inheritdoc/>
        public bool ContainsKey(string key) => delta.ContainsKey(key) || baseState.ContainsKey(key);

        /// <inheritdoc/>
        public IEnumerable<string> Keys {
            get {
                foreach (var key in delta.Keys) yield return key;
                foreach (var key in baseState.Keys) if (!delta.ContainsKey(key)) yield return key;
            }
        }

        /// <inheritdoc/>
        public IPlanningStepState Snapshot() {
            var child = pool.RentNodeState(baseState);
            foreach (var kvp in delta) child.delta[kvp.Key] = kvp.Value;
            baseState.RegisterChild(child);
            return child;
        }

        /// <summary>
        /// Unregisters from the root base state and returns to pool.
        /// Safe to call directly — the base state will not attempt to return this node again.
        /// </summary>
        public void Dispose() {
            delta.Clear();
            baseState.UnregisterChild(this);
            pool.ReturnNodeState(this);
        }

        internal IEnumerable<string> DeltaKeys => delta.Keys;

        internal bool HasDelta => delta.Count > 0;

        internal void Reinitialize(PlanningBaseState newBaseState) {
            baseState = newBaseState;
        }
    }
}
