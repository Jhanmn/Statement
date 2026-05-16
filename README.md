# Statement

[![CI](https://github.com/Jhanmn/Statement/actions/workflows/ci.yml/badge.svg)](https://github.com/Jhanmn/Statement/actions/workflows/ci.yml)
[![License](https://img.shields.io/github/license/Jhanmn/Statement)](License)
[![.NET Standard 2.0](https://img.shields.io/badge/target-.NET%20Standard%202.0-512BD4)](https://learn.microsoft.com/dotnet/standard/net-standard)

A lightweight, type-driven state machine library for .NET.

In Statement, **each state is its own class**. Transitions are expressed by switching the machine's current state type, and entry/exit behavior lives either on the state itself (via `IStatement`) or on the machine through a fluent builder API. Optional base-type constraints let you guarantee at compile time that every state in a machine implements a common interface or inherits from a common base class.

## Features

- States as first-class types — no string identifiers, no enums.
- Fluent `StateMachineBuilder` API with `OnEntry` / `OnExit` / `CannotTransitionTo` rules.
- Optional typed machines (`StateMachineBuilder.For<TBase>()`) for compile-time safety.
- Register states by type (auto-instantiated) or by pre-built instance (for states with constructor arguments).
- Built-in `IStatement` interface for states that prefer to own their own entry/exit logic.

## Quick start

Install the project as a reference (NuGet package coming later) and define your states:

```csharp
// States are just plain classes — implementing IStatement is optional.
public class Idle { }

// Implement IStatement only if the state wants to own its entry/exit logic.
public class Running : IStatement
{
    public void OnEntry() => Console.WriteLine("started");
    public void OnExit()  => Console.WriteLine("stopped");
}
```

Build a machine and drive it by type:

```csharp
using Statement.Fluent.Api;

var machine = StateMachineBuilder.New()
    .AddState<Idle>()
    .AddState<Running>(s => s.CannotTransitionTo<Idle>()) // example rule
    .StartIn<Idle>()
    .Build();

machine.SetCurrentState<Running>();          // fires Running.OnEntry
var current = machine.GetCurrentState();     // returns the Running instance
```

### Typed machine with a shared base type

```csharp
var machine = StateMachineBuilder.For<IMyState>()
    .AddState<Connecting>()
    .AddState<Connected>()
    .StartIn<Connecting>()
    .BuildTyped();   // StateMachine<IMyState>

IMyState state = machine.GetCurrentState<IMyState>();
```

### Pre-built state instances

For states that need constructor arguments or dependencies:

```csharp
var configured = new WithConfig("hello");

var machine = StateMachineBuilder.New()
    .AddState<WithConfig>(configured)
    .Build();
```

## Examples

For more usage patterns — entry/exit callbacks, transition rules, typed machines, pre-built instances, and `IStatement` states — see the unit tests under [tests/Statement.Tests](tests/Statement.Tests).

## License

See [License](License).
