namespace Statement.Failures;

/// <summary>
/// Reason a fired trigger could not produce a transition.
/// </summary>
public enum TriggerFailureReason
{
    /// <summary>The current state has no handler for the fired trigger.</summary>
    NoHandler,

    /// <summary>A handler exists, but its guard predicate returned <c>false</c>.</summary>
    GuardFailed,

    /// <summary>
    /// A handler was invoked but threw an exception during execution.
    /// This typically occurs while draining queued triggers, where a single trigger's
    /// failure is reported through the failure policy so that remaining queued triggers
    /// can still be processed.
    /// </summary>
    HandlerThrew,

    /// <summary>The trigger was skipped because the machine's <see cref="StateMachineState"/> is <see cref="StateMachineState.Paused"/>.</summary>
    MachinePaused,
}
