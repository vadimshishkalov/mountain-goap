// <copyright file="IAgentTemplate.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Read-only view of an agent type descriptor. Obtain instances via
    /// <see cref="AgentRegistry.RegisterAgent"/>.
    /// </summary>
    public interface IAgentTemplate {
        /// <summary>Gets the name used to identify this agent type in the registry.</summary>
        string Name { get; }

        /// <summary>Gets the initial state values copied into each runtime instance on construction or reinitialisation.</summary>
        IReadOnlyDictionary<string, object?> StateTemplate { get; }

        /// <summary>Gets the goals shared across all runtime instances of this template.</summary>
        IReadOnlyList<IReadOnlyGoal> Goals { get; }

        /// <summary>Gets the actions available to runtime instances of this template.</summary>
        IReadOnlyActionIndex Actions { get; }

        /// <summary>Gets the sensors shared across all runtime instances of this template.</summary>
        IReadOnlyList<Sensor> Sensors { get; }

        /// <summary>Gets the maximum plan cost forwarded to each instance.</summary>
        float CostMaximum { get; }

        /// <summary>Gets the maximum plan step count forwarded to each instance.</summary>
        int StepMaximum { get; }

        /// <summary>Gets the neighbor lookup strategy forwarded to each instance.</summary>
        NeighborLookupMode NeighborLookupMode { get; }
    }
}
