# Sensor

Namespace: MountainGoap

Sensor for getting information about world state.

```csharp
public class Sensor
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Sensor](./mountaingoap.sensor.md)

## Fields

### **Name**

Name of the sensor.

```csharp
public readonly string Name;
```

## Constructors

### **Sensor(SensorRunCallback, String)**

Initializes a new instance of the [Sensor](./mountaingoap.sensor.md) class.

```csharp
public Sensor(SensorRunCallback runCallback, string? name = null)
```

#### Parameters

`runCallback` SensorRunCallback — Callback to be executed when the sensor runs.

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)? — Name of the sensor.

## Methods

### **Run(IAgent)**

Runs the sensor during a game loop.

```csharp
public void Run(IAgent agent)
```

#### Parameters

`agent` [IAgent](./mountaingoap.iagent.md) — Agent for which the sensor is being run.

## Events

### **OnSensorRun**

Event that triggers when a sensor runs.

```csharp
public static event SensorRunEvent? OnSensorRun;
```
