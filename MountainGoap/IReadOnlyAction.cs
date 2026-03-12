// <copyright file="IReadOnlyAction.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>
namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Read-only view of an executing action, exposed to planning and read-only execution callbacks.
    /// </summary>
    public interface IReadOnlyAction {
        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value of a parameter by key, or null if not present.
        /// </summary>
        object? GetParameter(string key);

        /// <summary>
        /// Gets all parameter keys set on this action instance.
        /// </summary>
        IEnumerable<string> ParameterKeys { get; }
    }
}
