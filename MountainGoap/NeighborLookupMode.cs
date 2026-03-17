// <copyright file="NeighborLookupMode.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Controls how <see cref="Internals.ActionGraph"/> discovers neighbor nodes during A* planning.
    /// </summary>
    public enum NeighborLookupMode {
        /// <summary>Full iteration every hop — no caching, no index lookups.</summary>
        Disabled,

        /// <summary>Delta-based single <c>AvailableActions</c> set with precondition index lookups.</summary>
        Index,

        /// <summary>Dual-set <c>Possible</c>/<c>Candidates</c> with promotion and delta invalidation.</summary>
        Aggressive
    }
}
