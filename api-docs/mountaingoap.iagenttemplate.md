# IAgentTemplate

Namespace: MountainGoap

Read-only view of an agent template describing shared configuration for a type of agent.

```csharp
public interface IAgentTemplate
```

## Properties

### **Name**

```csharp
string Name { get; }
```

### **StateTemplate**

Gets the initial state values copied into each runtime instance.

```csharp
IReadOnlyDictionary<string, object?> StateTemplate { get; }
```

### **Goals**

```csharp
IReadOnlyList<IReadOnlyGoal> Goals { get; }
```

### **Actions**

```csharp
IReadOnlyActionIndex Actions { get; }
```

#### Property Value

[IReadOnlyActionIndex](./mountaingoap.ireadonlyactionindex.md)

### **Sensors**

```csharp
IReadOnlyList<Sensor> Sensors { get; }
```

### **Configuration**

```csharp
AgentConfiguration Configuration { get; }
```

#### Property Value

[AgentConfiguration](./mountaingoap.agentconfiguration.md)
