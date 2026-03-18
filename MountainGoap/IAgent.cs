// <copyright file="IAgent.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Mutable view of an agent, exposed to executor and sensor callbacks.
    /// </summary>
    public interface IAgent : IReadOnlyAgent {
        /// <summary>
        /// Gets the agent's current state with read-write access.
        /// Hides <see cref="IReadOnlyAgent.State"/> to return the writable <see cref="IState"/>.
        /// </summary>
        new IState State { get; }

        /// <summary>Gets the agent's memory storage.</summary>
        Dictionary<string, object?> Memory { get; }
    }
}
