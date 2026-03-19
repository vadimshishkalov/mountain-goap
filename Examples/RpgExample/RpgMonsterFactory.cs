// <copyright file="RpgMonsterFactory.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace Examples {
    using System.Numerics;
    using MountainGoap;

    /// <summary>
    /// Class for generating an RPG monster.
    /// </summary>
    internal static class RpgMonsterFactory {
        private static readonly Random Rng = new();
        private static int counter = 1;

        /// <summary>
        /// Returns an RPG monster agent.
        /// </summary>
        /// <param name="agents">List of agents included in the world state.</param>
        /// <param name="foodPositions">List of positions for food in the world state.</param>
        /// <returns>An RPG character agent.</returns>
        internal static Agent Create(List<Agent> agents, List<Vector2> foodPositions) {
            var registry = new Registry();
            // TODO: Migrate to Registry.RegisterAgent / GetInstance.
            // The mutations below (State setup + Goals/Sensors/Actions.Add) must move into
            // RegisterAgent before this factory can use instance pooling.
            var agent = RpgCharacterFactory.Create(agents, $"Monster {counter++}");
            Goal eatFood = new(
                name: "Eat Food",
                weight: 0.1f,
                desiredState: new() {
                    { "eatingFood", true }
                }
            );
            Sensor seeFoodSensor = new(SeeFoodSensorHandler, "Food Sight Sensor");
            Sensor foodProximitySensor = new(FoodProximitySensorHandler, "Food Proximity Sensor");
            var lookForFood = registry.RegisterAction(
                name: "Look For Food",
                executor: LookForFoodExecutor,
                preconditions: new() {
                    { "canSeeFood", false },
                    { "canSeeEnemies", false }
                },
                postconditions: new() {
                    { "canSeeFood", true }
                }
            );
            var goToFood = registry.RegisterAction(
                name: "Go To Food",
                executor: GoToFoodExecutor,
                preconditions: new() {
                    { "canSeeFood", true },
                    { "canSeeEnemies", false }
                },
                postconditions: new() {
                    { "nearFood", true }
                },
                permutationSelectors: new() {
                    { "target", RpgUtils.FoodPermutations },
                    { "startingPosition", RpgUtils.StartingPositionPermutations }
                },
                costCallback: RpgUtils.GoToFoodCost
            );
            var eat = registry.RegisterAction(
                name: "Eat",
                executor: EatExecutor,
                preconditions: new() {
                    { "nearFood", true },
                    { "canSeeEnemies", false }
                },
                postconditions: new() {
                    { "eatingFood", true }
                }
            );
            // TODO: Move these initial-state assignments into the state template in RegisterAgent.
            agent.State["canSeeFood"] = false;
            agent.State["nearFood"] = false;
            agent.State["eatingFood"] = false;
            agent.State["foodPositions"] = foodPositions;
            agent.State["hp"] = 2;
            // TODO: Declare these in RegisterAgent (goals/sensors/actions) instead of mutating post-construction.
            // Goals/Sensors/Actions are now owned by AgentTemplate and are read-only on Agent.
            // agent.Goals.Add(eatFood);
            // agent.Sensors.Add(seeFoodSensor);
            // agent.Sensors.Add(foodProximitySensor);
            // agent.Actions.Add(goToFood);
            // agent.Actions.Add(lookForFood);
            // agent.Actions.Add(eat);
            return agent;
        }

        private static Vector2? GetFoodInRange(Vector2 source, List<Vector2> foodPositions, float range) {
            var output = foodPositions.FirstOrDefault((position) => RpgUtils.InDistance(source, position, range), new Vector2(-1, -1));
            if (output == new Vector2(-1, -1)) return null;
            return output;
        }

        private static void SeeFoodSensorHandler(IAgent agent) {
            if (agent.State["position"] is Vector2 agentPosition && agent.State["foodPositions"] is List<Vector2> foodPositions) {
                var foodPosition = GetFoodInRange(agentPosition, foodPositions, 5f);
                if (foodPosition != null) agent.State["canSeeFood"] = true;
                else {
                    agent.State["canSeeFood"] = false;
                    agent.State["eatingFood"] = false;
                }
            }
        }

        private static void FoodProximitySensorHandler(IAgent agent) {
            if (agent.State["position"] is Vector2 agentPosition && agent.State["foodPositions"] is List<Vector2> foodPositions) {
                var foodPosition = GetFoodInRange(agentPosition, foodPositions, 1f);
                if (foodPosition != null) agent.State["nearFood"] = true;
                else {
                    agent.State["nearFood"] = false;
                    agent.State["eatingFood"] = false;
                }
            }
        }

        private static ExecutionStatus LookForFoodExecutor(IAgent agent, IAction action) {
            if (agent.State["position"] is Vector2 position) {
                position.X += Rng.Next(-1, 2);
                position.X = Math.Clamp(position.X, 0, RpgExample.MaxX - 1);
                position.Y += Rng.Next(-1, 2);
                position.Y = Math.Clamp(position.Y, 0, RpgExample.MaxY - 1);
                agent.State["position"] = position;
            }
            if (agent.State["canSeeFood"] is bool canSeeFood && canSeeFood) return ExecutionStatus.Succeeded;
            return ExecutionStatus.Failed;
        }

        private static ExecutionStatus GoToFoodExecutor(IAgent agent, IAction action) {
            if (action.GetParameter("target") is Vector2 foodPosition && agent.State["position"] is Vector2 position) {
                position = RpgUtils.MoveTowardsOtherPosition(position, foodPosition);
                agent.State["position"] = position;
                if (RpgUtils.InDistance(position, foodPosition, 1f)) return ExecutionStatus.Succeeded;
            }
            return ExecutionStatus.Failed;
        }

        private static ExecutionStatus EatExecutor(IAgent agent, IAction action) {
            if (agent.State["foodPositions"] is List<Vector2> foodPositions && agent.State["position"] is Vector2 position) {
                var foodPosition = GetFoodInRange(position, foodPositions, 1f);
                if (foodPosition != null) {
                    foodPositions.Remove((Vector2)foodPosition);
                    return ExecutionStatus.Succeeded;
                }
            }
            return ExecutionStatus.Failed;
        }
    }
}
