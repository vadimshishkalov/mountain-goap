// <copyright file="IExecutionState.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// State view used during action execution. Reads and writes go directly to live agent data.
    /// </summary>
    public interface IExecutionState : IState {
    }
}
