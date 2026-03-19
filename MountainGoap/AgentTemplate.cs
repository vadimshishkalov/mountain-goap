// <copyright file="AgentTemplate.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Immutable design-time descriptor for an agent type. Registered once via
    /// <see cref="AgentRegistry.RegisterAgent"/> and shared across all runtime instances
    /// of that type.
    /// </summary>
    internal class AgentTemplate : IAgentTemplate {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object?> StateTemplate { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IReadOnlyGoal> Goals { get; }

        /// <inheritdoc/>
        public IReadOnlyActionIndex Actions { get; }

        /// <summary>
        /// Internal concrete action collection — used by the planner and registry.
        /// </summary>
        internal ActionCollection ActionCollection { get; }

        /// <inheritdoc/>
        public IReadOnlyList<Sensor> Sensors { get; }

        /// <inheritdoc/>
        public AgentConfiguration Configuration { get; }

        internal AgentTemplate(
            string name,
            IReadOnlyDictionary<string, object?> stateTemplate,
            List<BaseGoal> goals,
            ActionCollection actions,
            List<Sensor> sensors,
            AgentConfiguration configuration) {
            Name = name;
            StateTemplate = stateTemplate;
            Goals = goals.Cast<IReadOnlyGoal>().ToList().AsReadOnly();
            ActionCollection = actions;
            Actions = actions;
            Sensors = sensors.AsReadOnly();
            Configuration = configuration;
        }
    }
}
