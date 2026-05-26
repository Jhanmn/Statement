namespace Statement.Failures;

/// <summary>
/// Reason a transition attempt did not produce a state change.
/// </summary>
public enum TransitionFailureReason
{
    /// <summary>The transition was disallowed by a configured transition rule.</summary>
    BlockedByRule,

    /// <summary>The transition was skipped because the machine's <see cref="StateMachineState"/> is <see cref="StateMachineState.Paused"/>.</summary>
    MachinePaused
}
