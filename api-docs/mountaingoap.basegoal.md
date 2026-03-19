# BaseGoal

Namespace: MountainGoap

Represents an abstract class for a goal to be achieved for an agent.

```csharp
public abstract class BaseGoal
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [BaseGoal](./mountaingoap.basegoal.md)

## Fields

### **Name**

Name of the goal.

```csharp
public readonly string Name;
```

### **Weight**

Weight of the goal.

```csharp
public readonly float Weight;
```

## Constructors

### **BaseGoal(String, Single)**

```csharp
protected BaseGoal(string? name = null, float weight = 1f)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)? — Name of the goal.

`weight` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single) — Weight to give the goal.
