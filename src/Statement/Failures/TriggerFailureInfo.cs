using System;

namespace Statement.Failures;

/// <summary>
/// Describes a fired trigger that did not produce a transition.
/// </summary>
public sealed record TriggerFailureInfo
{
    /// <summary>
    /// The state the machine was in when the trigger was fired.
    /// </summary>
    public Type? CurrentState { get; }

    /// <summary>
    /// The trigger value that was fired.
    /// </summary>
    public object Trigger { get; }

    /// <summary>
    /// Why the trigger did not produce a transition.
    /// </summary>
    public TriggerFailureReason Reason { get; }

    /// <summary>
    /// can carry the exception which was thrown during the error.
    /// </summary>
    public Exception? Exception { get; }

    internal TriggerFailureInfo(Type? currentState, object trigger, TriggerFailureReason reason, Exception? exception = null)
    {
        CurrentState = currentState;
        Trigger = trigger;
        Reason = reason;
        Exception = exception;
    }

}
