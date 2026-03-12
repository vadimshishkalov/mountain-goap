// <copyright file="IActionNodePool.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Pool for ActionNode and ExecutingAction objects used during A* planning.
    /// A single pool instance can be shared across multiple agents to reduce per-agent
    /// memory overhead. Thread-safe implementations allow concurrent planning.
    /// </summary>
    internal interface IActionNodePool {
        /// <summary>
        /// Rents an ActionNode, reusing a pooled instance if available.
        /// </summary>
        ActionNode RentNode(ExecutingAction? action, IPlanningStepState state);

        /// <summary>
        /// Returns an ActionNode to the pool. Clears the action reference before storing.
        /// </summary>
        void ReturnNode(ActionNode node);

        /// <summary>
        /// Rents an ExecutingAction, reusing a pooled instance if available.
        /// The returned instance is initialized with a copy of the provided parameters.
        /// </summary>
        ExecutingAction RentAction(Action template, Dictionary<string, object?> parameters);

        /// <summary>
        /// Returns an ExecutingAction to the pool.
        /// </summary>
        void ReturnAction(ExecutingAction action);
    }
}
