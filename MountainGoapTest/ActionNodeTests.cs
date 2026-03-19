namespace MountainGoapTest {
    using System.Collections.Generic;

    public class AgentTests {
        [Fact]
        public void ItHandlesInitialNullStateValuesCorrectly() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "key", null }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "key", "non-null value" }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        preconditions: new() {
                            { "key", null }
                        },
                        postconditions: new() {
                            { "key", "non-null value" }
                        },
                        executor: (IAgent agent, IAction action) => {
                            return ExecutionStatus.Succeeded;
                        }
                    )
                }
            );
            var agent = new Agent(template);
            agent.Step(StepMode.OneAction);
            Assert.NotNull(agent.State["key"]);
        }

        [Fact]
        public void ItHandlesNullGoalsCorrectly() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "key", "non-null value" }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "key", null }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        preconditions: new() {
                            { "key", "non-null value" }
                        },
                        postconditions: new() {
                            { "key", null }
                        },
                        executor: (IAgent agent, IAction action) => {
                            return ExecutionStatus.Succeeded;
                        }
                    )
                }
            );
            var agent = new Agent(template);
            agent.Step(StepMode.OneAction);
            Assert.Null(agent.State["key"]);
        }

        [Fact]
        public void ItHandlesNonNullStateValuesCorrectly() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "key", "value" }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "key", "new value" }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        preconditions: new() {
                            { "key", "value" }
                        },
                        postconditions: new() {
                            { "key", "new value" }
                        },
                        executor: (IAgent agent, IAction action) => {
                            return ExecutionStatus.Succeeded;
                        }
                    )
                }
            );
            var agent = new Agent(template);
            agent.Step(StepMode.OneAction);
            object? value = agent.State["key"];
            Assert.NotNull(value);
            if (value is not null) Assert.Equal("new value", (string)value);
        }

        [Fact]
        public void ItExecutesOneActionInOneActionStepMode() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var actionCount = 0;
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "key", "value" }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "key", "new value" }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        preconditions: new() {
                            { "key", "value" }
                        },
                        postconditions: new() {
                            { "key", "new value" }
                        },
                        executor: (IAgent agent, IAction action) => {
                            actionCount++;
                            return ExecutionStatus.Succeeded;
                        }
                    )
                }
            );
            var agent = new Agent(template);
            agent.Step(StepMode.OneAction);
            Assert.Equal(1, actionCount);
        }

        [Fact]
        public void ItExecutesAllActionsInAllActionsStepMode() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var actionCount = 0;
            var template = agentRegistry.RegisterAgent(
                name: "test",
                state: new() {
                    { "key", "value" }
                },
                goals: new List<BaseGoal> {
                    new Goal(
                        desiredState: new() {
                            { "key", "new value" }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        name: "Step 1",
                        preconditions: new() {
                            { "key", "value" }
                        },
                        postconditions: new() {
                            { "key", "intermediate value" }
                        },
                        executor: (IAgent agent, IAction action) => {
                            actionCount++;
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "Step 2",
                        preconditions: new() {
                            { "key", "intermediate value" }
                        },
                        postconditions: new() {
                            { "key", "new value" }
                        },
                        executor: (IAgent agent, IAction action) => {
                            actionCount++;
                            return ExecutionStatus.Succeeded;
                        }
                    )
                }
            );
            var agent = new Agent(template);
            agent.Step(StepMode.AllActions);
            Assert.Equal(2, actionCount);
        }
    }
}
