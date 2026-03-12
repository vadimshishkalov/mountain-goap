// <copyright file="IAction.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>
namespace MountainGoap {
    /// <summary>
    /// Read-write view of an executing action, exposed to the executor callback.
    /// </summary>
    public interface IAction : IReadOnlyAction {
        /// <summary>
        /// Sets a parameter on the action instance.
        /// </summary>
        void SetParameter(string key, object value);
    }
}
