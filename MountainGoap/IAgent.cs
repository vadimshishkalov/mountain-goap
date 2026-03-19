// <copyright file="IAgent.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Mutable view of an agent, exposed to executor and sensor callbacks.
    /// </summary>
    public interface IAgent : IReadOnlyAgent {
        /// <summary>
        /// Gets the agent's current state with read-write access.
        /// Hides <see cref="IReadOnlyAgent.State"/> to return the writable <see cref="IState"/>.
        /// </summary>
        new IState State { get; }

        /// <summary>Gets the agent's memory storage.</summary>
        Dictionary<string, object?> Memory { get; }

        /// <summary>Runs one step of agent work (sensors, then planning/execution).</summary>
        void Step(StepMode mode = StepMode.Default);

        /// <summary>Makes a plan synchronously.</summary>
        void Plan();

        /// <summary>Makes a plan asynchronously.</summary>
        void PlanAsync();

        /// <summary>Executes the current plan.</summary>
        void ExecutePlan();

        /// <summary>Clears the current action sequences.</summary>
        void ClearPlan();
    }
}
