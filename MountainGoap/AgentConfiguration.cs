// <copyright file="AgentConfiguration.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Immutable value object that bundles per-agent behavioural settings. Pass to
    /// <see cref="Registry"/> or per-template <see cref="Registry.RegisterAgent"/> to control
    /// planning limits and neighbour-lookup strategy.
    /// </summary>
    public class AgentConfiguration {
        /// <summary>Gets the maximum allowable plan cost. Defaults to <see cref="float.MaxValue"/>.</summary>
        public float CostMaximum { get; init; } = float.MaxValue;

        /// <summary>Gets the maximum allowable number of plan steps. Defaults to <see cref="int.MaxValue"/>.</summary>
        public int StepMaximum { get; init; } = int.MaxValue;

        /// <summary>Gets the neighbour-lookup strategy used during A* planning. Defaults to <see cref="NeighborLookupMode.Index"/>.</summary>
        public NeighborLookupMode NeighborLookupMode { get; init; } = NeighborLookupMode.Index;
    }
}
