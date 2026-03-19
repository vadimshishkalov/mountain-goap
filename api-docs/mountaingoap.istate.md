# IState

Namespace: MountainGoap

Mutable state interface for reading and writing agent state values.

```csharp
public interface IState : IReadOnlyState
```

Extends [IReadOnlyState](./mountaingoap.ireadonlystate.md)

## Properties

### **Item[String]**

Gets or sets a state value by key.

```csharp
new object? this[string key] { get; set; }
```

## Methods

### **Set(String, Object)**

Sets a state value.

```csharp
void Set(string key, object? value)
```
