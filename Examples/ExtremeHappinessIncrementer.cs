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
            var registry = new Registry();
            registry.RegisterAgent(
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
                    registry.RegisterAction(
                        name: "Seek Happiness",
                        executor: SeekHappinessAction,
                        preconditions: new() {
                            { "health", true }
                        },
                        arithmeticPostconditions: new() {
                            { "happiness", 1 }
                        }),
                    registry.RegisterAction(
                        name: "Seek Greater Happiness",
                        executor: SeekGreaterHappinessAction,
                        preconditions: new() {
                            { "health", true }
                        },
                        arithmeticPostconditions: new() {
                            { "happiness", 2 }
                        }),
                    registry.RegisterAction(
                        name: "Seek Health",
                        executor: SeekHealth,
                        postconditions: new() {
                            { "health", true }
                        }),
                });
            IAgent agent = registry.GetInstance("Happiness Agent");
            while (agent.State["happiness"] is int happiness && happiness != 10) {
                agent.Step();
                Console.WriteLine($"NEW HAPPINESS IS {agent.State["happiness"]}");
            }
        }

        private static ExecutionStatus SeekHappinessAction(IAgent agent, IAction action) {
            Console.WriteLine("Seeking happiness.");
            return ExecutionStatus.Succeeded;
        }

        private static ExecutionStatus SeekGreaterHappinessAction(IAgent agent, IAction action) {
            Console.WriteLine("Seeking even greater happiness.");
            return ExecutionStatus.Succeeded;
        }

        private static ExecutionStatus SeekHealth(IAgent agent, IAction action) {
            Console.WriteLine("Seeking health.");
            return ExecutionStatus.Succeeded;
        }
    }
}
