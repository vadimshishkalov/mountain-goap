// <copyright file="IReadOnlyState.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Read-only view of agent state. Callers use Get/indexer without knowing the underlying storage.
    /// </summary>
    public interface IReadOnlyState {
        /// <summary>
        /// Gets the value at the given key, or null if absent.
        /// </summary>
        object? this[string key] { get; }

        /// <summary>
        /// Gets the value at the given key, or null if absent.
        /// </summary>
        object? Get(string key);

        /// <summary>
        /// Returns true if the key exists in state.
        /// </summary>
        bool ContainsKey(string key);

        /// <summary>
        /// All keys currently in state.
        /// </summary>
        IEnumerable<string> Keys { get; }
    }
}
