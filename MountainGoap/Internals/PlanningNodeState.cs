// <copyright file="PlanningNodeState.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Per-node planning state. Reads fall back from delta to shared base; writes go to delta only.
    /// Snapshot() propagates the current path's delta to a child node without copying the base.
    /// </summary>
    internal class PlanningNodeState : IPlanningStepState {
        private readonly IReadOnlyState baseState;
        private readonly Dictionary<string, object?> delta;

        internal PlanningNodeState(IReadOnlyState baseState) {
            this.baseState = baseState;
            delta = new Dictionary<string, object?>();
        }

        private PlanningNodeState(IReadOnlyState baseState, Dictionary<string, object?> delta) {
            this.baseState = baseState;
            this.delta = delta;
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
                var keys = new HashSet<string>(delta.Keys);
                foreach (var key in baseState.Keys) keys.Add(key);
                return keys;
            }
        }

        /// <inheritdoc/>
        public IPlanningStepState Snapshot() => new PlanningNodeState(baseState, new Dictionary<string, object?>(delta));
    }
}
