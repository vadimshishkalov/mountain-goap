// <copyright file="Registry.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Per-world factory for both <see cref="Action"/> and <see cref="Agent"/> objects.
    /// Actions are deduplicated by name; agent templates are registered once and vend pooled
    /// runtime instances. Provide an <see cref="AgentConfiguration"/> and/or a
    /// <see cref="PoolManager"/> at construction to set world-wide defaults.
    /// </summary>
    public class Registry {
        private readonly AgentConfiguration defaultConfiguration;
        private readonly PoolManager? poolManager;
        private readonly Dictionary<string, AgentTemplate> agentTemplates = new();
        private readonly Dictionary<string, Stack<Agent>> agentPools = new();
        private readonly Dictionary<string, Action> actionStore = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Registry"/> class.
        /// </summary>
        /// <param name="configuration">Default behavioural configuration applied to every registered agent type. Individual calls to <see cref="RegisterAgent"/> may override this per template.</param>
        /// <param name="poolManager">Shared object pools for planning. Pass <c>null</c> to let each agent create its own pools.</param>
        public Registry(AgentConfiguration? configuration = null, PoolManager? poolManager = null) {
            defaultConfiguration = configuration ?? new AgentConfiguration();
            this.poolManager = poolManager;
        }

        /// <summary>
        /// Returns a registered <see cref="Action"/> with the given name, creating and storing it
        /// on first call. Subsequent calls with the same name return the cached instance regardless
        /// of other arguments. Passing <c>null</c> as the name always creates a new instance.
        /// </summary>
        public Action RegisterAction(
            string? name = null,
            Dictionary<string, PermutationSelectorCallback>? permutationSelectors = null,
            ExecutorCallback? executor = null,
            float cost = 1f,
            CostCallback? costCallback = null,
            Dictionary<string, object?>? preconditions = null,
            Dictionary<string, ComparisonValuePair>? comparativePreconditions = null,
            Dictionary<string, object?>? postconditions = null,
            Dictionary<string, object>? arithmeticPostconditions = null,
            Dictionary<string, string>? parameterPostconditions = null,
            StateMutatorCallback? stateMutator = null,
            StateCheckerCallback? stateChecker = null,
            StateCostDeltaMultiplierCallback? stateCostDeltaMultiplier = null) {
            if (name != null && actionStore.TryGetValue(name, out var existing)) return existing;
#pragma warning disable CS0618
            var action = new Action(
                name, permutationSelectors, executor, cost, costCallback,
                preconditions, comparativePreconditions, postconditions,
                arithmeticPostconditions, parameterPostconditions,
                stateMutator, stateChecker, stateCostDeltaMultiplier);
#pragma warning restore CS0618
            actionStore[action.Name] = action;
            return action;
        }

        /// <summary>
        /// Registers an agent template by name. Throws <see cref="InvalidOperationException"/> if
        /// the name is already registered. The provided <paramref name="configuration"/> overrides
        /// the registry-level default for this template only; pass <c>null</c> to use the default.
        /// </summary>
        /// <param name="name">Unique name identifying this agent type within the registry.</param>
        /// <param name="state">Initial state values copied into each runtime instance.</param>
        /// <param name="goals">Goals shared across all runtime instances of this template.</param>
        /// <param name="actions">Actions available to runtime instances of this template.</param>
        /// <param name="sensors">Sensors shared across all runtime instances of this template.</param>
        /// <param name="configuration">Behavioural configuration for this template; overrides the registry default.</param>
        /// <returns>The registered <see cref="IAgentTemplate"/>.</returns>
        public IAgentTemplate RegisterAgent(
            string name,
            State? state = null,
            List<BaseGoal>? goals = null,
            ActionCollection? actions = null,
            List<Sensor>? sensors = null,
            AgentConfiguration? configuration = null) {
            if (agentTemplates.ContainsKey(name))
                throw new InvalidOperationException($"Agent template '{name}' is already registered. RegisterAgent must be called only once per name.");

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
                configuration ?? defaultConfiguration);
            agentTemplates[name] = template;
            agentPools[name] = new Stack<Agent>();
            return template;
        }

        /// <summary>
        /// Returns <c>true</c> if an agent template with the given name is registered.
        /// </summary>
        public bool HasTemplate(string name) => agentTemplates.ContainsKey(name);

        /// <summary>
        /// Attempts to retrieve a registered agent template by name.
        /// </summary>
        /// <param name="name">The template name to look up.</param>
        /// <param name="template">When this method returns, contains the template if found; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the template was found; otherwise <c>false</c>.</returns>
        public bool TryGetTemplate(string name, out IAgentTemplate? template) {
            if (agentTemplates.TryGetValue(name, out var t)) {
                template = t;
                return true;
            }
            template = null;
            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if an action with the given name is registered.
        /// </summary>
        public bool HasAction(string name) => actionStore.ContainsKey(name);

        /// <summary>
        /// Attempts to retrieve a registered action by name.
        /// </summary>
        /// <param name="name">The action name to look up.</param>
        /// <param name="action">When this method returns, contains the action if found; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the action was found; otherwise <c>false</c>.</returns>
        public bool TryGetAction(string name, out Action? action) {
            return actionStore.TryGetValue(name, out action);
        }

        /// <summary>
        /// Returns a runtime instance for the named agent template. Draws from the pool when
        /// available (reinitialising to the template's default state); otherwise creates a new agent.
        /// </summary>
        /// <param name="templateName">The name of a previously registered agent template.</param>
        /// <param name="name">Optional instance name for identification/debugging. When <c>null</c>, auto-generated as <c>templateName_guid</c>.</param>
        /// <returns>A ready-to-use <see cref="IAgent"/> instance.</returns>
        public IAgent GetInstance(string templateName, string? name = null) {
            if (!agentTemplates.TryGetValue(templateName, out var template))
                throw new InvalidOperationException($"Agent template '{templateName}' is not registered. Call RegisterAgent first.");
            if (agentPools[templateName].TryPop(out var agent)) {
                agent.Reinitialize(template, name);
                return agent;
            }
            return new Agent(template, poolManager, name);
        }

        /// <summary>
        /// Returns an agent to its pool for future reuse. The caller must not use the agent
        /// after returning it.
        /// </summary>
        public void ReturnInstance(IAgent agent) {
            var a = (Agent)agent;
            if (!agentPools.TryGetValue(a.Template.Name, out var pool))
                throw new InvalidOperationException($"Agent '{a.Name}' was not vended by this registry or its template is not registered.");
            pool.Push(a);
        }
    }
}
