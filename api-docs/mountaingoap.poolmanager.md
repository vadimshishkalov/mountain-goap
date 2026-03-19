# PoolManager

Namespace: MountainGoap

Centralized container for shared object pools used during planning.

```csharp
public class PoolManager
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PoolManager](./mountaingoap.poolmanager.md)

## Constructors

### **PoolManager(Boolean, Boolean, Boolean, Boolean)**

```csharp
public PoolManager(bool shareNodePool = true, bool shareGraphPool = true, bool sharePlanPool = true, bool shareStatePool = true)
```

#### Parameters

`shareNodePool` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean) — Share A* nodes across agents.

`shareGraphPool` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean) — Share A* graphs across agents.

`sharePlanPool` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean) — Share action plans across agents.

`shareStatePool` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean) — Share planning states across agents.

## Properties

### **NodePool**

Gets the shared action node pool, if enabled.

```csharp
public ActionNodePool? NodePool { get; }
```

### **StatePool**

Gets the shared state pool, if enabled.

```csharp
public StatePool? StatePool { get; }
```
