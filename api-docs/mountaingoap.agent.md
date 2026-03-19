# Agent

Namespace: MountainGoap

GOAP agent runtime instance. Consumers should use [Registry.GetInstance](./mountaingoap.registry.md) to obtain agents.

```csharp
public class Agent : IAgent
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Agent](./mountaingoap.agent.md)

Implements [IAgent](./mountaingoap.iagent.md), [IReadOnlyAgent](./mountaingoap.ireadonlyagent.md)

## Properties

### **Name**

Gets the name of the agent.

```csharp
public string Name { get; }
```

### **Template**

Gets the agent template this instance was created from.

```csharp
public IAgentTemplate Template { get; }
```

#### Property Value

[IAgentTemplate](./mountaingoap.iagenttemplate.md)

### **State**

Gets or sets the current world state from the agent perspective.

```csharp
public State State { get; set; }
```

#### Property Value

[State](./mountaingoap.state.md)

### **Memory**

Gets or sets the memory storage object for the agent.

```csharp
public Dictionary<string, object?> Memory { get; set; }
```

### **Goals**

Gets the list of active goals for the agent.

```csharp
public IReadOnlyList<IReadOnlyGoal> Goals { get; }
```

### **Actions**

Gets the actions available to the agent.

```csharp
public IReadOnlyActionIndex Actions { get; }
```

#### Property Value

[IReadOnlyActionIndex](./mountaingoap.ireadonlyactionindex.md)

### **Sensors**

Gets the sensors available to the agent.

```csharp
public IReadOnlyList<Sensor> Sensors { get; }
```

### **Configuration**

Gets or sets the behavioural configuration for this agent.

```csharp
public AgentConfiguration Configuration { get; set; }
```

#### Property Value

[AgentConfiguration](./mountaingoap.agentconfiguration.md)

### **CurrentActionSequences**

Gets the chains of actions currently being performed by the agent.

```csharp
public IReadOnlyList<IActionPlan> CurrentActionSequences { get; }
```

#### Property Value

IReadOnlyList&lt;[IActionPlan](./mountaingoap.iactionplan.md)&gt;

### **IsBusy**

Gets a value indicating whether the agent is currently executing one or more actions.

```csharp
public bool IsBusy { get; }
```

### **IsPlanning**

Gets a value indicating whether the agent is currently planning.

```csharp
public bool IsPlanning { get; }
```

## Constructors

### **Agent(IAgentTemplate, PoolManager)**

Initializes a new instance of the [Agent](./mountaingoap.agent.md) class. Prefer [Registry.GetInstance](./mountaingoap.registry.md) for pooled creation.

```csharp
public Agent(IAgentTemplate template, PoolManager? poolManager = null)
```

#### Parameters

`template` [IAgentTemplate](./mountaingoap.iagenttemplate.md) — The agent template defining shared configuration.

`poolManager` [PoolManager](./mountaingoap.poolmanager.md)? — Optional shared object pools for planning.

## Methods

### **Step(StepMode)**

You should call this every time your game state updates.

```csharp
public void Step(StepMode mode = StepMode.Default)
```

#### Parameters

`mode` [StepMode](./mountaingoap.stepmode.md) — Mode to be used for executing the step of work.

### **ClearPlan()**

Clears the current action sequences (also known as plans).

```csharp
public void ClearPlan()
```

### **Plan()**

Makes a plan synchronously.

```csharp
public void Plan()
```

### **PlanAsync()**

Enqueues planning to the background worker pool.

```csharp
public void PlanAsync()
```

### **ExecutePlan()**

Executes the current plan without replanning.

```csharp
public void ExecutePlan()
```

## Events

### **OnAgentStep**

```csharp
public static event AgentStepEvent? OnAgentStep;
```

### **OnAgentActionSequenceCompleted**

```csharp
public static event AgentActionSequenceCompletedEvent? OnAgentActionSequenceCompleted;
```

### **OnPlanningStarted**

```csharp
public static event PlanningStartedEvent? OnPlanningStarted;
```

### **OnPlanningStartedForSingleGoal**

```csharp
public static event PlanningStartedForSingleGoalEvent? OnPlanningStartedForSingleGoal;
```

### **OnPlanningFinishedForSingleGoal**

```csharp
public static event PlanningFinishedForSingleGoalEvent? OnPlanningFinishedForSingleGoal;
```

### **OnPlanningFinished**

```csharp
public static event PlanningFinishedEvent? OnPlanningFinished;
```

### **OnPlanUpdated**

```csharp
public static event PlanUpdatedEvent? OnPlanUpdated;
```

### **OnEvaluatedActionNode**

```csharp
public static event EvaluatedActionNodeEvent? OnEvaluatedActionNode;
```
