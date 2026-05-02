using System;

namespace Statement.Fluent.Api;

public class StateDecorator
{
    public StateMachine Machine { get; set; }
    public Type InnerStateType { get; set; }
}