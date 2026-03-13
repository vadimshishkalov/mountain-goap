// <copyright file="IActionGraphPool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Pool for <see cref="ActionGraph"/> instances. One graph is rented per planning pass
    /// and returned via <see cref="ActionGraph.Dispose"/>.
    /// </summary>
    internal interface IActionGraphPool {
        /// <summary>
        /// Rents an <see cref="ActionGraph"/> initialized for a planning pass.
        /// </summary>
        ActionGraph Rent(IReadOnlyActionIndex index, IActionNodePool nodePool);

        /// <summary>
        /// Returns a disposed <see cref="ActionGraph"/> to the pool for reuse.
        /// </summary>
        void Return(ActionGraph graph);
    }
}
