// <copyright file="FinishExecuteActionEvent.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    /// <summary>
    /// Delegate type for a listener to the event that fires when an action finishes executing.
    /// </summary>
    /// <param name="agent">Agent executing the action.</param>
    /// <param name="action">Action being executed.</param>
    /// <param name="status">Execution status of the action.</param>
    public delegate void FinishExecuteActionEvent(Agent agent, IAction action, ExecutionStatus status);
}
