# Akka.Monitoring.Prometheus

[Monitoring system instrumentation](https://github.com/petabridge/akka-monitoring) in Prometheus for [Akka.NET](https://github.com/akkadotnet/akka.net) actor systems.

## Quickstart

### Add the `Akka.Monitoring.Prometheus` package to your project:

```bash
dotnet add package Akka.Monitoring.Prometheus
```

### Write code

1. Register the Prometheus monitor. From [Program](src/Akka.Monitoring.Prometheus.Demo/Program.cs):

```csharp
var system = ActorSystem.Create("akka-performance-demo");

var didMonitorRegister = ActorMonitoringExtension.RegisterMonitor(system, new ActorPrometheusMonitor());
```

2. Instrument your actor system as normal. From [PlutoActor](src/Akka.Monitoring.Prometheus.Demo/PlutoActor.cs):

```csharp
Receive<string>(_ =>
{
    Context.IncrementCounter("revolutions");
    Context.GetLogger().Debug("Whee!");
    Context.Gauge("revolutions.enjoyment", random.Next(1, 11));
});
```

For more information on instrumenting [Akka.NET](https://github.com/akkadotnet/akka.net) actor systems, please see [Akka.Monitoring](https://github.com/petabridge/akka-monitoring).

## Building

[![Travis](https://img.shields.io/travis/syncromatics/Akka.Monitoring.Prometheus.svg)](https://travis-ci.org/syncromatics/Akka.Monitoring.Prometheus)
[![NuGet](https://img.shields.io/nuget/v/Akka.Monitoring.Prometheus.svg)](https://www.nuget.org/packages/Akka.Monitoring.Prometheus/)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/Akka.Monitoring.Prometheus.svg)](https://www.nuget.org/packages/Akka.Monitoring.Prometheus/)

The package targets .NET Standard 2.0 and can be built via [.NET Core](https://www.microsoft.com/net/core):

```bash
dotnet build
```

## Code of Conduct

We are committed to fostering an open and welcoming environment. Please read our [code of conduct](CODE_OF_CONDUCT.md) before participating in or contributing to this project.

## Contributing

We welcome contributions and collaboration on this project. Please read our [contributor's guide](CONTRIBUTING.md) to understand how best to work with us.

## License and Authors

[![Syncromatics Engineering logo](https://en.gravatar.com/userimage/100017782/89bdc96d68ad4b23998e3cdabdeb6e13.png?size=16) Syncromatics Engineering](https://github.com/syncromatics)

[![license](https://img.shields.io/github/license/syncromatics/Akka.Monitoring.Prometheus.svg)](https://github.com/syncromatics/Akka.Monitoring.Prometheus/blob/master/LICENSE)
[![GitHub contributors](https://img.shields.io/github/contributors/syncromatics/Akka.Monitoring.Prometheus.svg)](https://github.com/syncromatics/Akka.Monitoring.Prometheus/graphs/contributors)

This software is made available by Syncromatics Engineering under the MIT license.