// <copyright file="AgentTemplate.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Immutable design-time descriptor for an agent type. Registered once via
    /// <see cref="AgentRegistry.RegisterAgent"/> and shared across all runtime instances
    /// of that type.
    /// </summary>
    public class AgentTemplate {
        /// <summary>
        /// Gets the name used to identify this agent type in the registry.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Shallow snapshot of the initial state taken at registration time.
        /// Copied into each new <see cref="AgentInstance"/> on construction or reinitialisation.
        /// </summary>
        internal IReadOnlyDictionary<string, object?> StateTemplate { get; }

        /// <summary>
        /// Goals to copy into each runtime instance.
        /// </summary>
        internal IReadOnlyList<BaseGoal> GoalsTemplate { get; }

        /// <summary>
        /// Shared action collection. All instances of this type reference the same object — do not
        /// mutate it after registration.
        /// </summary>
        public ActionCollection Actions { get; }

        /// <summary>
        /// Sensors to copy into each runtime instance.
        /// </summary>
        internal IReadOnlyList<Sensor> Sensors { get; }

        /// <summary>
        /// Maximum plan cost forwarded to each instance.
        /// </summary>
        public float CostMaximum { get; }

        /// <summary>
        /// Maximum plan step count forwarded to each instance.
        /// </summary>
        public int StepMaximum { get; }

        /// <summary>
        /// Neighbor lookup strategy forwarded to each instance.
        /// </summary>
        public NeighborLookupMode NeighborLookupMode { get; }

        internal AgentTemplate(
            string name,
            IReadOnlyDictionary<string, object?> stateTemplate,
            List<BaseGoal> goals,
            ActionCollection actions,
            List<Sensor> sensors,
            float costMaximum,
            int stepMaximum,
            NeighborLookupMode neighborLookupMode) {
            Name = name;
            StateTemplate = stateTemplate;
            GoalsTemplate = goals.AsReadOnly();
            Actions = actions;
            Sensors = sensors.AsReadOnly();
            CostMaximum = costMaximum;
            StepMaximum = stepMaximum;
            NeighborLookupMode = neighborLookupMode;
        }
    }
}
