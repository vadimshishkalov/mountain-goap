namespace MountainGoapTest {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class PlannerWorkerPoolTests {
        private static Agent BuildPlanningAgent(Registry registry) {
            var template = registry.RegisterAgent(
                name: "test-" + Guid.NewGuid(),
                state: new() { { "done", false } },
                goals: new List<BaseGoal> {
                    new Goal(desiredState: new() { { "done", true } })
                },
                actions: new ActionCollection {
                    registry.RegisterAction(
                        preconditions: new() { { "done", false } },
                        postconditions: new() { { "done", true } },
                        executor: (IAgent agent, IAction action) => ExecutionStatus.Succeeded
                    )
                }
            );
            return new Agent(template);
        }

        private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 3000) {
            var sw = Stopwatch.StartNew();
            while (!condition() && sw.ElapsedMilliseconds < timeoutMs)
                await Task.Delay(10);
            if (!condition()) throw new TimeoutException("Condition not met within timeout.");
        }

        [Fact]
        public async Task DefaultPoolCompletesPlanning() {
            var agent = BuildPlanningAgent(new Registry());

            agent.PlanAsync();

            await WaitUntilAsync(() => !agent.IsPlanning);
            Assert.True(agent.IsBusy);
        }

        [Fact]
        public async Task DefaultPoolPlansMultipleAgentsConcurrently() {
            var agents = new Agent[8];
            for (int i = 0; i < agents.Length; i++)
                agents[i] = BuildPlanningAgent(new Registry());

            foreach (var a in agents) a.PlanAsync();

            var tasks = new Task[agents.Length];
            for (int i = 0; i < agents.Length; i++) {
                var a = agents[i];
                tasks[i] = WaitUntilAsync(() => !a.IsPlanning);
            }
            await Task.WhenAll(tasks);

            foreach (var a in agents)
                Assert.True(a.IsBusy);
        }

        [Fact]
        public async Task IsPlanningGuardPreventsDoubleEnqueue() {
            var agent = BuildPlanningAgent(new Registry());
            int planStartCount = 0;

            Agent.OnPlanningStarted += CountPlan;
            void CountPlan(IReadOnlyAgent a) { if (ReferenceEquals(a, agent)) planStartCount++; }

            try {
                agent.PlanAsync();
                agent.PlanAsync();

                await WaitUntilAsync(() => !agent.IsPlanning);

                Assert.Equal(1, planStartCount);
            }
            finally {
                Agent.OnPlanningStarted -= CountPlan;
            }
        }
    }
}
