# ActionCollection

Namespace: MountainGoap

Ordered collection of action templates with a precondition-based index for fast candidate filtering.

```csharp
public class ActionCollection : IReadOnlyActionIndex, IEnumerable<Action>
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ActionCollection](./mountaingoap.actioncollection.md)

Implements [IReadOnlyActionIndex](./mountaingoap.ireadonlyactionindex.md), IEnumerable

## Properties

### **Count**

Gets the number of actions in the collection.

```csharp
public int Count { get; }
```

### **Item[Int32]**

Gets the action at the specified index.

```csharp
public Action this[int i] { get; }
```

## Methods

### **Add(Action)**

Adds an action to the collection and updates the precondition index.

```csharp
public void Add(Action action)
```

### **Remove(Action)**

Removes an action from the collection.

```csharp
public bool Remove(Action action)
```

### **GetCandidates(IEnumerable&lt;String&gt;, HashSet&lt;Action&gt;)**

Populates the result set with actions whose preconditions reference any of the given state keys.

```csharp
public void GetCandidates(IEnumerable<string> keys, HashSet<Action> result)
```
