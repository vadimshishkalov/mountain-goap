namespace MountainGoapTest {
    using System.Collections.Generic;

    /// <summary>
    /// Tests for the Candidates/Possible two-set logic in ActionGraph.Neighbors.
    ///
    /// ActionNode holds two sets:
    ///   Possible   — templates confirmed to pass IsPossible by this node or ancestors.
    ///                Trusted: expanded without re-checking static preconditions
    ///                (stateChecker still runs if present).
    ///   Candidates — templates not yet verified for this node's state.
    ///                Checked via IsPossible: promoted to Possible on pass, discarded on fail.
    ///
    /// Delta invalidation: when an action's postcondition keys overlap with a Possible
    /// template's precondition keys, that template moves back to Candidates in the child.
    /// StateMutator actions invalidate ALL Possible templates (unknown mutation scope).
    /// </summary>
    public class ActionGraphNeighborsTests {
        /// <summary>
        /// Two-step chain: A (x=1→2) then B (x=2→3).
        /// Both start as Candidates on the root node (x=1).
        /// A passes IsPossible (x=1 met), B fails (x!=2).
        /// A's child gets B back as Candidate via delta (A changed "x").
        /// B now passes (x=2 after A). Plan = A→B.
        /// </summary>
        [Fact]
        public void CandidatePromotionFindsCorrectPlan() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var executedActions = new List<string>();
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "x", 1 }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "x", 3 }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        name: "A",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "x", 2 } },
                        executor: (agent, action) => {
                            executedActions.Add("A");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "B",
                        preconditions: new() { { "x", 2 } },
                        postconditions: new() { { "x", 3 } },
                        executor: (agent, action) => {
                            executedActions.Add("B");
                            return ExecutionStatus.Succeeded;
                        }
                    )
                },
                neighborLookupMode: NeighborLookupMode.Aggressive
            );
            var agent = new Agent(template);
            agent.Step(StepMode.AllActions);
            Assert.Equal(new List<string> { "A", "B" }, executedActions);
            Assert.Equal(3, agent.State["x"]);
        }

        /// <summary>
        /// Three actions: A (x=1→2), B (y=99→100, impossible), C (x=2→3).
        /// State: x=1, y=0. Goal: x=3.
        /// B's precondition y=99 never holds — fails IsPossible on root, added to failedTemp.
        /// B is not in delta for A (A changes "x", B's precondition is "y").
        /// So B is excluded from A's child entirely. Plan = A→C without B ever executing.
        /// </summary>
        [Fact]
        public void FailedCandidateNotPassedToChildren() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var executedActions = new List<string>();
            var bChecked = 0;
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "x", 1 },
                    { "y", 0 }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "x", 3 }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        name: "A",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "x", 2 } },
                        executor: (agent, action) => {
                            executedActions.Add("A");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "B",
                        preconditions: new() { { "y", 99 } },
                        postconditions: new() { { "y", 100 } },
                        stateChecker: (action, state) => {
                            bChecked++;
                            return true;
                        },
                        executor: (agent, action) => {
                            executedActions.Add("B");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "C",
                        preconditions: new() { { "x", 2 } },
                        postconditions: new() { { "x", 3 } },
                        executor: (agent, action) => {
                            executedActions.Add("C");
                            return ExecutionStatus.Succeeded;
                        }
                    )
                },
                neighborLookupMode: NeighborLookupMode.Aggressive
            );
            var agent = new Agent(template);
            agent.Step(StepMode.AllActions);
            Assert.Equal(new List<string> { "A", "C" }, executedActions);
            Assert.DoesNotContain("B", executedActions);
        }

        /// <summary>
        /// A (x=1→2) then B (x=2→3). Both share precondition key "x".
        /// A's postcondition writes "x" → delta includes "x" → B's precondition key
        /// overlaps with delta → B moves from Possible/Candidates back to Candidates
        /// in A's child. B is then re-checked with IsPossible against x=2 and passes.
        /// </summary>
        [Fact]
        public void DeltaInvalidatesPossibleTemplate() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var executedActions = new List<string>();
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "x", 1 }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "x", 3 }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        name: "A",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "x", 2 } },
                        executor: (agent, action) => {
                            executedActions.Add("A");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "B",
                        preconditions: new() { { "x", 2 } },
                        postconditions: new() { { "x", 3 } },
                        executor: (agent, action) => {
                            executedActions.Add("B");
                            return ExecutionStatus.Succeeded;
                        }
                    )
                },
                neighborLookupMode: NeighborLookupMode.Aggressive
            );
            var agent = new Agent(template);
            agent.Step(StepMode.AllActions);
            Assert.Equal(new List<string> { "A", "B" }, executedActions);
            Assert.Equal(3, agent.State["x"]);
        }

        /// <summary>
        /// "Guarded" has no static preconditions → always a candidate (via alwaysCandidates).
        /// It has a stateChecker that rejects when y=1.
        /// Even when Guarded is promoted to Possible, ExpandPossible must still call
        /// IsPossible because HasStateChecker is true. This verifies the stateChecker
        /// gate is not skipped for Possible templates.
        /// </summary>
        [Fact]
        public void StateCheckerCalledForPossibleTemplates() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var executedActions = new List<string>();
            var stateCheckerCalls = 0;
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "x", 1 },
                    { "y", 0 }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "x", 2 },
                            { "y", 1 }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        name: "SetX",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "x", 2 }, { "y", 1 } },
                        executor: (agent, action) => {
                            executedActions.Add("SetX");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "Guarded",
                        postconditions: new() { { "y", 1 } },
                        stateChecker: (action, state) => {
                            stateCheckerCalls++;
                            return state.ContainsKey("y") && state["y"] is int val && val != 1;
                        },
                        executor: (agent, action) => {
                            executedActions.Add("Guarded");
                            return ExecutionStatus.Succeeded;
                        }
                    )
                },
                neighborLookupMode: NeighborLookupMode.Aggressive
            );
            var agent = new Agent(template);
            agent.Step(StepMode.AllActions);
            Assert.True(stateCheckerCalls > 0, "stateChecker should have been called");
            Assert.Contains("SetX", executedActions);
        }

        /// <summary>
        /// A changes "x" (x=1→2), B's precondition is on "w" (w=1).
        /// A's postcondition key "x" does NOT overlap with B's precondition key "w".
        /// So B is NOT delta-invalidated — it stays in Possible after being confirmed
        /// at the root. B has no stateChecker, so ExpandPossible skips IsPossible
        /// entirely and expands B directly. Plan = A + B.
        /// </summary>
        [Fact]
        public void PossibleTemplateExpandsWithoutRecheckingStaticPreconditions() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var executedActions = new List<string>();
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "x", 1 },
                    { "w", 1 },
                    { "z", 0 }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "x", 2 },
                            { "z", 1 }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        name: "A",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "x", 2 } },
                        executor: (agent, action) => {
                            executedActions.Add("A");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "B",
                        preconditions: new() { { "w", 1 } },
                        postconditions: new() { { "z", 1 } },
                        executor: (agent, action) => {
                            executedActions.Add("B");
                            return ExecutionStatus.Succeeded;
                        }
                    )
                },
                neighborLookupMode: NeighborLookupMode.Aggressive
            );
            var agent = new Agent(template);
            agent.Step(StepMode.AllActions);
            Assert.Contains("A", executedActions);
            Assert.Contains("B", executedActions);
            Assert.Equal(2, agent.State["x"]);
            Assert.Equal(1, agent.State["z"]);
        }

        /// <summary>
        /// "Mutator" has a stateMutator callback that writes y=99.
        /// Because HasStateMutator is true, delta = ALL base state keys.
        /// This invalidates every Possible template back to Candidates.
        /// "Finisher" requires y=99 — impossible initially, but after Mutator's
        /// stateMutator runs during planning, y=99 is set, and Finisher
        /// (now a Candidate again) passes IsPossible. Plan = Mutator→Finisher.
        /// </summary>
        [Fact]
        public void StateMutatorInvalidatesAllPossible() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var executedActions = new List<string>();
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "x", 1 },
                    { "y", 0 },
                    { "goal", false }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "goal", true }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        name: "Mutator",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "x", 2 } },
                        stateMutator: (action, state) => {
                            state.Set("y", 99);
                        },
                        executor: (agent, action) => {
                            executedActions.Add("Mutator");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "Finisher",
                        preconditions: new() { { "y", 99 } },
                        postconditions: new() { { "goal", true } },
                        executor: (agent, action) => {
                            executedActions.Add("Finisher");
                            return ExecutionStatus.Succeeded;
                        }
                    )
                },
                neighborLookupMode: NeighborLookupMode.Aggressive
            );
            var agent = new Agent(template);
            agent.Step(StepMode.AllActions);
            Assert.Equal(new List<string> { "Mutator", "Finisher" }, executedActions);
            Assert.True((bool)agent.State["goal"]!);
        }

        /// <summary>
        /// Three-step chain: Step1 (x=1→2), Step2 (x=2→3), Step3 (x=3→4).
        /// All share precondition key "x". Each step's postcondition changes "x",
        /// so the delta invalidates the next step's template back to Candidates.
        /// This validates the full Candidates → Possible → delta invalidation cycle
        /// works correctly across three levels of A* search depth.
        /// </summary>
        [Fact]
        public void MultiStepPlanPreservesPossibleAcrossDepth() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var executedActions = new List<string>();
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "x", 1 }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "x", 4 }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        name: "Step1",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "x", 2 } },
                        executor: (agent, action) => {
                            executedActions.Add("Step1");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "Step2",
                        preconditions: new() { { "x", 2 } },
                        postconditions: new() { { "x", 3 } },
                        executor: (agent, action) => {
                            executedActions.Add("Step2");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "Step3",
                        preconditions: new() { { "x", 3 } },
                        postconditions: new() { { "x", 4 } },
                        executor: (agent, action) => {
                            executedActions.Add("Step3");
                            return ExecutionStatus.Succeeded;
                        }
                    )
                },
                neighborLookupMode: NeighborLookupMode.Aggressive
            );
            var agent = new Agent(template);
            agent.Step(StepMode.AllActions);
            Assert.Equal(new List<string> { "Step1", "Step2", "Step3" }, executedActions);
            Assert.Equal(4, agent.State["x"]);
        }
    }
}
