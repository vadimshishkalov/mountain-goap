// <copyright file="IState.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Read-write view of agent state. Callers use Set without knowing the underlying storage or layer routing.
    /// </summary>
    public interface IState : IReadOnlyState {
        /// <summary>
        /// Sets the value at the given key.
        /// </summary>
        void Set(string key, object? value);

        /// <summary>
        /// Gets or sets the value at the given key. Hides the read-only indexer from <see cref="IReadOnlyState"/>.
        /// </summary>
        new object? this[string key] { get; set; }
    }
}
