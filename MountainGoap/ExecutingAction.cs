// <copyright file="ExecutingAction.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>
namespace MountainGoap {
    using System.Collections.Generic;

    /// <summary>
    /// Runtime instance of an action — holds a reference to its <see cref="Action"/> template and
    /// the resolved parameters for a specific permutation.
    /// </summary>
    public class ExecutingAction : IAction {
        /// <summary>
        /// Parameters resolved for this action instance.
        /// </summary>
        internal Dictionary<string, object?> parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutingAction"/> class.
        /// </summary>
        /// <param name="template">The immutable template that defines this action's behavior.</param>
        /// <param name="parameters">Resolved permutation parameters for this instance.</param>
        public ExecutingAction(Action template, Dictionary<string, object?>? parameters = null) {
            Template = template;
            this.parameters = parameters ?? new();
        }

        /// <summary>
        /// Gets the immutable template defining this action's behavior.
        /// </summary>
        internal Action Template { get; private set; }

        /// <summary>
        /// Gets the name of the action (forwarded from template).
        /// </summary>
        public string Name => Template.Name;

        /// <summary>
        /// Gets multiplier for delta value to provide delta cost (forwarded from template).
        /// </summary>
        internal StateCostDeltaMultiplierCallback? StateCostDeltaMultiplier => Template.StateCostDeltaMultiplier;

        /// <summary>
        /// Gets or sets the execution status of the action.
        /// </summary>
        internal ExecutionStatus ExecutionStatus { get; set; } = ExecutionStatus.NotYetExecuted;

        /// <summary>
        /// Sets a parameter on the action.
        /// </summary>
        public void SetParameter(string key, object value) {
            parameters[key] = value;
        }

        /// <summary>
        /// Gets a parameter from the action.
        /// </summary>
        public object? GetParameter(string key) {
            if (parameters.ContainsKey(key)) return parameters[key];
            return null;
        }

        /// <summary>
        /// Gets all parameter keys set on this action instance.
        /// </summary>
        public IEnumerable<string> ParameterKeys => parameters.Keys;

        /// <summary>
        /// Gets the cost of the action for the given state.
        /// </summary>
        internal float GetCost(IReadOnlyState currentState) => Template.GetCost(this, currentState);

        /// <summary>
        /// Determines whether the action is possible given the current state.
        /// </summary>
        internal bool IsPossible(IReadOnlyState state) => Template.IsPossible(this, state);

        /// <summary>
        /// Applies the effects of the action to the given state.
        /// </summary>
        internal void ApplyEffects(IState state) => Template.ApplyEffects(this, state);

        /// <summary>
        /// Executes a step of work for the agent.
        /// </summary>
        internal ExecutionStatus Execute(Agent agent) => Template.Execute(agent, this);

        /// <summary>
        /// Reinitializes this instance for reuse from a pool. Copies the provided parameters
        /// into the existing dictionary to avoid allocation.
        /// </summary>
        internal void Reinitialize(Action template, Dictionary<string, object?> parameters) {
            Template = template;
            ExecutionStatus = ExecutionStatus.NotYetExecuted;
            this.parameters.Clear();
            foreach (var kvp in parameters) this.parameters[kvp.Key] = kvp.Value;
        }
    }
}
