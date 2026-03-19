namespace MountainGoapTest {
    using System.Collections.Generic;

    public class PermutationSelectorTests {
        [Fact]
        public void ItSelectsFromADynamicallyGeneratedCollectionInState() {
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            var collection = new List<int> { 1, 2, 3 };
            var selector = PermutationSelectorGenerators.SelectFromCollectionInState<int>("collection");
            var template = agentRegistry.RegisterAgent(
                name: "sample agent",
                state: new() {
                    { "collection", collection },
                    { "goalAchieved", false }
                },
                goals: new() {
                    new Goal(
                        name: "sample goal",
                        desiredState: new Dictionary<string, object?> {
                            { "goalAchieved", true }
                        }
                    )
                },
                actions: new ActionCollection {
                    actionRegistry.RegisterAction(
                        name: "sample action",
                        cost: 1f,
                        preconditions: new() {
                            { "goalAchieved", false }
                        },
                        postconditions: new() {
                            { "goalAchieved", true }
                        },
                        executor: (agent, action) => { return ExecutionStatus.Succeeded; }
                    )
                },
                sensors: new() {
                    new(
                        (agent) => {
                            if (agent.State["collection"] is List<int> collection) {
                                collection.Add(4);
                            }
                        },
                        name: "sample sensor"
                    )
                }
            );
            var agent = new Agent(template);
            IReadOnlyList<object> permutations = selector(agent.State);
            Assert.Equal(3, permutations.Count);
            agent.Step(StepMode.OneAction);
            permutations = selector(agent.State);
            Assert.Equal(4, permutations.Count);
            agent.Step(StepMode.OneAction);
            permutations = selector(agent.State);
            Assert.Equal(5, permutations.Count);
        }
    }
}
