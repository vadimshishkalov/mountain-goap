# State

Namespace: MountainGoap

Thread-safe agent state wrapper backed by ConcurrentDictionary.

```csharp
public class State : IExecutionState, IEnumerable<KeyValuePair<string, object?>>
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [State](./mountaingoap.state.md)

Implements [IState](./mountaingoap.istate.md), [IReadOnlyState](./mountaingoap.ireadonlystate.md), IEnumerable

## Properties

### **Item[String]**

Gets or sets a state value by key.

```csharp
public object? this[string key] { get; set; }
```

### **Keys**

Gets all keys in the state.

```csharp
public IEnumerable<string> Keys { get; }
```

## Methods

### **Get(String)**

Gets a value by key.

```csharp
public object? Get(string key)
```

### **Set(String, Object)**

Sets a value by key.

```csharp
public void Set(string key, object? value)
```

### **Add(String, Object)**

Adds a key-value pair.

```csharp
public void Add(string key, object? value)
```

### **ContainsKey(String)**

Checks whether the state contains the specified key.

```csharp
public bool ContainsKey(string key)
```

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)
