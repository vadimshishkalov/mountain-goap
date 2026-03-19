# IReadOnlyState

Namespace: MountainGoap

Read-only view of agent state. Used in planning, cost callbacks, and state checkers.

```csharp
public interface IReadOnlyState
```

## Properties

### **Item[String]**

Gets a state value by key.

```csharp
object? this[string key] { get; }
```

### **Keys**

Gets all keys in the state.

```csharp
IEnumerable<string> Keys { get; }
```

## Methods

### **Get(String)**

Gets a value by key.

```csharp
object? Get(string key)
```

### **ContainsKey(String)**

Checks whether the state contains the specified key.

```csharp
bool ContainsKey(string key)
```
