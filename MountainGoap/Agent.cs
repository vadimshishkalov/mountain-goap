// <copyright file="Agent.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// GOAP agent.
    /// </summary>
    public class Agent : IAgent {
        /// <summary>
        /// Name of the agent.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class from a template.
        /// </summary>
        /// <param name="template">Template describing this agent type. Obtain via <see cref="Registry.RegisterAgent"/>.</param>
        /// <param name="poolManager">Shared pool manager. Pass <c>null</c> to let the agent create its own pools.</param>
        public Agent(IAgentTemplate template, PoolManager? poolManager = null) {
            if (template == null) throw new ArgumentNullException(nameof(template));
            Name = template.Name;
            State = new State(poolManager?.StatePool);
            foreach (var kvp in template.StateTemplate) State.Set(kvp.Key, kvp.Value);
            Configuration = template.Configuration;
            Template = template;
            planner = new Planner(template.Actions, poolManager);
        }

        /// <summary>
        /// Event that fires when the agent executes a step of work.
        /// </summary>
        public static event AgentStepEvent OnAgentStep = (agent) => { };

        /// <summary>
        /// Event that fires when an action sequence completes.
        /// </summary>
        public static event AgentActionSequenceCompletedEvent OnAgentActionSequenceCompleted = (agent) => { };

        /// <summary>
        /// Event that fires when planning begins.
        /// </summary>
        public static event PlanningStartedEvent OnPlanningStarted = (agent) => { };

        /// <summary>
        /// Event that fires when planning for a single goal starts.
        /// </summary>
        public static event PlanningStartedForSingleGoalEvent OnPlanningStartedForSingleGoal = (agent, goal) => { };

        /// <summary>
        /// Event that fires when planning for a single goal finishes.
        /// </summary>
        public static event PlanningFinishedForSingleGoalEvent OnPlanningFinishedForSingleGoal = (agent, goal, utility) => { };

        /// <summary>
        /// Event that fires when planning finishes.
        /// </summary>
        public static event PlanningFinishedEvent OnPlanningFinished = (agent, goal, utility) => { };

        /// <summary>
        /// Event that fires when a new plan is finalized for the agent.
        /// </summary>
        public static event PlanUpdatedEvent OnPlanUpdated = (agent, actionList) => { };

        /// <summary>
        /// Event that fires when the pathfinder evaluates a single node in the action graph.
        /// </summary>
        public static event EvaluatedActionNodeEvent OnEvaluatedActionNode = (node, nodes) => { };

        private readonly Planner planner;
        private readonly List<ActionPlan> actionSequences = new();
        private volatile bool _isBusy;
        private volatile bool _isPlanning;

        /// <summary>
        /// Gets the template this agent was created from.
        /// </summary>
        public IAgentTemplate Template { get; internal set; }

        /// <inheritdoc/>
        IReadOnlyState IReadOnlyAgent.State => State;

        /// <inheritdoc/>
        IState IAgent.State => State;

        /// <summary>
        /// Gets the chains of actions currently being performed by the agent.
        /// </summary>
        public IReadOnlyList<IActionPlan> CurrentActionSequences => actionSequences;

        /// <summary>
        /// Gets or sets the current world state from the agent perspective.
        /// </summary>
        public State State { get; set; } = null!;

        /// <summary>
        /// Gets or sets the memory storage object for the agent.
        /// </summary>
        public Dictionary<string, object?> Memory { get; set; } = new();

        /// <inheritdoc/>
        IReadOnlyList<IReadOnlyGoal> IReadOnlyAgent.Goals => Template!.Goals;

        /// <summary>
        /// Gets the goals this agent pursues. Owned by the agent's template.
        /// </summary>
        public IReadOnlyList<IReadOnlyGoal> Goals => Template!.Goals;

        /// <summary>
        /// Gets the actions available to the agent. Owned by the agent's template.
        /// </summary>
        public IReadOnlyActionIndex Actions => Template!.Actions;

        /// <summary>
        /// Gets the sensors this agent runs each step. Owned by the agent's template.
        /// </summary>
        public IReadOnlyList<Sensor> Sensors => Template!.Sensors;

        /// <summary>
        /// Gets or sets the behavioural configuration for this agent instance.
        /// Initialised from the template; can be overridden per-instance at runtime.
        /// </summary>
        public AgentConfiguration Configuration { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the agent is currently executing one or more actions.
        /// </summary>
        public bool IsBusy {
            get => _isBusy;
            internal set => _isBusy = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the agent is currently planning.
        /// </summary>
        public bool IsPlanning {
            get => _isPlanning;
            internal set => _isPlanning = value;
        }

        /// <summary>
        /// You should call this every time your game state updates.
        /// </summary>
        /// <param name="mode">Mode to be used for executing the step of work.</param>
        public void Step(StepMode mode = StepMode.Default) {
            OnAgentStep(this);
            foreach (var sensor in Sensors) sensor.Run(this);
            if (mode == StepMode.Default) {
                StepAsync();
                return;
            }
            if (!IsBusy) planner.Plan(this);
            if (mode == StepMode.OneAction) Execute();
            else if (mode == StepMode.AllActions) while (IsBusy) Execute();
        }

        /// <summary>
        /// Clears the current action sequences (also known as plans).
        /// </summary>
        public void ClearPlan() {
            foreach (var plan in actionSequences) plan.Dispose();
            actionSequences.Clear();
        }

        /// <summary>
        /// Resets mutable per-instance state from the given template so this agent can be reused
        /// from a pool. The planner and its internal object pools are retained across reuses.
        /// </summary>
        internal void Reinitialize(AgentTemplate template) {
            Template = template;
            Configuration = template.Configuration;
            State.Clear();
            foreach (var kvp in template.StateTemplate) State.Set(kvp.Key, kvp.Value);
            Memory = new Dictionary<string, object?>();
            IsBusy = false;
            IsPlanning = false;
            ClearPlan();
        }

        /// <summary>
        /// Adds an action sequence to the agent's current action sequences.
        /// </summary>
        internal void AddActionSequence(ActionPlan plan) => actionSequences.Add(plan);

        /// <summary>
        /// Makes a plan.
        /// </summary>
        public void Plan() {
            if (!IsBusy && !IsPlanning) {
                IsPlanning = true;
                planner.Plan(this);
            }
        }

        /// <summary>
        /// Makes a plan asynchronously.
        /// </summary>
        public void PlanAsync() {
            if (!_isBusy && !_isPlanning) {
                _isPlanning = true;
                if (!PlannerWorkerPool.Default.Enqueue(this))
                    _isPlanning = false;
            }
        }

        /// <summary>
        /// Runs the planner synchronously on this agent. Called by <see cref="PlannerWorkerPool"/> workers.
        /// </summary>
        internal void RunPlan() => planner.Plan(this);

        /// <summary>
        /// Executes the current plan.
        /// </summary>
        public void ExecutePlan() {
            if (!IsPlanning) Execute();
        }

        /// <summary>
        /// Triggers OnPlanningStarted event.
        /// </summary>
        /// <param name="agent">Agent that started planning.</param>
        internal static void TriggerOnPlanningStarted(Agent agent) {
            OnPlanningStarted(agent);
        }

        /// <summary>
        /// Triggers OnPlanningStartedForSingleGoal event.
        /// </summary>
        /// <param name="agent">Agent that started planning.</param>
        /// <param name="goal">Goal for which planning was started.</param>
        internal static void TriggerOnPlanningStartedForSingleGoal(Agent agent, IReadOnlyGoal goal) {
            OnPlanningStartedForSingleGoal(agent, goal);
        }

        /// <summary>
        /// Triggers OnPlanningFinishedForSingleGoal event.
        /// </summary>
        /// <param name="agent">Agent that finished planning.</param>
        /// <param name="goal">Goal for which planning was completed.</param>
        /// <param name="utility">Utility of the plan.</param>
        internal static void TriggerOnPlanningFinishedForSingleGoal(Agent agent, IReadOnlyGoal goal, float utility) {
            OnPlanningFinishedForSingleGoal(agent, goal, utility);
        }

        /// <summary>
        /// Triggers OnPlanningFinished event.
        /// </summary>
        /// <param name="agent">Agent that finished planning.</param>
        /// <param name="goal">Goal that was selected.</param>
        /// <param name="utility">Utility of the plan.</param>
        internal static void TriggerOnPlanningFinished(Agent agent, IReadOnlyGoal? goal, float utility) {
            OnPlanningFinished(agent, goal, utility);
        }

        /// <summary>
        /// Triggers OnPlanUpdated event.
        /// </summary>
        /// <param name="agent">Agent for which the plan was updated.</param>
        /// <param name="plan">New plan for the agent.</param>
        internal static void TriggerOnPlanUpdated(Agent agent, IActionPlan plan) {
            OnPlanUpdated(agent, plan);
        }

        /// <summary>
        /// Triggers OnEvaluatedActionNode event.
        /// </summary>
        /// <param name="node">Action node being evaluated.</param>
        /// <param name="nodes">List of nodes in the path that led to this point.</param>
        internal static void TriggerOnEvaluatedActionNode(IReadOnlyActionNode node, IReadOnlyDictionary<IReadOnlyActionNode, IReadOnlyActionNode> nodes) {
            OnEvaluatedActionNode(node, nodes);
        }

        /// <summary>
        /// Executes an asynchronous step of agent work.
        /// </summary>
        private void StepAsync() {
            if (!_isBusy && !_isPlanning) {
                _isPlanning = true;
                if (!PlannerWorkerPool.Default.Enqueue(this))
                    _isPlanning = false;
            }
            else if (!_isPlanning) Execute();
        }

        /// <summary>
        /// Executes the current action sequences.
        /// </summary>
        private void Execute() {
            if (actionSequences.Count > 0) {
                List<ActionPlan> cullableSequences = new();
                foreach (var sequence in actionSequences) {
                    if (sequence.Steps.Count > 0) {
                        var executionStatus = sequence.Steps[0].Execute(this);
                        if (executionStatus != ExecutionStatus.Executing) sequence.ReturnStep(0);
                    }
                    else cullableSequences.Add(sequence);
                }
                foreach (var sequence in cullableSequences) {
                    actionSequences.Remove(sequence);
                    sequence.Dispose();
                    OnAgentActionSequenceCompleted(this);
                }
            }
            else IsBusy = false;
        }
    }
}
