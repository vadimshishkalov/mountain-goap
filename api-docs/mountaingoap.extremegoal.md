# ExtremeGoal

Namespace: MountainGoap

Represents a goal requiring an extreme value to be achieved for an agent.

```csharp
public class ExtremeGoal : BaseGoal
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [BaseGoal](./mountaingoap.basegoal.md) → [ExtremeGoal](./mountaingoap.extremegoal.md)

## Fields

### **Name**

Name of the goal.

```csharp
public readonly string Name;
```

## Constructors

### **ExtremeGoal(String, Single, Dictionary&lt;String, Boolean&gt;)**

Initializes a new instance of the [ExtremeGoal](./mountaingoap.extremegoal.md) class.

```csharp
public ExtremeGoal(string? name = null, float weight = 1f, Dictionary<string, bool>? desiredState = null)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Name of the goal.

`weight` [Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>
Weight to give the goal.

`desiredState` [Dictionary&lt;String, Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)<br>
States to be maximized or minimized.
