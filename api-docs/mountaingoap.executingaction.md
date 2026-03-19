# ExecutingAction

Namespace: MountainGoap

Runtime action instance holding resolved parameters for a specific permutation. Created internally during planning.

```csharp
public class ExecutingAction : IAction
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ExecutingAction](./mountaingoap.executingaction.md)

Implements [IAction](./mountaingoap.iaction.md), [IReadOnlyAction](./mountaingoap.ireadonlyaction.md)

## Properties

### **Name**

Gets the name of the underlying action template.

```csharp
public string Name { get; }
```

### **ParameterKeys**

Gets the keys of all parameters currently set on this instance.

```csharp
public IEnumerable<string> ParameterKeys { get; }
```

## Methods

### **SetParameter(String, Object)**

Sets a parameter value.

```csharp
public void SetParameter(string key, object value)
```

### **GetParameter(String)**

Gets a parameter value.

```csharp
public object? GetParameter(string key)
```

#### Returns

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)?
