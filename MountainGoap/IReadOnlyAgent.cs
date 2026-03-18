// <copyright file="IReadOnlyAgent.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Read-only view of an agent, exposed to observation event callbacks.
    /// </summary>
    public interface IReadOnlyAgent {
        /// <summary>Gets the agent's name.</summary>
        string Name { get; }

        /// <summary>Gets a read-only view of the agent's current state.</summary>
        IReadOnlyState State { get; }

        /// <summary>Gets the template this agent was created from, or null if not registry-vended.</summary>
        AgentTemplate? Template { get; }

        /// <summary>Gets a value indicating whether the agent is currently executing one or more actions.</summary>
        bool IsBusy { get; }

        /// <summary>Gets a value indicating whether the agent is currently planning.</summary>
        bool IsPlanning { get; }

        /// <summary>Gets the chains of actions currently being performed by the agent.</summary>
        IReadOnlyList<IActionPlan> CurrentActionSequences { get; }
    }
}
