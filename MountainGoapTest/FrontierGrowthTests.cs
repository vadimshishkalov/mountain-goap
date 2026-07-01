namespace MountainGoapTest {
    using System.Collections.Generic;

    /// <summary>
    /// Tests for the configurable + auto-growing A* frontier (open-set) priority queue.
    ///
    /// <see cref="AgentConfiguration.FrontierInitialCapacity"/> sets the queue's starting size.
    /// When a search would enqueue more concurrently-open nodes than the current capacity, the
    /// queue grows by 2x (handled in ActionAStar via the queue's Resize/MaxSize/Count API).
    /// </summary>
    public class FrontierGrowthTests {
        /// <summary>
        /// With an intentionally tiny FrontierInitialCapacity of 2, expanding the root state yields
        /// five simultaneously-open neighbours (A + four decoys, all applicable at x=1), forcing the
        /// frontier to grow past its initial capacity. The search must still find the correct plan
        /// A→B (x:1→2→3), proving growth preserves heap contents and correctness. Because tests run
        /// in Debug, this also exercises the queue's #if DEBUG "full" guard: the pre-Enqueue resize
        /// must keep it from ever tripping.
        /// </summary>
        [Fact]
        public void FrontierGrowsBeyondInitialCapacityAndStillPlans() {
            var registry = new Registry();
            var executedActions = new List<string>();
            var template = registry.RegisterAgent(
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
                    registry.RegisterAction(
                        name: "A",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "x", 2 } },
                        executor: (agent, action) => {
                            executedActions.Add("A");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    registry.RegisterAction(
                        name: "B",
                        preconditions: new() { { "x", 2 } },
                        postconditions: new() { { "x", 3 } },
                        executor: (agent, action) => {
                            executedActions.Add("B");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    // Four decoy actions, all applicable at x=1, each writing a distinct key.
                    // They widen the frontier at the first expansion but never reach the goal.
                    registry.RegisterAction(
                        name: "D1",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "d1", 1 } },
                        executor: (agent, action) => {
                            executedActions.Add("D1");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    registry.RegisterAction(
                        name: "D2",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "d2", 1 } },
                        executor: (agent, action) => {
                            executedActions.Add("D2");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    registry.RegisterAction(
                        name: "D3",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "d3", 1 } },
                        executor: (agent, action) => {
                            executedActions.Add("D3");
                            return ExecutionStatus.Succeeded;
                        }
                    ),
                    registry.RegisterAction(
                        name: "D4",
                        preconditions: new() { { "x", 1 } },
                        postconditions: new() { { "d4", 1 } },
                        executor: (agent, action) => {
                            executedActions.Add("D4");
                            return ExecutionStatus.Succeeded;
                        }
                    )
                },
                configuration: new AgentConfiguration { FrontierInitialCapacity = 2 }
            );
            var agent = new Agent(template);
            agent.Step(StepMode.AllActions);
            Assert.Equal(new List<string> { "A", "B" }, executedActions);
            Assert.Equal(3, agent.State["x"]);
        }

        /// <summary>
        /// The default frontier capacity is 1024 when not overridden.
        /// </summary>
        [Fact]
        public void DefaultFrontierInitialCapacityIs1024() {
            Assert.Equal(1024, new AgentConfiguration().FrontierInitialCapacity);
        }
    }
}
