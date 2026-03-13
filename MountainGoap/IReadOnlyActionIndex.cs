// <copyright file="IReadOnlyActionIndex.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Read-only view of an indexed action collection. Used by the planning internals to
    /// retrieve action candidates by state key without scanning every action template.
    /// </summary>
    internal interface IReadOnlyActionIndex : IEnumerable<Action> {
        /// <summary>Gets the total number of action templates in the collection.</summary>
        int Count { get; }

        /// <summary>
        /// Adds to <paramref name="result"/> every action template whose preconditions reference
        /// at least one of the given <paramref name="keys"/>, plus any always-candidate templates
        /// (those with no static precondition keys). Does not clear <paramref name="result"/>
        /// before filling — caller is responsible for clearing when needed.
        /// </summary>
        void GetCandidates(IEnumerable<string> keys, HashSet<Action> result);
    }
}
