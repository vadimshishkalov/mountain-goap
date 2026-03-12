// <copyright file="ActionRegistry.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>
namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Instance factory for <see cref="Action"/> objects. Deduplicates by name: registering the
    /// same name more than once returns the first stored instance.
    /// </summary>
    public class ActionRegistry {
        private readonly Dictionary<string, Action> store = new();

        /// <summary>
        /// Returns a registered <see cref="Action"/> with the given name, creating and storing it
        /// on first call. Subsequent calls with the same name return the cached instance regardless
        /// of other arguments.
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
            StateCostDeltaMultiplierCallback? stateCostDeltaMultiplier = null
        ) {
            if (name != null && store.TryGetValue(name, out var existing)) return existing;
#pragma warning disable CS0618
            var action = new Action(
                name, permutationSelectors, executor, cost, costCallback,
                preconditions, comparativePreconditions, postconditions,
                arithmeticPostconditions, parameterPostconditions,
                stateMutator, stateChecker, stateCostDeltaMultiplier);
#pragma warning restore CS0618
            store[action.Name] = action;
            return action;
        }
    }
}
