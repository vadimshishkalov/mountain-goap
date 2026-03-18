// <copyright file="State.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Live agent state backed by a ConcurrentDictionary. Reads and writes go directly to the data.
    /// Supports collection initializer syntax: new State { { "key", value } }.
    /// </summary>
    public class State : IExecutionState, IEnumerable<KeyValuePair<string, object?>> {
        private readonly ConcurrentDictionary<string, object?> data = new();
        private readonly IStatePool pool;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="pool">Optional pool for planning state objects. If null, a new pool is created.</param>
        public State(StatePool? pool = null) {
            this.pool = pool ?? new StatePool();
        }

        /// <inheritdoc/>
        public object? this[string key] {
            get => data.TryGetValue(key, out var value) ? value : null;
            set => data[key] = value;
        }

        /// <inheritdoc/>
        public object? Get(string key) => data.TryGetValue(key, out var value) ? value : null;

        /// <inheritdoc/>
        public void Set(string key, object? value) => data[key] = value;

        /// <summary>
        /// Adds a key-value pair; enables collection initializer syntax.
        /// </summary>
        public void Add(string key, object? value) => data[key] = value;

        /// <inheritdoc/>
        public bool ContainsKey(string key) => data.ContainsKey(key);

        /// <inheritdoc/>
        public IEnumerable<string> Keys => data.Keys;

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => data.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)data).GetEnumerator();

        /// <summary>
        /// Removes all entries. Used by <see cref="Agent.Reinitialize"/> to reset state in-place.
        /// </summary>
        internal void Clear() => data.Clear();

        /// <summary>
        /// Creates an immutable base-layer snapshot of the current state for use during a planning pass.
        /// </summary>
        internal IPlanningBaseState Snapshot() => pool.RentBaseState(data);
    }
}
