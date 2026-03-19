# IActionPlan

Namespace: MountainGoap

Read-only view of a completed action plan.

```csharp
public interface IActionPlan
```

## Properties

### **Steps**

Gets the ordered list of actions in this plan.

```csharp
IReadOnlyList<IReadOnlyAction> Steps { get; }
```
