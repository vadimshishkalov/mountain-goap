# IAgent

Namespace: MountainGoap

Mutable interface for a GOAP agent runtime instance.

```csharp
public interface IAgent : IReadOnlyAgent
```

Extends [IReadOnlyAgent](./mountaingoap.ireadonlyagent.md)

## Properties

### **State**

Gets the mutable agent state.

```csharp
new IState State { get; }
```

#### Property Value

[IState](./mountaingoap.istate.md)

### **Memory**

Gets the memory storage object for the agent.

```csharp
Dictionary<string, object?> Memory { get; }
```

## Methods

### **Step(StepMode)**

Executes a step of work.

```csharp
void Step(StepMode mode = StepMode.Default)
```

### **Plan()**

Makes a plan synchronously.

```csharp
void Plan()
```

### **PlanAsync()**

Enqueues planning to the background worker pool.

```csharp
void PlanAsync()
```

### **ExecutePlan()**

Executes the current plan without replanning.

```csharp
void ExecutePlan()
```

### **ClearPlan()**

Clears the current action sequences.

```csharp
void ClearPlan()
```
