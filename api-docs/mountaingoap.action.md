# Action

Namespace: MountainGoap

Represents an immutable action template in a GOAP system. Consumers should use [Registry.RegisterAction](./mountaingoap.registry.md) to create actions.

```csharp
public class Action
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Action](./mountaingoap.action.md)

## Fields

### **Name**

Name of the action.

```csharp
public readonly string Name;
```

## Properties

### **StateCostDeltaMultiplier**

Gets the multiplier callback for delta value to provide delta cost.

```csharp
public StateCostDeltaMultiplierCallback? StateCostDeltaMultiplier { get; }
```

#### Property Value

StateCostDeltaMultiplierCallback?

## Constructors

### **Action(String, Dictionary&lt;String, PermutationSelectorCallback&gt;, ExecutorCallback, Single, CostCallback, Dictionary&lt;String, Object&gt;, Dictionary&lt;String, ComparisonValuePair&gt;, Dictionary&lt;String, Object&gt;, Dictionary&lt;String, Object&gt;, Dictionary&lt;String, String&gt;, StateMutatorCallback, StateCheckerCallback, StateCostDeltaMultiplierCallback)**

> **Obsolete.** Use [Registry.RegisterAction](./mountaingoap.registry.md) instead.

```csharp
[Obsolete]
public Action(string? name = null, Dictionary<string, PermutationSelectorCallback>? permutationSelectors = null, ExecutorCallback? executor = null, float cost = 1f, CostCallback? costCallback = null, Dictionary<string, object?>? preconditions = null, Dictionary<string, ComparisonValuePair>? comparativePreconditions = null, Dictionary<string, object?>? postconditions = null, Dictionary<string, object>? arithmeticPostconditions = null, Dictionary<string, string>? parameterPostconditions = null, StateMutatorCallback? stateMutator = null, StateCheckerCallback? stateChecker = null, StateCostDeltaMultiplierCallback? stateCostDeltaMultiplier = null)
```

## Methods

### **DefaultStateCostDeltaMultiplier(IReadOnlyAction, String)**

Default implementation of the state cost delta multiplier.

```csharp
public static float DefaultStateCostDeltaMultiplier(IReadOnlyAction? action, string stateKey)
```

#### Parameters

`action` [IReadOnlyAction](./mountaingoap.ireadonlyaction.md)?

`stateKey` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)

#### Returns

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)

## Events

### **OnBeginExecuteAction**

Event that triggers when an action begins executing.

```csharp
public static event BeginExecuteActionEvent? OnBeginExecuteAction;
```

### **OnFinishExecuteAction**

Event that triggers when an action finishes executing.

```csharp
public static event FinishExecuteActionEvent? OnFinishExecuteAction;
```
