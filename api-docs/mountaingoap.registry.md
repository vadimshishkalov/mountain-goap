# Registry

Namespace: MountainGoap

Per-world factory for registering action and agent templates and vending pooled runtime instances.

```csharp
public class Registry
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Registry](./mountaingoap.registry.md)

## Constructors

### **Registry(AgentConfiguration, PoolManager)**

```csharp
public Registry(AgentConfiguration? configuration = null, PoolManager? poolManager = null)
```

#### Parameters

`configuration` [AgentConfiguration](./mountaingoap.agentconfiguration.md)? — Default behavioural configuration applied to every registered agent type.

`poolManager` [PoolManager](./mountaingoap.poolmanager.md)? — Shared object pools for planning. Pass null to let each agent create its own pools.

## Methods

### **RegisterAction(String, Dictionary&lt;String, PermutationSelectorCallback&gt;, ExecutorCallback, Single, CostCallback, Dictionary&lt;String, Object&gt;, Dictionary&lt;String, ComparisonValuePair&gt;, Dictionary&lt;String, Object&gt;, Dictionary&lt;String, Object&gt;, Dictionary&lt;String, String&gt;, StateMutatorCallback, StateCheckerCallback, StateCostDeltaMultiplierCallback)**

Returns a registered action with the given name, creating and storing it on first call. Subsequent calls with the same name return the cached instance. Passing null as the name always creates a new instance.

```csharp
public Action RegisterAction(string? name = null, Dictionary<string, PermutationSelectorCallback>? permutationSelectors = null, ExecutorCallback? executor = null, float cost = 1f, CostCallback? costCallback = null, Dictionary<string, object?>? preconditions = null, Dictionary<string, ComparisonValuePair>? comparativePreconditions = null, Dictionary<string, object?>? postconditions = null, Dictionary<string, object>? arithmeticPostconditions = null, Dictionary<string, string>? parameterPostconditions = null, StateMutatorCallback? stateMutator = null, StateCheckerCallback? stateChecker = null, StateCostDeltaMultiplierCallback? stateCostDeltaMultiplier = null)
```

#### Returns

[Action](./mountaingoap.action.md)

### **RegisterAgent(String, State, List&lt;BaseGoal&gt;, ActionCollection, List&lt;Sensor&gt;, AgentConfiguration)**

Registers an agent template by name. Throws if the name is already registered.

```csharp
public IAgentTemplate RegisterAgent(string name, State? state = null, List<BaseGoal>? goals = null, ActionCollection? actions = null, List<Sensor>? sensors = null, AgentConfiguration? configuration = null)
```

#### Returns

[IAgentTemplate](./mountaingoap.iagenttemplate.md)

### **HasTemplate(String)**

Returns true if an agent template with the given name is registered.

```csharp
public bool HasTemplate(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string) — The template name to look up.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)

### **TryGetTemplate(String, IAgentTemplate)**

Attempts to retrieve a registered agent template by name.

```csharp
public bool TryGetTemplate(string name, out IAgentTemplate? template)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string) — The template name to look up.

`template` [IAgentTemplate](./mountaingoap.iagenttemplate.md)? — When this method returns, contains the template if found; otherwise null.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)

### **HasAction(String)**

Returns true if an action with the given name is registered.

```csharp
public bool HasAction(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string) — The action name to look up.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)

### **TryGetAction(String, Action)**

Attempts to retrieve a registered action by name.

```csharp
public bool TryGetAction(string name, out Action? action)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string) — The action name to look up.

`action` [Action](./mountaingoap.action.md)? — When this method returns, contains the action if found; otherwise null.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)

### **GetInstance(String)**

Returns a runtime instance for the named agent template. Draws from the pool when available; otherwise creates a new agent.

```csharp
public IAgent GetInstance(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string) — The name of a previously registered agent template.

#### Returns

[IAgent](./mountaingoap.iagent.md)

### **ReturnInstance(IAgent)**

Returns an agent to its pool for future reuse. The caller must not use the agent after returning it.

```csharp
public void ReturnInstance(IAgent agent)
```

#### Parameters

`agent` [IAgent](./mountaingoap.iagent.md) — Agent to return.
