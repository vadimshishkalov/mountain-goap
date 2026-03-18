// <copyright file="Action.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>
namespace MountainGoap {
    using System;
    using System.Collections.Generic;
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

        // Permutation key array — fixed at construction, safe to share across threads.
        private readonly string[] _permKeys;

        // Materialised key sets — fixed at construction, avoids yield-return enumerator allocations.
        // HashSet<T>.GetEnumerator() returns a struct enumerator — zero heap allocation in foreach.
        private readonly HashSet<string> _preconditionKeys;
        private readonly HashSet<string> _postconditionKeys;

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
            _permKeys = new string[this.permutationSelectors.Count];
            int ki = 0;
            foreach (var key in this.permutationSelectors.Keys) _permKeys[ki++] = key;
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

            _preconditionKeys = CollectKeys(this.preconditions.Keys, this.comparativePreconditions.Keys);
            _postconditionKeys = CollectKeys(this.postconditions.Keys, this.arithmeticPostconditions.Keys, this.parameterPostconditions.Values);
        }

        private static readonly HashSet<string> EmptyKeySet = new();

        private static HashSet<string> CollectKeys(params IEnumerable<string>[] sources) {
            HashSet<string>? set = null;
            foreach (var s in sources)
                foreach (var key in s)
                    (set ??= new HashSet<string>()).Add(key);
            return set ?? EmptyKeySet;
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
        /// Gets the distinct state keys referenced by static preconditions. Used by
        /// <see cref="ActionCollection"/> to build the inverse precondition index.
        /// </summary>
        internal IEnumerable<string> PreconditionKeys => _preconditionKeys;

        /// <summary>
        /// Gets the distinct state keys this action writes to via static postconditions.
        /// Does not include keys written by <see cref="stateMutator"/> (unknown at design time).
        /// </summary>
        internal IEnumerable<string> PostconditionKeys => _postconditionKeys;

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
        /// Yields permutations lazily using cached scratch buffers — zero per-call allocations.
        /// The yielded <see cref="Permutation"/> shares a reusable values buffer; consumers
        /// must copy before advancing the enumerator.
        /// </summary>
        // Per-thread scratch buffers for GetPermutations. Safe because permutation iteration
        // is never nested on the same thread (Neighbors fully consumes one action before the next).
        [ThreadStatic] private static List<object>[]? _tsValues;
        [ThreadStatic] private static int[]? _tsIndices;
        [ThreadStatic] private static int[]? _tsCounts;
        [ThreadStatic] private static object?[]? _tsCurrentValues;

        internal IEnumerable<Permutation> GetPermutations(IReadOnlyState state) {
            var n = _permKeys.Length;
            if (n == 0) {
                yield return new Permutation(_permKeys, Array.Empty<object?>(), 0);
                yield break;
            }

            if (_tsValues == null || _tsValues.Length < n) {
                _tsValues = new List<object>[n];
                _tsIndices = new int[n];
                _tsCounts = new int[n];
                _tsCurrentValues = new object?[n];
            }

            var values = _tsValues;
            var indices = _tsIndices!;
            var counts = _tsCounts!;
            var currentValues = _tsCurrentValues!;

            for (int i = 0; i < n; i++) {
                indices[i] = 0;
                values[i] = permutationSelectors[_permKeys[i]](state);
                var c = values[i].Count;
                if (c == 0) yield break;
                counts[i] = c;
            }

            var permutation = new Permutation(_permKeys, currentValues, n);

            while (true) {
                for (int i = 0; i < n; i++) {
                    currentValues[i] = values[i][indices[i]];
                }
                yield return permutation;

                int j = 0;
                while (j < n) {
                    if (indices[j] < counts[j] - 1) {
                        indices[j]++;
                        break;
                    }
                    indices[j] = 0;
                    j++;
                }
                if (j == n) yield break;
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

        private static ExecutionStatus DefaultExecutorCallback(IAgent agent, IAction action) {
            return ExecutionStatus.Failed;
        }
    }
}
