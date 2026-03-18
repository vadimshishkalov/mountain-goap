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
        /// Goals shared across all runtime instances of this template.
        /// </summary>
        public IReadOnlyList<BaseGoal> Goals { get; }

        /// <summary>
        /// Read-only view of the shared action collection. Use <see cref="ActionCollection"/>
        /// (internal) when Add access is required.
        /// </summary>
        public IReadOnlyActionIndex Actions { get; }

        /// <summary>
        /// Internal concrete action collection — used by the planner and registry.
        /// </summary>
        internal ActionCollection ActionCollection { get; }

        /// <summary>
        /// Sensors shared across all runtime instances of this template.
        /// </summary>
        public IReadOnlyList<Sensor> Sensors { get; }

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
            Goals = goals.AsReadOnly();
            ActionCollection = actions;
            Actions = actions;
            Sensors = sensors.AsReadOnly();
            CostMaximum = costMaximum;
            StepMaximum = stepMaximum;
            NeighborLookupMode = neighborLookupMode;
        }
    }
}
