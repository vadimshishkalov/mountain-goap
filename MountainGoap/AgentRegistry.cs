// <copyright file="AgentRegistry.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Per-world factory for <see cref="Agent"/> objects. Registers immutable
    /// <see cref="AgentTemplate"/> descriptors by name and vends pooled runtime instances.
    /// Each independent game world should own one <see cref="AgentRegistry"/> instance.
    /// </summary>
    public class AgentRegistry {
        private readonly Dictionary<string, AgentTemplate> templates = new();
        private readonly Dictionary<string, Stack<Agent>> pools = new();

        /// <summary>
        /// Registers an agent template by name. Calling this method with the same name more than once
        /// throws <see cref="InvalidOperationException"/> — registration is one-time and explicit.
        /// </summary>
        /// <param name="name">Unique name for this agent template within the registry.</param>
        /// <param name="state">Initial state template. Snapshotted at registration; later instance mutations do not affect it.</param>
        /// <param name="goals">Goals copied into each runtime instance.</param>
        /// <param name="actions">Action collection shared across all instances of this template.</param>
        /// <param name="sensors">Sensors copied into each runtime instance.</param>
        /// <param name="costMaximum">Maximum plan cost forwarded to each instance.</param>
        /// <param name="stepMaximum">Maximum plan steps forwarded to each instance.</param>
        /// <param name="neighborLookupMode">Neighbor lookup strategy forwarded to each instance.</param>
        /// <returns>The registered <see cref="AgentTemplate"/> descriptor.</returns>
        public AgentTemplate RegisterAgent(
            string name,
            State? state = null,
            List<BaseGoal>? goals = null,
            ActionCollection? actions = null,
            List<Sensor>? sensors = null,
            float costMaximum = float.MaxValue,
            int stepMaximum = int.MaxValue,
            NeighborLookupMode neighborLookupMode = NeighborLookupMode.Index) {
            if (templates.ContainsKey(name))
                throw new InvalidOperationException($"Agent template '{name}' is already registered. RegisterAgent must be called only once per name.");

            // Snapshot the state at registration time so instance mutations never pollute the template.
            var stateSnapshot = new Dictionary<string, object?>();
            if (state != null) {
                foreach (var kvp in state) stateSnapshot[kvp.Key] = kvp.Value;
            }

            var template = new AgentTemplate(
                name,
                stateSnapshot,
                goals ?? new List<BaseGoal>(),
                actions ?? new ActionCollection(),
                sensors ?? new List<Sensor>(),
                costMaximum,
                stepMaximum,
                neighborLookupMode);
            templates[name] = template;
            pools[name] = new Stack<Agent>();
            return template;
        }

        /// <summary>
        /// Returns a runtime instance for the named agent template. Draws from the pool when
        /// available (reinitialising to the template's default state); otherwise creates a new agent.
        /// </summary>
        /// <param name="name">Name of a previously registered agent template.</param>
        /// <returns>A ready-to-use <see cref="Agent"/>.</returns>
        public Agent GetInstance(string name) {
            if (!templates.TryGetValue(name, out var template))
                throw new InvalidOperationException($"Agent template '{name}' is not registered. Call RegisterAgent first.");
            if (pools[name].TryPop(out var agent)) {
                agent.Reinitialize(template);
                return agent;
            }
            return CreateFromTemplate(template);
        }

        /// <summary>
        /// Returns an agent to its pool for future reuse. The caller must not use the agent
        /// after returning it.
        /// </summary>
        /// <param name="agent">Agent to return. Must have been vended by this registry.</param>
        public void ReturnInstance(Agent agent) {
            if (agent.Template == null || !pools.TryGetValue(agent.Template.Name, out var pool))
                throw new InvalidOperationException($"Agent '{agent.Name}' was not vended by this registry or its template is not registered.");
            pool.Push(agent);
        }

        private static Agent CreateFromTemplate(AgentTemplate template) {
            var s = new State();
            foreach (var kvp in template.StateTemplate) s.Set(kvp.Key, kvp.Value);
            var agent = new Agent(
                name: template.Name,
                state: s,
                goals: new List<BaseGoal>(template.GoalsTemplate),
                actions: template.Actions,
                sensors: new List<Sensor>(template.Sensors),
                costMaximum: template.CostMaximum,
                stepMaximum: template.StepMaximum,
                neighborLookupMode: template.NeighborLookupMode);
            agent.Template = template;
            return agent;
        }
    }
}
