# IReadOnlyAgent

Namespace: MountainGoap

Read-only view of a GOAP agent. Used in event callbacks and read-only contexts.

```csharp
public interface IReadOnlyAgent
```

## Properties

### **Name**

```csharp
string Name { get; }
```

### **State**

```csharp
IReadOnlyState State { get; }
```

#### Property Value

[IReadOnlyState](./mountaingoap.ireadonlystate.md)

### **Template**

```csharp
IAgentTemplate Template { get; }
```

#### Property Value

[IAgentTemplate](./mountaingoap.iagenttemplate.md)

### **Goals**

```csharp
IReadOnlyList<IReadOnlyGoal> Goals { get; }
```

### **Sensors**

```csharp
IReadOnlyList<Sensor> Sensors { get; }
```

### **Actions**

```csharp
IReadOnlyActionIndex Actions { get; }
```

#### Property Value

[IReadOnlyActionIndex](./mountaingoap.ireadonlyactionindex.md)

### **IsBusy**

```csharp
bool IsBusy { get; }
```

### **IsPlanning**

```csharp
bool IsPlanning { get; }
```

### **CurrentActionSequences**

```csharp
IReadOnlyList<IActionPlan> CurrentActionSequences { get; }
```
