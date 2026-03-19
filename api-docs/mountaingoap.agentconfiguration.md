# AgentConfiguration

Namespace: MountainGoap

Behavioural configuration for an agent or agent template.

```csharp
public class AgentConfiguration
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [AgentConfiguration](./mountaingoap.agentconfiguration.md)

## Properties

### **CostMaximum**

Gets the maximum cost of an allowable plan.

```csharp
public float CostMaximum { get; init; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single) — Default: `float.MaxValue`

### **StepMaximum**

Gets the maximum number of steps in an allowable plan.

```csharp
public int StepMaximum { get; init; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32) — Default: `int.MaxValue`

### **NeighborLookupMode**

Gets the neighbor lookup mode used during planning.

```csharp
public NeighborLookupMode NeighborLookupMode { get; init; }
```

#### Property Value

[NeighborLookupMode](./mountaingoap.neighborlookupmode.md)
