// <copyright file="ArithmeticHappinessIncrementer.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace Examples {
    using MountainGoap;
    using MountainGoapLogging;

    /// <summary>
    /// Simple goal to maximize happiness.
    /// </summary>
    internal static class ArithmeticHappinessIncrementer {
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
                },
                goals: new() {
                    new Goal(
                        name: "Maximize Happiness",
                        desiredState: new() {
                            { "happiness", 10 }
                        })
                },
                actions: new() {
                    registry.RegisterAction(
                        name: "Seek Happiness",
                        executor: SeekHappinessAction,
                        arithmeticPostconditions: new() {
                            { "happiness", 1 }
                        }
                    ),
                    registry.RegisterAction(
                        name: "Seek Greater Happiness",
                        executor: SeekGreaterHappinessAction,
                        arithmeticPostconditions: new() {
                            { "happiness", 2 }
                        }
                    )
                }
            );
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
    }
}
