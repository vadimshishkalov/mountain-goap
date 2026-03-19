# IReadOnlyActionIndex

Namespace: MountainGoap

Read-only indexed collection of action templates.

```csharp
public interface IReadOnlyActionIndex : IEnumerable<Action>
```

Extends IEnumerable&lt;[Action](./mountaingoap.action.md)&gt;

## Properties

### **Count**

Gets the number of actions.

```csharp
int Count { get; }
```

## Methods

### **GetCandidates(IEnumerable&lt;String&gt;, HashSet&lt;Action&gt;)**

Populates the result set with actions whose preconditions reference any of the given state keys.

```csharp
void GetCandidates(IEnumerable<string> keys, HashSet<Action> result)
```
