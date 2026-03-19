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
        /// Returns a runtime instance for the named agent template. Draws from the pool when
        /// available (reinitialising to the template's default state); otherwise creates a new agent.
        /// </summary>
        public IAgent GetInstance(string name) {
            if (!agentTemplates.TryGetValue(name, out var template))
                throw new InvalidOperationException($"Agent template '{name}' is not registered. Call RegisterAgent first.");
            if (agentPools[name].TryPop(out var agent)) {
                agent.Reinitialize(template);
                return agent;
            }
            return new Agent(template, poolManager);
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
