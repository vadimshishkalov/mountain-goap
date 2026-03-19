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
        /// <param name="position">Starting position of the monster.</param>
        /// <returns>An RPG monster agent.</returns>
        internal static Agent Create(List<Agent> agents, List<Vector2> foodPositions, Vector2 position) {
            var registry = new Registry();
            Goal removeEnemies = new(
                name: "Remove Enemies",
                weight: 1f,
                desiredState: new() {
                    { "canSeeEnemies", false }
                }
            );
            Goal eatFood = new(
                name: "Eat Food",
                weight: 0.1f,
                desiredState: new() {
                    { "eatingFood", true }
                }
            );
            Sensor seeEnemiesSensor = new(SeeEnemiesSensorHandler, "Enemy Sight Sensor");
            Sensor enemyProximitySensor = new(EnemyProximitySensorHandler, "Enemy Proximity Sensor");
            Sensor seeFoodSensor = new(SeeFoodSensorHandler, "Food Sight Sensor");
            Sensor foodProximitySensor = new(FoodProximitySensorHandler, "Food Proximity Sensor");
            ActionCollection actions = new() {
                registry.RegisterAction(
                    name: "Go To Enemy",
                    executor: GoToEnemyExecutor,
                    preconditions: new() {
                        { "canSeeEnemies", true },
                        { "nearEnemy", false }
                    },
                    postconditions: new() {
                        { "nearEnemy", true }
                    },
                    permutationSelectors: new() {
                        { "target", RpgUtils.EnemyPermutations },
                        { "startingPosition", RpgUtils.StartingPositionPermutations }
                    },
                    costCallback: RpgUtils.GoToEnemyCost),
                registry.RegisterAction(
                    name: "Kill Nearby Enemy",
                    executor: KillNearbyEnemyExecutor,
                    preconditions: new() {
                        { "nearEnemy", true }
                    },
                    postconditions: new() {
                        { "canSeeEnemies", false },
                        { "nearEnemy", false }
                    }),
                registry.RegisterAction(
                    name: "Look For Food",
                    executor: LookForFoodExecutor,
                    preconditions: new() {
                        { "canSeeFood", false },
                        { "canSeeEnemies", false }
                    },
                    postconditions: new() {
                        { "canSeeFood", true }
                    }),
                registry.RegisterAction(
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
                    costCallback: RpgUtils.GoToFoodCost),
                registry.RegisterAction(
                    name: "Eat",
                    executor: EatExecutor,
                    preconditions: new() {
                        { "nearFood", true },
                        { "canSeeEnemies", false }
                    },
                    postconditions: new() {
                        { "eatingFood", true }
                    }),
            };
            var template = registry.RegisterAgent(
                name: $"Monster {counter++}",
                state: new() {
                    { "canSeeEnemies", false },
                    { "nearEnemy", false },
                    { "hp", 2 },
                    { "position", position },
                    { "faction", "enemy" },
                    { "agents", agents },
                    { "canSeeFood", false },
                    { "nearFood", false },
                    { "eatingFood", false },
                    { "foodPositions", foodPositions }
                },
                goals: new() {
                    removeEnemies,
                    eatFood
                },
                sensors: new() {
                    seeEnemiesSensor,
                    enemyProximitySensor,
                    seeFoodSensor,
                    foodProximitySensor
                },
                actions: actions);
            return new Agent(template);
        }

        private static Vector2? GetFoodInRange(Vector2 source, List<Vector2> foodPositions, float range) {
            var output = foodPositions.FirstOrDefault((position) => RpgUtils.InDistance(source, position, range), new Vector2(-1, -1));
            if (output == new Vector2(-1, -1)) return null;
            return output;
        }

        private static void SeeEnemiesSensorHandler(IAgent agent) {
            if (agent.State["agents"] is List<Agent> agents) {
                var agent2 = RpgUtils.GetEnemyInRange(agent, agents, 5f);
                if (agent2 != null) agent.State.Set("canSeeEnemies", true);
                else agent.State.Set("canSeeEnemies", false);
            }
        }

        private static void EnemyProximitySensorHandler(IAgent agent) {
            if (agent.State["agents"] is List<Agent> agents) {
                var agent2 = RpgUtils.GetEnemyInRange(agent, agents, 1f);
                if (agent2 != null) agent.State.Set("nearEnemy", true);
                else agent.State.Set("nearEnemy", false);
            }
        }

        private static void SeeFoodSensorHandler(IAgent agent) {
            if (agent.State["position"] is Vector2 agentPosition && agent.State["foodPositions"] is List<Vector2> foodPositions) {
                var foodPosition = GetFoodInRange(agentPosition, foodPositions, 5f);
                if (foodPosition != null) agent.State.Set("canSeeFood", true);
                else {
                    agent.State.Set("canSeeFood", false);
                    agent.State.Set("eatingFood", false);
                }
            }
        }

        private static void FoodProximitySensorHandler(IAgent agent) {
            if (agent.State["position"] is Vector2 agentPosition && agent.State["foodPositions"] is List<Vector2> foodPositions) {
                var foodPosition = GetFoodInRange(agentPosition, foodPositions, 1f);
                if (foodPosition != null) agent.State.Set("nearFood", true);
                else {
                    agent.State.Set("nearFood", false);
                    agent.State.Set("eatingFood", false);
                }
            }
        }

        private static ExecutionStatus KillNearbyEnemyExecutor(IAgent agent, IAction action) {
            if (agent.State["agents"] is List<Agent> agents) {
                var agent2 = RpgUtils.GetEnemyInRange(agent, agents, 1f);
                if (agent2 != null && agent2.State["hp"] is int hp) {
                    hp--;
                    agent2.State["hp"] = hp;
                    if (hp <= 0) return ExecutionStatus.Succeeded;
                }
            }
            return ExecutionStatus.Failed;
        }

        private static ExecutionStatus GoToEnemyExecutor(IAgent agent, IAction action) {
            if (action.GetParameter("target") is not Agent target) return ExecutionStatus.Failed;
            if (agent.State["position"] is Vector2 pos1 && target.State["position"] is Vector2 pos2) {
                var newPos = RpgUtils.MoveTowardsOtherPosition(pos1, pos2);
                agent.State.Set("position", newPos);
                if (RpgUtils.InDistance(newPos, pos2, 1f)) return ExecutionStatus.Succeeded;
            }
            return ExecutionStatus.Failed;
        }

        private static ExecutionStatus LookForFoodExecutor(IAgent agent, IAction action) {
            if (agent.State["position"] is Vector2 position) {
                position.X += Rng.Next(-1, 2);
                position.X = Math.Clamp(position.X, 0, RpgExample.MaxX - 1);
                position.Y += Rng.Next(-1, 2);
                position.Y = Math.Clamp(position.Y, 0, RpgExample.MaxY - 1);
                agent.State.Set("position", position);
            }
            if (agent.State["canSeeFood"] is bool canSeeFood && canSeeFood) return ExecutionStatus.Succeeded;
            return ExecutionStatus.Failed;
        }

        private static ExecutionStatus GoToFoodExecutor(IAgent agent, IAction action) {
            if (action.GetParameter("target") is Vector2 foodPosition && agent.State["position"] is Vector2 position) {
                position = RpgUtils.MoveTowardsOtherPosition(position, foodPosition);
                agent.State.Set("position", position);
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
