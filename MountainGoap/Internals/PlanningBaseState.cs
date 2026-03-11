// <copyright file="PlanningBaseState.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Immutable snapshot of agent state taken once at the start of a planning pass.
    /// Shared by reference across all planning nodes in a single pass — never copied again.
    /// </summary>
    internal class PlanningBaseState : IReadOnlyState {
        private readonly Dictionary<string, object?> data;

        internal PlanningBaseState(Dictionary<string, object?> snapshot) {
            data = snapshot;
        }

        /// <inheritdoc/>
        public object? this[string key] => data.TryGetValue(key, out var value) ? value : null;

        /// <inheritdoc/>
        public object? Get(string key) => data.TryGetValue(key, out var value) ? value : null;

        /// <inheritdoc/>
        public bool ContainsKey(string key) => data.ContainsKey(key);

        /// <inheritdoc/>
        public IEnumerable<string> Keys => data.Keys;
    }
}
