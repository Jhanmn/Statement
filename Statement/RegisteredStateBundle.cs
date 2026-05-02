using System;

namespace Statement;

public class RegisteredStateBundle(Type registeredState)
{
    public Type RegisteredState { get; set; } = registeredState;
    public Action<StateMachine>? OnEntryCallback { get; set; }
    public Action<StateMachine>? OnExitCallback { get; set; }
}