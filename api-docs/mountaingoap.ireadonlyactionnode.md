# IReadOnlyActionNode

Namespace: MountainGoap

Read-only view of an action node in an action graph. Replaces the previously public `ActionNode` class.

```csharp
public interface IReadOnlyActionNode
```

## Properties

### **Action**

Gets the action associated with this node.

```csharp
IReadOnlyAction? Action { get; }
```

#### Property Value

[IReadOnlyAction](./mountaingoap.ireadonlyaction.md)?

### **State**

Gets the planning state at this node.

```csharp
IReadOnlyState State { get; }
```

#### Property Value

[IReadOnlyState](./mountaingoap.ireadonlystate.md)
