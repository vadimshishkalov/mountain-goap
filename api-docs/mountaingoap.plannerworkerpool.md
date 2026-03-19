# PlannerWorkerPool

Namespace: MountainGoap

Fixed-size worker pool for offloading planning to background threads.

```csharp
public class PlannerWorkerPool : IDisposable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PlannerWorkerPool](./mountaingoap.plannerworkerpool.md)

Implements [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable)

## Properties

### **Default**

Gets the global shared worker pool instance.

```csharp
public static PlannerWorkerPool Default { get; }
```

## Constructors

### **PlannerWorkerPool(Int32)**

```csharp
public PlannerWorkerPool(int workerCount = 0)
```

#### Parameters

`workerCount` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32) — Number of worker threads. Defaults to `Environment.ProcessorCount`.

## Methods

### **Dispose()**

Disposes the worker pool and stops all workers.

```csharp
public void Dispose()
```
