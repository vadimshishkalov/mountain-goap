# IReadOnlyAction

Namespace: MountainGoap

Read-only view of an action. Used in cost callbacks, state checkers, and other read-only contexts.

```csharp
public interface IReadOnlyAction
```

## Properties

### **Name**

Gets the name of the action.

```csharp
string Name { get; }
```

### **ParameterKeys**

Gets the keys of all parameters set on this action instance.

```csharp
IEnumerable<string> ParameterKeys { get; }
```

## Methods

### **GetParameter(String)**

Gets a parameter value by key.

```csharp
object? GetParameter(string key)
```
