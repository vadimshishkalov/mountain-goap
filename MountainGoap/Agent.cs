// <copyright file="Agent.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// GOAP agent.
    /// </summary>
    public class Agent {
        /// <summary>
        /// Name of the agent.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="name">Name of the agent.</param>
        /// <param name="state">Initial agent state.</param>
        /// <param name="memory">Initial agent memory.</param>
        /// <param name="goals">Initial agent goals.</param>
        /// <param name="actions">Actions available to the agent.</param>
        /// <param name="sensors">Sensors available to the agent.</param>
        /// <param name="costMaximum">Maximum cost of an allowable plan.</param>
        /// <param name="stepMaximum">Maximum steps in an allowable plan.</param>
        /// <param name="neighborLookupMode">Neighbor lookup strategy for planning.</param>
        public Agent(string? name = null, State? state = null, Dictionary<string, object?>? memory = null, List<BaseGoal>? goals = null, ActionCollection? actions = null, List<Sensor>? sensors = null, float costMaximum = float.MaxValue, int stepMaximum = int.MaxValue, NeighborLookupMode neighborLookupMode = NeighborLookupMode.Index) {
            Name = name ?? $"Agent {Guid.NewGuid()}";
            if (state != null) State = state;
            if (memory != null) Memory = memory;
            if (goals != null) Goals = goals;
            if (actions != null) Actions = actions;
            if (sensors != null) Sensors = sensors;
            CostMaximum = costMaximum;
            StepMaximum = stepMaximum;
            planner = new Planner(Actions, new ActionNodePool(), neighborLookupMode);
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

        /// <summary>
        /// Gets the template this agent was created from, or null for agents not vended by <see cref="AgentRegistry"/>.
        /// </summary>
        public AgentTemplate? Template { get; internal set; }

        /// <summary>
        /// Gets the chains of actions currently being performed by the agent.
        /// </summary>
        public IReadOnlyList<IActionPlan> CurrentActionSequences => actionSequences;

        /// <summary>
        /// Gets or sets the current world state from the agent perspective.
        /// </summary>
        public State State { get; set; } = new();

        /// <summary>
        /// Gets or sets the memory storage object for the agent.
        /// </summary>
        public Dictionary<string, object?> Memory { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of active goals for the agent.
        /// </summary>
        public List<BaseGoal> Goals { get; set; } = new();

        /// <summary>
        /// Gets or sets the actions available to the agent.
        /// </summary>
        public ActionCollection Actions { get; set; } = new();

        /// <summary>
        /// Gets or sets the sensors available to the agent.
        /// </summary>
        public List<Sensor> Sensors { get; set; } = new();

        /// <summary>
        /// Gets or sets the plan cost maximum for the agent.
        /// </summary>
        public float CostMaximum { get; set; }

        /// <summary>
        /// Gets or sets the step maximum for the agent.
        /// </summary>
        public int StepMaximum { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the agent is currently executing one or more actions.
        /// </summary>
        public bool IsBusy { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the agent is currently planning.
        /// </summary>
        public bool IsPlanning { get; set; } = false;

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
            if (!IsBusy) planner.Plan(this, CostMaximum, StepMaximum);
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
            State = CloneStateFromTemplate(template);
            Goals = new List<BaseGoal>(template.GoalsTemplate);
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
                planner.Plan(this, CostMaximum, StepMaximum);
            }
        }

        /// <summary>
        /// Makes a plan asynchronously.
        /// </summary>
        public void PlanAsync() {
            if (!IsBusy && !IsPlanning) {
                IsPlanning = true;
                var t = new Thread(new ThreadStart(() => { planner.Plan(this, CostMaximum, StepMaximum); }));
                t.Start();
            }
        }

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
        internal static void TriggerOnPlanningStartedForSingleGoal(Agent agent, BaseGoal goal) {
            OnPlanningStartedForSingleGoal(agent, goal);
        }

        /// <summary>
        /// Triggers OnPlanningFinishedForSingleGoal event.
        /// </summary>
        /// <param name="agent">Agent that finished planning.</param>
        /// <param name="goal">Goal for which planning was completed.</param>
        /// <param name="utility">Utility of the plan.</param>
        internal static void TriggerOnPlanningFinishedForSingleGoal(Agent agent, BaseGoal goal, float utility) {
            OnPlanningFinishedForSingleGoal(agent, goal, utility);
        }

        /// <summary>
        /// Triggers OnPlanningFinished event.
        /// </summary>
        /// <param name="agent">Agent that finished planning.</param>
        /// <param name="goal">Goal that was selected.</param>
        /// <param name="utility">Utility of the plan.</param>
        internal static void TriggerOnPlanningFinished(Agent agent, BaseGoal? goal, float utility) {
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

        private static State CloneStateFromTemplate(AgentTemplate template) {
            var s = new State();
            foreach (var kvp in template.StateTemplate) s.Set(kvp.Key, kvp.Value);
            return s;
        }

        /// <summary>
        /// Triggers OnEvaluatedActionNode event.
        /// </summary>
        /// <param name="node">Action node being evaluated.</param>
        /// <param name="nodes">List of nodes in the path that led to this point.</param>
        internal static void TriggerOnEvaluatedActionNode(ActionNode node, IReadOnlyDictionary<ActionNode, ActionNode> nodes) {
            OnEvaluatedActionNode(node, nodes);
        }

        /// <summary>
        /// Executes an asynchronous step of agent work.
        /// </summary>
        private void StepAsync() {
            if (!IsBusy && !IsPlanning) {
                IsPlanning = true;
                var t = new Thread(new ThreadStart(() => { planner.Plan(this, CostMaximum, StepMaximum); }));
                t.Start();
            }
            else if (!IsPlanning) Execute();
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
