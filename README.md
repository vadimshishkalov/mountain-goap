<img src="https://github.com/caesuric/mountain-goap/raw/main/logo.png" alt="Mountain GOAP logo" title="Mountain GOAP" align="right" height="180" />

# Mountain GOAP

Generic C# GOAP (Goal Oriented Action Planning) library for creating AI agents to be used in games. GOAP is a type of an AI system for games popularized by [the F.E.A.R. AI paper](https://alumni.media.mit.edu/~jorkin/gdc2006_orkin_jeff_fear.pdf). GOAP agents use A\* pathfinding to plan paths through a series of sequential actions, creating action sequences that allow the agent to achieve its goals.

Mountain GOAP favors composition over inheritance, allowing you to create agents from a series of callbacks. In addition, Mountain GOAP's agents support multiple weighted goals and will attempt to find the greatest utility among a series of goals.

1. [What is this fork for](#what-is-this-fork-for)
2. [API migration from upstream](#api-migration-from-upstream)
3. [Quickstart](#quickstart)
    1. [Using distributable](#using-distributable)
    2. [Using distributable in Unity](#using-distributable-in-unity)
    3. [Using NuGet package](#using-nuget-package)
    4. [Using as a Unity package](#using-as-a-unity-package)
    5. [Using the code directly](#using-the-code-directly)
    6. [Using the library after installation](#using-the-library-after-installation)
4. [Concepts & API](#concepts--api)
    1. [Agents](#agents)
        1. [Agent state](#agent-state)
    2. [Goals](#goals)
        1. [Extreme Goals](#extreme-goals)
        2. [Comparative Goals](#comparative-goals)
    3. [Actions](#actions)
        1. [Comparative Preconditions](#comparative-preconditions)
        2. [Arithmetic Postconditions](#arithmetic-postconditions)
        3. [Parameter Postconditions](#parameter-postconditions)
    4. [Sensors](#sensors)
    5. [Permutation selectors](#permutation-selectors)
    6. [Cost callbacks](#cost-callbacks)
    7. [State mutators](#state-mutators)
    8. [State checkers](#state-checkers)
    9. [Full API Docs](#full-api-docs)
5. [Events](#events)
    1. [Agent events](#agent-events)
    2. [Action events](#action-events)
    3. [Sensor events](#sensor-events)
6. [Logger](#logger)
7. [Examples](#examples)
8. [Project structure](#project-structure)
9. [Roadmap](#roadmap)
10. [Other open source GOAP projects](#other-open-source-goap-projects)
11. [License Acknowledgements](#license-acknowledgements)

## What Is This Fork For

This fork optimizes Mountain GOAP for long-running, high-throughput environments (game servers, simulations) where GC pauses and per-frame allocations are unacceptable.

Key changes from upstream:

- **Delta-based planning state** — During A* expansion, nodes share a read-only base snapshot and store only their changes, avoiding full-state copies at every step.
- **Pipeline-wide object pooling** — All transient objects created during planning — nodes, graphs, plans, state layers — are pooled and reused across planning passes, eliminating per-cycle allocations.
- **Indexed action lookup** — Actions are indexed by the state keys they touch, so candidate filtering is a key lookup instead of a linear scan.
- **Design-time / runtime separation** — Actions and agents are split into immutable templates (defined once, shared) and lightweight runtime instances (carrying only mutable state), so configuration is never duplicated and instances can be pooled.
- **Dedicated planner worker pool** — Planning runs on a fixed-size worker pool, keeping action execution on the caller thread without blocking.

## API Migration from Upstream

### Agent and action creation

Agents and actions are now created through a `Registry` instead of direct constructors.

**Before (upstream):**

```csharp
Action seekHappiness = new(
    name: "Seek Happiness",
    executor: SeekHappinessAction,
    preconditions: new() { { "happinessRecentlyIncreased", false } },
    postconditions: new() { { "happinessRecentlyIncreased", true } });

Agent agent = new(
    name: "Happiness Agent",
    state: new() { { "happiness", 0 } },
    goals: new() { goal },
    actions: new() { seekHappiness });
```

**After (this fork):**

```csharp
var registry = new Registry();

var seekHappiness = registry.RegisterAction(
    name: "Seek Happiness",
    executor: SeekHappinessAction,
    preconditions: new() { { "happinessRecentlyIncreased", false } },
    postconditions: new() { { "happinessRecentlyIncreased", true } });

registry.RegisterAgent(
    name: "Happiness Agent",
    state: new() { { "happiness", 0 } },
    goals: new() { goal },
    actions: new() { seekHappiness });

IAgent agent = registry.GetInstance("Happiness Agent");
```

Actions registered with the same name are deduplicated. Agent instances can be returned to the pool with `registry.ReturnInstance(agent)`.

### Callback signatures

All callbacks now receive interfaces instead of concrete types:

| Callback | Before | After |
|---|---|---|
| `ExecutorCallback` | `(Agent, Action)` | `(IAgent, IAction)` |
| `CostCallback` | `(Action, ConcurrentDictionary<string, object?>)` | `(IReadOnlyAction, IReadOnlyState)` |
| `SensorRunCallback` | `(Agent)` | `(IAgent)` |
| `StateMutatorCallback` | `(Action, ConcurrentDictionary<string, object?>)` | `(IReadOnlyAction, IState)` |
| `StateCheckerCallback` | `(Action, ConcurrentDictionary<string, object?>)` | `(IReadOnlyAction, IReadOnlyState)` |
| `PermutationSelectorCallback` | `(ConcurrentDictionary<string, object?>)` | `(IReadOnlyState)` |
| `StateCostDeltaMultiplierCallback` | `(Action, string)` | `(IReadOnlyAction?, string)` |

Many properties and collections that were previously mutable are now exposed through read-only interfaces (`IReadOnlyAgent`, `IReadOnlyAction`, `IReadOnlyState`, `IReadOnlyGoal`). This is intentional — there is no mutable equivalent for these access paths. Code that previously modified agent or action fields directly must be restructured to work within the designated write points (executors, sensors, state mutators).

### State access

Agent state is no longer a raw `ConcurrentDictionary`. Reading works the same way via indexer; writing uses `Set()`:

```csharp
// read (unchanged)
var value = agent.State["key"];

// write
agent.State.Set("key", value);   // new
```

### Pooling

Object pooling is always active — each agent owns its own pools by default. Pass a shared `PoolManager` to the `Registry` to let multiple agents draw from the same pools:

```csharp
var pool = new PoolManager();
var registry = new Registry(poolManager: pool);
```

### Async planning (optional)

`Step(StepMode.Default)` now offloads planning to a background worker pool. For synchronous planning, use `agent.Plan()` or `Step(StepMode.OneAction)`.

## Quickstart

### Using distributable

Download the [release](https://github.com/caesuric/mountain-goap/releases), unzip, and include the DLL in your project. In Visual Studio, you can do this by right-clicking on "Dependencies" in the Solution Explorer, then clicking "Add COM Reference," clicking "Browse," and browsing to the DLL.

### Using distributable in Unity

Download the [release](https://github.com/caesuric/mountain-goap/releases), unzip to a folder, and drag the folder into your Unity project.

### Using NuGet package

If you are not using Unity, you can download and use MountainGoap as a NuGet package.

Right click your package and click "Manage NuGet Packages," then search for "MountainGoap" and install the package.

### Using as a Unity package

In the works.

### Using the code directly

Clone the repo and copy the code in the MountainGoap folder to your repo.

### Using the library after installation

No matter which method of installation you use, you can access MountainGoap by using the `MountainGoap` namespace as a prefix to the library classes, or by including the following line in your code:

```csharp
using MountainGoap;
```

## Concepts & API

### Agents

**Agents** are individual entities that act within your game or simulated world. Agents are created through a `Registry`, which manages templates and pooling:

```csharp
var registry = new Registry();
registry.RegisterAgent(name: "My Agent");
IAgent agent = registry.GetInstance("My Agent");
```

In practice, you will want to pass `RegisterAgent` various things it needs to make a functional agent. Read on to understand what kinds of objects you should pass your agent templates.

When you want your agent to act, just call the following:
`agent.Step();`

What kind of timeframe is represented by a "step" will vary based on your engine. In a turn based game, a step might be one turn. In a realtime engine like Unity, you might call `agent.Step()` on every `Update()` cycle. In a turn-based game you will probably want to call `agent.Step()` on every turn. In the case of turn-based games, you can force at least one action to be taken per turn by calling `agent.Step(mode: StepMode.OneAction)`. If you want the entire action sequence to be completed in one turn, you can call `agent.Step(mode: StepMode.AllActions)`.

For additional granularity, you can also call the functions `agent.Plan()`, `agent.ClearPlan()`, `agent.PlanAsync()`, and `agent.ExecutePlan()`. See the full API docs for additional details.

#### Agent state

The agent stores a dictionary of objects called its **state**. This state can include anything, but simple values work best with [goals](#goals) and [actions](#actions). If you need to reference complex game state, however, this is not a problem -- [sensors](#sensors), covered below, can be used to translate complex values like map states into simpler ones, like booleans. More on that below.

State can be passed when registering an agent template, like so:

```csharp
var registry = new Registry();
registry.RegisterAgent(
    name: "My Agent",
    state: new() {
        { "nearOtherAgent", false },
        { "otherAgents", new List<Agent>() }
    });
IAgent agent = registry.GetInstance("My Agent");
```

### Goals

**Goals** dictate the state values that the agent is trying to achieve. Goals have relatively simple constructors, taking just a dictionary of keys and values the agent wants to see in its state and a weight that indicates how important the goal is. The higher the weight, the more important the goal.

Goals can be passed when registering an agent template, like so:

```csharp
Goal goal = new Goal(
    desiredState: new Dictionary<string, object> {
        { "nearOtherAgent", true }
    },
    weight: 2f
);
var registry = new Registry();
registry.RegisterAgent(
    name: "My Agent",
    goals: new() { goal });
IAgent agent = registry.GetInstance("My Agent");
```

#### Extreme Goals

**Extreme goals** attempt to maximize or minimize a numeric state value. They take similar parameters to normal goals, but the values in the dictionary must be booleans. If the boolean is true, the goal will attempt to maximize the value. If the boolean is false, the goal will attempt to minimize the value. The state value must be a numeric type for this to work correctly.

Example that will try to maximize the agent's health:

```csharp
ExtremeGoal goal = new ExtremeGoal(
    desiredState: new Dictionary<string, object> {
        { "health", true }
    },
    weight: 2f
);
```

#### Comparative Goals

**Comparative goals** attempt to make a numeric state value compare in a certain way to a base value. They take similar parameters to normal goals, but the values in the dictionary must be ComparisonValuePair objects. The ComparisonValuePair object specifies a value to use for comparison and a comparison operator to use. The state value must be a numeric type and the same type as the comparison value for this to work correctly.

Example that will try to make the agent's health greater than 50:

```csharp
ComparativeGoal goal = new ComparativeGoal(
    desiredState: new Dictionary<string, object> {
        { "health", new ComparisonValuePair {
            Value = 50,
            Operator = ComparisonOperator.GreaterThan
         } }
    },
    weight: 2f
);
```

### Actions

**Actions** dictate arbitrary code the agent can execute to affect the world and achieve its goals. Each action, when it runs, will execute the code passed to it, which is called the action **executor**. Actions can also have **preconditions**, state values required before the agent is allowed to execute the action, and **postconditions**, which are values the state is expected to hold if the action is successful. Finally, each action has a **cost**, which is used in calculating the best plan for the agent.

Actions return an `ExecutionStatus` enum to say if they succeeded or not. If they succeed, the postconditions will automatically be set to the values passed to the action constructor. If the `ExecutionStatus` returned is `ExecutionStatus.Executing`, the action will be considered in progress, and the executor will be called again on the next Step command.

Actions are registered through the `Registry` and passed when registering an agent template, like so:

```csharp
var registry = new Registry();
var giveHugAction = registry.RegisterAction(
    name: "Give Hug",
    executor: (IAgent agent, IAction action) => {
        Console.WriteLine("hugged someone");
        return ExecutionStatus.Succeeded;
    },
    preconditions: new() {
        { "nearOtherAgent", true }
    },
    postconditions: new() {
        { "wasHugged", true }
    },
    cost: 0.5f
);
registry.RegisterAgent(
    name: "My Agent",
    actions: new() { giveHugAction });
IAgent agent = registry.GetInstance("My Agent");
```

#### Comparative Preconditions

**Comparative preconditions** are preconditions that are calculated by comparing a state value to a target value. They take similar parameters to normal preconditions, but the values in the dictionary must be ComparisonValuePair objects.

Example where energy must be greater than zero to walk:

```csharp
var walk = registry.RegisterAction(
    name: "Walk",
    executor: (IAgent agent, IAction action) => {
        Console.WriteLine("I'm walkin' here!");
        return ExecutionStatus.Succeeded;
    },
    comparativePreconditions: new() {
        { "energy", new() { Operator = ComparisonOperator.GreaterThan, Value = 0 } }
    });
```

#### Arithmetic Postconditions

**Arithmetic postconditions** are postconditions that are calculated by performing arithmetic on other state values. They take similar parameters to normal postconditions, but the values in the dictionary are added to the existing state value instead of replacing it. Note that the state value and the postcondition value must be of the same numeric type for this to work correctly.

Example that will add 10 to the agent's health as a postcondition:

```csharp
var heal = registry.RegisterAction(
    name: "Heal",
    executor: (IAgent agent, IAction action) => {
        Console.WriteLine("healed for 10 hp");
        return ExecutionStatus.Succeeded;
    },
    arithmeticPostconditions: new() {
        { "health", 10 }
    },
    cost: 0.5f
);
```

Note that [normal Goals](#goals) use simple equality checks and cannot tell that a value is closer or further away from the goal value. If you want to use an arithmetic postcondition and have the library detect that you are moving closer to your goal, use [Comparative Goals](#comparative-goals) or [Extreme Goals](#extreme-goals). Both of these types of numerically based goals will calculate distance from a numeric goal value properly.

#### Parameter Postconditions

**Parameter postconditions** are postconditions that copy one of the parameters passed to the action into the agent state. The structure uses a dictionary of string keys and string values, where the keys are the keys to the parameter value being copied and the values are the state key into which you are copying the value.

Example that will copy the "target" parameter into the "target" state value:

```csharp
var targets = new List<string> { "target1", "target2" };
var setTarget = registry.RegisterAction(
    name: "Set Target",
    executor: (IAgent agent, IAction action) => {
        Console.WriteLine("set target");
        return ExecutionStatus.Succeeded;
    },
    permutationSelectors: new() {
        { "target", PermutationSelectorGenerators.SelectFromCollection(targets) }
    },
    parameterPostconditions: new() {
        { "target", "target" }
    },
    cost: 0.5f
);
```

This example will create permutations of the action that either target `"target1"` or `"target2"`, and will copy the selected target into the `"target"` state value. See [permutation selectors](#permutation-selectors) for more information on permutation selectors.

### Sensors

**Sensors** allow an agent to distill information into their state, often derived from other state values. Sensors execute on every `Step()` call, and use a **sensor handler** to execute code. Sensors can be passed into the agent constructor, like so:

```csharp
Sensor agentProximitySensor = new Sensor(
    (IAgent agent) => {
        if (AgentNearOtherAgent(agent)) agent.State.Set("nearOtherAgent", true);
        else agent.State.Set("nearOtherAgent", false);
    }
);
var registry = new Registry();
registry.RegisterAgent(
    name: "My Agent",
    sensors: new() { agentProximitySensor });
IAgent agent = registry.GetInstance("My Agent");
```

### Permutation Selectors

Finally, actions can be constructed with **permutation selectors**, which will instantiate multiple copies of the action with different parameters for purposes such as target selection. The library comes with some default permutation selector generators, or you can write your own as callbacks. For instance, if you want an action to be evaluated separately with each member of a list as a potential parameter, you would construct the action as so:

```csharp
var myAction = registry.RegisterAction(
    name: "My Action",
    permutationSelectors: new() {
        { "target1", PermutationSelectorGenerators.SelectFromCollectionInState<Agent>("otherAgents") },
        { "target2", PermutationSelectorGenerators.SelectFromCollectionInState<Agent>("otherAgents") }
    },
    executor: (IAgent agent, IAction action) => {
        Console.WriteLine(action.GetParameter("target1").ToString());
        Console.WriteLine(action.GetParameter("target2").ToString());
    }
);
```

The code above will create an action that when evaluated for execution in an agent plan will be considered once for every pair combination of elements in the "otherAgents" collection of the agent state, one for `target1`, and one for `target2`. In order to take advantage of this feature, you can also calculate variable costs based on action parameters using the `costCallback` argument in the `Action` constructor. See [cost callbacks](#cost-callbacks) for more information.

### Cost Callbacks

**Cost callbacks** allow you to calculate the cost of an action based on its parameters. This is useful for actions that have variable costs based on the parameters passed to them. For instance, if you have an action that moves the agent to a target, you might want to calculate the cost of the action based on the distance to the target. You can do this by passing a cost callback to the action constructor:

```csharp
var moveToTarget = registry.RegisterAction(
    name: "Move To Target",
    executor: (IAgent agent, IAction action) => {
        Console.WriteLine("moved to target");
        return ExecutionStatus.Succeeded;
    },
    costCallback: (IReadOnlyAction action, IReadOnlyState state) => {
        if (action.GetParameter("target") is Agent target) {
            var distance = GetDistance(this, target);
            return distance;
        }
        else return float.MaxValue;
    }
);
```

### State Mutators

**State mutators** allow you to mutate the agent state during execution or evaluation of an action. This is useful for actions that have more complex side effects on agent state. For instance, if you have an action that moves the agent to a target, you might want to mutate the agent state to reflect the new position of the agent. You can do this by passing a state mutator to the action constructor:

```csharp
var moveToTarget = registry.RegisterAction(
    name: "Move To Target",
    executor: (IAgent agent, IAction action) => {
        Console.WriteLine("moved to target");
        return ExecutionStatus.Succeeded;
    },
    stateMutator: (IReadOnlyAction action, IState state) => {
        if (action.GetParameter("target") is Agent target) {
            state.Set("position", target.State["position"]);
        }
    }
);
```

### State Checkers

**State checkers** allow you to check agent state programmatically during execution or evaluation of an action. This is useful for actions that have more complex preconditions than simple equality checks or arithmetic comparisons. For instance, if you have an action that moves the agent to a target, you might want to check that the target is in range. You can do this by passing a state checker to the action constructor:

```csharp
var moveToTarget = registry.RegisterAction(
    name: "Move To Target",
    executor: (IAgent agent, IAction action) => {
        Console.WriteLine("moved to target");
        return ExecutionStatus.Succeeded;
    },
    stateChecker: (IReadOnlyAction action, IReadOnlyState state) => {
        if (action.GetParameter("target") is Agent target) {
            var distance = GetDistance(this, target);
            if (distance > 10) return false;
            else return true;
        }
        return false;
    }
);
```

### Full API Docs

Full API docs are available [here](./api-docs/index.md).

## Events

Mountain GOAP features a simple event system that allows you to subscribe to events that occur during the agent's planning and execution process.

### Agent events

The following events are available on agents:

-   OnAgentActionSequenceCompleted: Called when the agent has finished executing its plan.
    -   Example usage: `Agent.OnAgentActionSequenceCompleted += (IReadOnlyAgent agent) => { Console.WriteLine("Agent finished executing its plan."); };`
-   OnAgentStep: Called when the agent executes a step of work.
    -   Example usage: `Agent.OnAgentStep += (IReadOnlyAgent agent) => { Console.WriteLine("Agent is working."); };`
-   OnPlanningStarted: Called when the agent begins planning.
    -   Example usage: `Agent.OnPlanningStarted += (IReadOnlyAgent agent) => { Console.WriteLine("Agent started planning."); };`
-   OnPlanningFinished: Called when the agent finishes planning.
    -   Example usage: `Agent.OnPlanningFinished += (IReadOnlyAgent agent, IReadOnlyGoal? goal, float utility) => { Console.WriteLine("Agent finished planning."); };`
-   OnPlanningFinishedForSingleGoal: Called when the agent finishes planning for a single goal.
    -   Example usage: `Agent.OnPlanningFinishedForSingleGoal += (IReadOnlyAgent agent, IReadOnlyGoal goal, float utility) => { Console.WriteLine("Agent finished planning for a single goal."); };`
-   OnPlanUpdated: Called when the agent generates a viable action sequence.
    -   Example usage: `Agent.OnPlanUpdated += (IReadOnlyAgent agent, IActionPlan plan) => { Console.WriteLine("Agent generated a viable action sequence."); };`
-   OnEvaluatedActionNode: Extremely low level debugging tool, called whenever the agent evaluates a potentially viable node in the action graph. The second parameter is a dictionary of all nodes that have been evaluated so far, with the keys as the most recent nodes and the values as the nodes that preceded them. You can use this to reconstruct the chain of actions that led to this point.
    -   Example usage: `Agent.OnEvaluatedActionNode += (IReadOnlyActionNode node, IReadOnlyDictionary<IReadOnlyActionNode, IReadOnlyActionNode> nodes) => { Console.WriteLine("Agent evaluated a node."); };`

### Action events

The following events are available on actions:

-   OnBeginExecuteAction: Called when the agent begins executing an action.
    -   Example usage: `Action.OnBeginExecuteAction += (IReadOnlyAgent agent, IAction action) => { Console.WriteLine("Agent started executing an action."); };`
-   OnFinishExecuteAction: Called when the agent finishes executing an action.
    -   Example usage: `Action.OnFinishExecuteAction += (IReadOnlyAgent agent, IAction action, ExecutionStatus status) => { Console.WriteLine("Agent finished executing an action."); };`

### Sensor events

The following events are available on sensors:

-   OnSensorRun: Called when the agent runs a sensor.
    -   Example usage: `Sensor.OnSensorRun += (IReadOnlyAgent agent, Sensor sensor) => { Console.WriteLine("Agent ran a sensor."); };`

## Logger

Mountain GOAP contains a default logger implementation that can be used to examine agent behavior. After including the MountainGoapLogger code, you can enable the logger using the following code:

```csharp
_ = new MountainGoapLogging.DefaultLogger(
    logToConsole: true,
    loggingFile: "agents.log"
);
```

## Examples

[Examples documentation](./Examples/examples.md).

1. [Happiness Maximizer Example](./Examples/HappinessIncrementer.cs)
2. [RPG Example](./Examples//RpgExample/RpgExample.cs)

## Project Structure

| File or folder                                                    | Description                                                                               |
| ----------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| `/Examples/`                                                      | Examples of how to use the library                                                        |
| `/Examples/RpgExample/`                                           | RPG grid-based example.                                                                   |
| `/Examples/RpgExample/RpgCharacterFactory.cs`                     | Static methods for creating character agents.                                             |
| `/Examples/RpgExample/RpgExample.cs`                              | Main RPG example entrypoint.                                                              |
| `/Examples/RpgExample/RpgMonsterFactory.cs`                       | Static methods for creating enemy agents derived from the base character agent.           |
| `/Examples/RpgExample/Utils.cs`                                   | RPG example utility functions.                                                            |
| `/Examples/examples.md`                                           | Examples documentation.                                                                   |
| `/Examples/HappinessIncrementer.cs`                               | Happiness incrementer example.                                                            |
| `/Examples/Program.cs`                                            | Examples entrypoint.                                                                      |
| `/MountainGoap/`                                                  | The main library folder                                                                   |
| `/MountainGoap/CallbackDelegates/`                                | Function signatures for callbacks.                                                        |
| `/MountainGoap/CallbackDelegates/ExecutorCallback.cs`             | Function signature for a callback that executes an action.                                |
| `/MountainGoap/CallbackDelegates/PermutationSelectorCallbacks.cs` | Function signature for a callback that selects a list of options for an action parameter. |
| `/MountainGoap/CallbackDelegates/SensorRunCallback.cs`            | Function signature for a callback that runs a sensor.                                     |
| `/MountainGoap/Internals/`                                        | Internal classes that external applications using the library will not need directly.     |
| `/MountainGoap/Internals/ActionAStar.cs`                          | Class that calculates AStar for an action graph.                                          |
| `/MountainGoap/Internals/ActionGraph.cs`                          | Class that represents an action graph.                                                    |
| `/MountainGoap/Internals/ActionNode.cs`                           | Class that represents a node in an action graph.                                          |
| `/MountainGoap/Internals/Planner.cs`                              | Planning class used by agents.                                                            |
| `/MountainGoap/Action.cs`                                         | An immutable action template that can be made available to agents.                        |
| `/MountainGoap/ActionCollection.cs`                               | Ordered collection of action templates with precondition index.                           |
| `/MountainGoap/Agent.cs`                                          | A runtime agent instance that can figure out plans to execute.                            |
| `/MountainGoap/AgentTemplate.cs`                                  | Immutable agent template describing shared configuration.                                 |
| `/MountainGoap/ExecutingAction.cs`                                | Runtime action instance holding resolved parameters.                                      |
| `/MountainGoap/PlannerWorkerPool.cs`                              | Fixed-size worker pool for offloading planning to background threads.                     |
| `/MountainGoap/PoolManager.cs`                                    | Centralized container for shared object pools.                                            |
| `/MountainGoap/Registry.cs`                                       | Per-world factory for registering action/agent templates and vending pooled instances.     |
| `/MountainGoap/State.cs`                                          | Thread-safe agent state wrapper.                                                          |
| `/MountainGoap/BaseGoal.cs`                                       | A base class for all goal types.                                                          |
| `/MountainGoap/ComparativeGoal.cs`                                | A goal that compares a value to a pre-existing value                                      |
| `/MountainGoap/ComparisonOperator.cs`                             | An enum defining the comparison operators that can be used with ComparativeGoal.          |
| `/MountainGoap/ComparisonValuePair.cs`                            | A class that represents a comparison value pair.                                          |
| `/MountainGoap/ExecutionStatus.cs`                                | An enum defining the execution status of an action.                                       |
| `/MountainGoap/ExtremeGoal.cs`                                    | A goal that attempts to minimize or maximize a state value.                               |
| `/MountainGoap/Goal.cs`                                           | A goal that agents can attempt to accomplish.                                             |
| `/MountainGoap/PermutationSelectorGenerators.cs`                  | Generators for lambda functions that return a list of options for an action parameter.    |
| `/MountainGoap/Sensor.cs`                                         | A sensor that generates data for use by an agent.                                         |
| `/MountainGoapLogging/DefaultLogger.cs`                           | Example logger implementation that can be used to inspect agent behavior.                 |

## Roadmap

-   Tests
-   Examples - general and Unity

## Other open source GOAP projects

-   [ReGoap](https://github.com/luxkun/ReGoap) - C# GOAP library with more direct Unity support, providing Unity Components that can be attached to GameObjects.

## License Acknowledgements

Project is MIT licensed, and the main license file can be found [here](./LICENSE.txt).

### Priority Queue Implementation

The MIT License (MIT)

Copyright (c) 2013 Daniel "BlueRaja" Pflughoeft

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
