// <copyright file="AgentConfiguration.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Value object that bundles per-agent behavioural settings, settable at construction. Pass to
    /// <see cref="Registry"/> or per-template <see cref="Registry.RegisterAgent"/> to control
    /// planning limits and neighbour-lookup strategy.
    /// </summary>
    public class AgentConfiguration {
        /// <summary>Gets or sets the maximum allowable plan cost. Defaults to <see cref="float.MaxValue"/>.</summary>
        public float CostMaximum { get; set; } = float.MaxValue;

        /// <summary>Gets or sets the maximum allowable number of plan steps. Defaults to <see cref="int.MaxValue"/>.</summary>
        public int StepMaximum { get; set; } = int.MaxValue;

        /// <summary>Gets or sets the neighbour-lookup strategy used during A* planning. Defaults to <see cref="NeighborLookupMode.Index"/>.</summary>
        public NeighborLookupMode NeighborLookupMode { get; set; } = NeighborLookupMode.Index;

        /// <summary>
        /// Gets or sets the initial capacity of the A* frontier (open-set) priority queue. The queue grows
        /// by 2x whenever it would overflow, so this is a starting size, not a hard cap. Larger
        /// values reserve more memory up front but avoid growth reallocations. Defaults to 1024.
        /// </summary>
        public int FrontierInitialCapacity { get; set; } = 1024;
    }
}
