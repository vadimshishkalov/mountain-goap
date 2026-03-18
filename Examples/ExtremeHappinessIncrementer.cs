// <copyright file="ExtremeHappinessIncrementer.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace Examples {
    using MountainGoap;
    using MountainGoapLogging;

    /// <summary>
    /// Simple goal to maximize happiness.
    /// </summary>
    internal static class ExtremeHappinessIncrementer {
        /// <summary>
        /// Runs the demo.
        /// </summary>
        internal static void Run() {
            _ = new DefaultLogger();
            var actionRegistry = new ActionRegistry();
            var agentRegistry = new AgentRegistry();
            agentRegistry.RegisterAgent(
                name: "Happiness Agent",
                state: new() {
                    { "happiness", 0 },
                    { "health", false },
                },
                goals: new() {
                    new ExtremeGoal(
                        name: "Maximize Happiness",
                        desiredState: new() {
                            { "happiness", true }
                        })
                },
                actions: new() {
                    actionRegistry.RegisterAction(
                        name: "Seek Happiness",
                        executor: SeekHappinessAction,
                        preconditions: new() {
                            { "health", true }
                        },
                        arithmeticPostconditions: new() {
                            { "happiness", 1 }
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "Seek Greater Happiness",
                        executor: SeekGreaterHappinessAction,
                        preconditions: new() {
                            { "health", true }
                        },
                        arithmeticPostconditions: new() {
                            { "happiness", 2 }
                        }
                    ),
                    actionRegistry.RegisterAction(
                        name: "Seek Health",
                        executor: SeekHealth,
                        postconditions: new() {
                            { "health", true }
                        }
                    ),
                }
            );
            Agent agent = agentRegistry.GetInstance("Happiness Agent");
            while (agent.State["happiness"] is int happiness && happiness != 10) {
                agent.Step();
                Console.WriteLine($"NEW HAPPINESS IS {agent.State["happiness"]}");
            }
        }

        private static ExecutionStatus SeekHappinessAction(Agent agent, IAction action) {
            Console.WriteLine("Seeking happiness.");
            return ExecutionStatus.Succeeded;
        }

        private static ExecutionStatus SeekGreaterHappinessAction(Agent agent, IAction action) {
            Console.WriteLine("Seeking even greater happiness.");
            return ExecutionStatus.Succeeded;
        }

        private static ExecutionStatus SeekHealth(Agent agent, IAction action) {
            Console.WriteLine("Seeking health.");
            return ExecutionStatus.Succeeded;
        }
    }
}
