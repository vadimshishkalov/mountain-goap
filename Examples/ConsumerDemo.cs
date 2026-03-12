// <copyright file="ConsumerDemo.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace Examples {
    using MountainGoap;
    using MountainGoapLogging;

    /// <summary>
    /// Goal to create enough food to eat by working and grocery shopping.
    /// </summary>
    internal static class ConsumerDemo {
        /// <summary>
        /// Runs the demo.
        /// </summary>
        internal static void Run() {
            _ = new DefaultLogger();
            var registry = new ActionRegistry();
            var locations = new List<string> { "home", "work", "store" };
            var agent = new Agent(
                name: "Consumer Agent",
                state: new() {
                    { "food", 0 },
                    { "energy", 100 },
                    { "money", 0 },
                    { "inCar", false },
                    { "location", "home" },
                    { "justTraveled", false }
                },
                goals: new() {
                    new ComparativeGoal(
                        name: "Get at least 5 food",
                        desiredState: new() {
                            {
                                "food", new() {
                                    Operator = ComparisonOperator.GreaterThanOrEquals,
                                    Value = 5
                                }
                            }
                        }),
                },
                actions: new() {
                    registry.RegisterAction(
                        name: "Walk",
                        cost: 6f,
                        executor: GenericExecutor,
                        preconditions: new() {
                            { "inCar", false }
                        },
                        permutationSelectors: new() {
                            { "location", PermutationSelectorGenerators.SelectFromCollection(locations) }
                        },
                        comparativePreconditions: new() {
                            { "energy", new() { Operator = ComparisonOperator.GreaterThan, Value = 0 } }
                        },
                        arithmeticPostconditions: new() {
                            { "energy", -1 }
                        },
                        parameterPostconditions: new() {
                            { "location", "location" }
                        }
                    ),
                    registry.RegisterAction(
                        name: "Drive",
                        cost: 1f,
                        preconditions: new() {
                            { "inCar", true },
                            { "justTraveled", false }
                        },
                        comparativePreconditions: new() {
                            { "energy", new() { Operator = ComparisonOperator.GreaterThan, Value = 0 } }
                        },
                        executor: GenericExecutor,
                        permutationSelectors: new() {
                            { "location", PermutationSelectorGenerators.SelectFromCollection(locations) }
                        },
                        arithmeticPostconditions: new() {
                            { "energy", -1 }
                        },
                        parameterPostconditions: new() {
                            { "location", "location" }
                        },
                        postconditions: new() {
                            { "justTraveled", true }
                        }
                    ),
                    registry.RegisterAction(
                        name: "Get in car",
                        cost: 1f,
                        preconditions: new() {
                            { "inCar", false },
                            { "justTraveled", false }
                        },
                        comparativePreconditions: new() {
                            { "energy", new() { Operator = ComparisonOperator.GreaterThan, Value = 0 } }
                        },
                        postconditions: new() {
                            { "inCar", true }
                        },
                        arithmeticPostconditions: new() {
                            { "energy", -1 }
                        },
                        executor: GenericExecutor
                    ),
                    registry.RegisterAction(
                        name: "Get out of car",
                        cost: 1f,
                        preconditions: new() {
                            { "inCar", true }
                        },
                        comparativePreconditions: new() {
                            { "energy", new() { Operator = ComparisonOperator.GreaterThan, Value = 0 } }
                        },
                        postconditions: new() {
                            { "inCar", false }
                        },
                        arithmeticPostconditions: new() {
                            { "energy", -1 }
                        },
                        executor: GenericExecutor
                    ),
                    registry.RegisterAction(
                        name: "Work",
                        cost: 1f,
                        preconditions: new() {
                            { "location", "work" },
                            { "inCar", false }
                        },
                        comparativePreconditions: new() {
                            { "energy", new() { Operator = ComparisonOperator.GreaterThan, Value = 0 } }
                        },
                        arithmeticPostconditions: new() {
                            { "energy", -1 },
                            { "money", 1 }
                        },
                        postconditions: new() {
                            { "justTraveled", false }
                        },
                        executor: GenericExecutor
                    ),
                    registry.RegisterAction(
                        name: "Shop",
                        cost: 1f,
                        preconditions: new() {
                            { "location", "store" },
                            { "inCar", false }
                        },
                        comparativePreconditions: new() {
                            { "energy", new() { Operator = ComparisonOperator.GreaterThan, Value = 0 } },
                            { "money", new() { Operator = ComparisonOperator.GreaterThan, Value = 0 } }
                        },
                        arithmeticPostconditions: new() {
                            { "energy", -1 },
                            { "money", -1 },
                            { "food", 1 }
                        },
                        postconditions: new() {
                            { "justTraveled", false }
                        },
                        executor: GenericExecutor
                    )
                });
            while (agent.State["food"] is int food && food < 5) agent.Step();
        }

        private static ExecutionStatus GenericExecutor(Agent agent, IAction action) {
            return ExecutionStatus.Succeeded;
        }
    }
}
