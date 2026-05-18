using System;

namespace Statement.Triggers;

internal sealed class TriggerHandler
{
    internal Type? Target { get; }
    internal Func<object?, bool>? Guard { get; }
    internal Action<object>? OnFire { get; }

    internal TriggerHandler(Type? target, Func<object?, bool>? guard, Action<object>? onFire)
    {
        Target = target;
        Guard = guard;
        OnFire = onFire;
    }
}
