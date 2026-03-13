// <copyright file="Action.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>
namespace MountainGoap {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Defines the immutable design-time definition of an action, shared across agents.
    /// </summary>
    public class Action {
        /// <summary>
        /// Name of the action.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Cost of the action.
        /// </summary>
        private readonly float cost;

        /// <summary>
        /// The permutation selector callbacks for the action.
        /// </summary>
        private readonly Dictionary<string, PermutationSelectorCallback> permutationSelectors;

        /// <summary>
        /// The executor callback for the action.
        /// </summary>
        private readonly ExecutorCallback executor;

        /// <summary>
        /// The cost callback for the action.
        /// </summary>
        private readonly CostCallback costCallback;

        private static readonly Dictionary<string, object?> EmptyObjectNullableDict = new();
        private static readonly Dictionary<string, ComparisonValuePair> EmptyComparisonDict = new();
        private static readonly Dictionary<string, object> EmptyObjectDict = new();
        private static readonly Dictionary<string, string> EmptyStringDict = new();

        /// <summary>
        /// Preconditions for the action. These things are required for the action to execute.
        /// </summary>
        private readonly Dictionary<string, object?> preconditions;

        /// <summary>
        /// Comparative preconditions for the action. Indicates that a value must be greater than or less than a certain value for the action to execute.
        /// </summary>
        private readonly Dictionary<string, ComparisonValuePair> comparativePreconditions;

        /// <summary>
        /// Postconditions for the action. These will be set when the action has executed.
        /// </summary>
        private readonly Dictionary<string, object?> postconditions;

        /// <summary>
        /// Arithmetic postconditions for the action. These will be added to the current value when the action has executed.
        /// </summary>
        private readonly Dictionary<string, object> arithmeticPostconditions;

        /// <summary>
        /// Parameter postconditions for the action. When the action has executed, the value of the parameter given in the key will be copied to the state with the name given in the value.
        /// </summary>
        private readonly Dictionary<string, string> parameterPostconditions;

        /// <summary>
        /// State mutator for modifying state programmatically after action execution or evaluation.
        /// </summary>
        private readonly StateMutatorCallback? stateMutator;

        /// <summary>
        /// State checker for checking state programmatically before action execution or evaluation.
        /// </summary>
        private readonly StateCheckerCallback? stateChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="Action"/> class.
        /// </summary>
        /// <param name="name">Name for the action, for eventing and logging purposes.</param>
        /// <param name="permutationSelectors">The permutation selector callback for the action's parameters.</param>
        /// <param name="executor">The executor callback for the action.</param>
        /// <param name="cost">Cost of the action.</param>
        /// <param name="costCallback">Callback for determining the cost of the action.</param>
        /// <param name="preconditions">Preconditions required in the world state in order for the action to occur.</param>
        /// <param name="comparativePreconditions">Preconditions indicating relative value requirements needed for the action to occur.</param>
        /// <param name="postconditions">Postconditions applied after the action is successfully executed.</param>
        /// <param name="arithmeticPostconditions">Arithmetic postconditions added to state after the action is successfully executed.</param>
        /// <param name="parameterPostconditions">Parameter postconditions copied to state after the action is successfully executed.</param>
        /// <param name="stateMutator">Callback for modifying state after action execution or evaluation.</param>
        /// <param name="stateChecker">Callback for checking state before action execution or evaluation.</param>
        /// <param name="stateCostDeltaMultiplier">Callback for multiplier for delta value to provide delta cost.</param>
        [Obsolete("Use ActionRegistry.RegisterAction instead.")]
        public Action(string? name = null, Dictionary<string, PermutationSelectorCallback>? permutationSelectors = null, ExecutorCallback? executor = null, float cost = 1f, CostCallback? costCallback = null, Dictionary<string, object?>? preconditions = null, Dictionary<string, ComparisonValuePair>? comparativePreconditions = null, Dictionary<string, object?>? postconditions = null, Dictionary<string, object>? arithmeticPostconditions = null, Dictionary<string, string>? parameterPostconditions = null, StateMutatorCallback? stateMutator = null, StateCheckerCallback? stateChecker = null, StateCostDeltaMultiplierCallback? stateCostDeltaMultiplier = null) {
            this.permutationSelectors = permutationSelectors ?? new();
            this.executor = executor ?? DefaultExecutorCallback;
            Name = name ?? $"Action {Guid.NewGuid()} ({this.executor.GetMethodInfo().Name})";
            this.cost = cost;
            this.costCallback = costCallback ?? ((_, _) => this.cost);
            this.preconditions = preconditions ?? EmptyObjectNullableDict;
            this.comparativePreconditions = comparativePreconditions ?? EmptyComparisonDict;
            this.postconditions = postconditions ?? EmptyObjectNullableDict;
            this.arithmeticPostconditions = arithmeticPostconditions ?? EmptyObjectDict;
            this.parameterPostconditions = parameterPostconditions ?? EmptyStringDict;
            this.stateMutator = stateMutator;
            this.stateChecker = stateChecker;
            StateCostDeltaMultiplier = stateCostDeltaMultiplier ?? DefaultStateCostDeltaMultiplier;
        }

        /// <summary>
        /// Gets or sets multiplier for delta value to provide delta cost.
        /// </summary>
        public StateCostDeltaMultiplierCallback? StateCostDeltaMultiplier { get; set; }

        /// <summary>
        /// Default multiplier callback returning 1 for all state keys.
        /// </summary>
        public static float DefaultStateCostDeltaMultiplier(IReadOnlyAction? action, string stateKey) => 1f;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Action other && Name == other.Name;

        /// <inheritdoc/>
        public override int GetHashCode() => Name.GetHashCode();

        /// <summary>
        /// Event that triggers when an action begins executing.
        /// </summary>
        public static event BeginExecuteActionEvent OnBeginExecuteAction = (agent, action) => { };

        /// <summary>
        /// Event that triggers when an action finishes executing.
        /// </summary>
        public static event FinishExecuteActionEvent OnFinishExecuteAction = (agent, action, status) => { };

        /// <summary>
        /// Gets the state keys referenced by static preconditions. Used by
        /// <see cref="ActionCollection"/> to build the inverse precondition index.
        /// </summary>
        internal IEnumerable<string> PreconditionKeys {
            get {
                foreach (var key in preconditions.Keys) yield return key;
                foreach (var key in comparativePreconditions.Keys) yield return key;
            }
        }

        /// <summary>
        /// Gets the state keys this action writes to via static postconditions.
        /// Does not include keys written by <see cref="stateMutator"/> (unknown at design time).
        /// </summary>
        internal IEnumerable<string> PostconditionKeys {
            get {
                foreach (var key in postconditions.Keys) yield return key;
                foreach (var key in arithmeticPostconditions.Keys) yield return key;
                foreach (var key in parameterPostconditions.Values) yield return key;
            }
        }

        /// <summary>
        /// True when a <see cref="stateMutator"/> is set — <see cref="PostconditionKeys"/> is
        /// then incomplete because the mutator can write to arbitrary state keys.
        /// </summary>
        internal bool HasStateMutator => stateMutator != null;

        internal bool HasStateChecker => stateChecker != null;


        /// <summary>
        /// Gets the cost of the action for the given runtime action and state.
        /// </summary>
        internal float GetCost(ExecutingAction action, IReadOnlyState currentState) {
            try {
                return costCallback(action, currentState);
            }
            catch {
                return float.MaxValue;
            }
        }

        /// <summary>
        /// Executes the action for the given agent and runtime action instance.
        /// </summary>
        internal ExecutionStatus Execute(Agent agent, ExecutingAction action) {
            OnBeginExecuteAction(agent, action);
            if (IsPossible(action, agent.State)) {
                var newState = executor(agent, action);
                if (newState == ExecutionStatus.Succeeded) ApplyEffects(action, agent.State);
                action.ExecutionStatus = newState;
                OnFinishExecuteAction(agent, action, action.ExecutionStatus);
                return newState;
            }
            else {
                OnFinishExecuteAction(agent, action, ExecutionStatus.NotPossible);
                return ExecutionStatus.NotPossible;
            }
        }

        /// <summary>
        /// Determines whether or not an action is possible.
        /// </summary>
        internal bool IsPossible(ExecutingAction action, IReadOnlyState state) {
            if (!CheckStaticPreconditions(state)) return false;
            if (stateChecker?.Invoke(action, state) == false) return false;
            return true;
        }

        /// <summary>
        /// Gets all permutations of parameters possible for this action.
        /// </summary>
        internal List<Dictionary<string, object?>> GetPermutations(IReadOnlyState state) {
            List<Dictionary<string, object?>> combinedOutputs = new();
            Dictionary<string, List<object>> outputs = new();
            foreach (var kvp in permutationSelectors) outputs[kvp.Key] = kvp.Value(state);
            var permutationParameters = outputs.Keys.ToList();
            List<int> indices = new();
            List<int> counts = new();
            foreach (var parameter in permutationParameters) {
                indices.Add(0);
                if (outputs[parameter].Count == 0) return combinedOutputs;
                counts.Add(outputs[parameter].Count);
            }
            while (true) {
                var singleOutput = new Dictionary<string, object?>();
                for (int i = 0; i < indices.Count; i++) {
                    if (indices[i] >= outputs[permutationParameters[i]].Count) continue;
                    singleOutput[permutationParameters[i]] = outputs[permutationParameters[i]][indices[i]];
                }
                combinedOutputs.Add(singleOutput);
                if (IndicesAtMaximum(indices, counts)) return combinedOutputs;
                IncrementIndices(indices, counts);
            }
        }

        /// <summary>
        /// Applies the effects of the action to the given state.
        /// </summary>
        internal void ApplyEffects(ExecutingAction action, IState state) {
            foreach (var kvp in postconditions) state.Set(kvp.Key, kvp.Value);
            foreach (var kvp in arithmeticPostconditions) {
                if (!state.ContainsKey(kvp.Key)) continue;
                if (state[kvp.Key] is int stateInt && kvp.Value is int conditionInt) state.Set(kvp.Key, stateInt + conditionInt);
                else if (state[kvp.Key] is float stateFloat && kvp.Value is float conditionFloat) state.Set(kvp.Key, stateFloat + conditionFloat);
                else if (state[kvp.Key] is double stateDouble && kvp.Value is double conditionDouble) state.Set(kvp.Key, stateDouble + conditionDouble);
                else if (state[kvp.Key] is long stateLong && kvp.Value is long conditionLong) state.Set(kvp.Key, stateLong + conditionLong);
                else if (state[kvp.Key] is decimal stateDecimal && kvp.Value is decimal conditionDecimal) state.Set(kvp.Key, stateDecimal + conditionDecimal);
                else if (state[kvp.Key] is DateTime stateDateTime && kvp.Value is TimeSpan conditionTimeSpan) state.Set(kvp.Key, stateDateTime + conditionTimeSpan);
            }
            foreach (var kvp in parameterPostconditions) {
                var paramVal = action.GetParameter(kvp.Key);
                if (paramVal == null) continue;
                state.Set(kvp.Value, paramVal);
            }
            stateMutator?.Invoke(action, state);
        }

        private static bool IndicesAtMaximum(List<int> indices, List<int> counts) {
            for (int i = 0; i < indices.Count; i++) if (indices[i] < counts[i] - 1) return false;
            return true;
        }

        private static void IncrementIndices(List<int> indices, List<int> counts) {
            if (IndicesAtMaximum(indices, counts)) return;
            for (int i = 0; i < indices.Count; i++) {
                if (indices[i] == counts[i] - 1) indices[i] = 0;
                else {
                    indices[i]++;
                    return;
                }
            }
        }

        private bool CheckStaticPreconditions(IReadOnlyState state) {
            foreach (var kvp in preconditions) {
                if (!state.ContainsKey(kvp.Key)) return false;
                if (state[kvp.Key] == null && state[kvp.Key] != kvp.Value) return false;
                else if (state[kvp.Key] == null && state[kvp.Key] == kvp.Value) continue;
                if (state[kvp.Key] is object obj && !obj.Equals(kvp.Value)) return false;
            }
            foreach (var kvp in comparativePreconditions) {
                if (!state.ContainsKey(kvp.Key)) return false;
                if (state[kvp.Key] == null) return false;
                if (state[kvp.Key] is object obj && kvp.Value.Value is object obj2) {
                    if (kvp.Value.Operator == ComparisonOperator.LessThan && !Utils.IsLowerThan(obj, obj2)) return false;
                    else if (kvp.Value.Operator == ComparisonOperator.GreaterThan && !Utils.IsHigherThan(obj, obj2)) return false;
                    else if (kvp.Value.Operator == ComparisonOperator.LessThanOrEquals && !Utils.IsLowerThanOrEquals(obj, obj2)) return false;
                    else if (kvp.Value.Operator == ComparisonOperator.GreaterThanOrEquals && !Utils.IsHigherThanOrEquals(obj, obj2)) return false;
                }
                else return false;
            }
            return true;
        }

        private static ExecutionStatus DefaultExecutorCallback(Agent agent, IAction action) {
            return ExecutionStatus.Failed;
        }
    }
}
