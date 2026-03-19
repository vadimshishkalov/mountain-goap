using System.Collections.Generic;

namespace MountainGoapTest
{
    public class ArithmeticPostconditionsTests
    {
        [Fact]
        public void MinimalExampleTest()
        {
            var actionRegistry = new MountainGoap.ActionRegistry();
            var agentRegistry = new MountainGoap.AgentRegistry();

            List<BaseGoal> goals = new() {
                new ComparativeGoal(
                    name: "Goal1",
                    desiredState: new() {
                        { "i", new ComparisonValuePair {
                            Value = 100,
                            Operator = ComparisonOperator.GreaterThan
                        } }
                    },
                    weight: 1f
                ),
            };

            ActionCollection actions = new() {
                actionRegistry.RegisterAction(
                    name: "Action1",
                    executor: (MountainGoap.IAgent agent, MountainGoap.IAction action) => {
                        return ExecutionStatus.Succeeded;
                    },
                    arithmeticPostconditions: new Dictionary<string, object> {
                        { "i", 10 }
                    },
                    cost: 0.5f
                ),
            };

            var template = agentRegistry.RegisterAgent(
                name: "test",
                goals: goals,
                actions: actions,
                state: new() {
                    { "i", 0 }
                }
            );

            Agent agent = new(template);

            agent.Step(StepMode.OneAction);
            Assert.Equal(10, agent.State["i"]);
            agent.Step(StepMode.OneAction);
            Assert.Equal(20, agent.State["i"]);
        }
    }
}
