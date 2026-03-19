# NeighborLookupMode

Namespace: MountainGoap

Controls how the planner finds candidate actions during A* neighbor expansion.

```csharp
public enum NeighborLookupMode
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [NeighborLookupMode](./mountaingoap.neighborlookupmode.md)

## Fields

| Name | Description |
| --- | --- |
| Disabled | Full scan of all actions (no indexing). |
| Index | Use precondition index to narrow candidates by state key. |
| Aggressive | Track viable vs. unchecked actions, promoting candidates as state evolves. |
