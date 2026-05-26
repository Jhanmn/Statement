namespace Statement;

/// <summary>
/// Execution state of a <see cref="StateMachine"/>, controlling whether transitions and triggers are processed.
/// </summary>
public enum StateMachineState
{
    /// <summary>
    /// The machine processes transitions and triggers normally.
    /// </summary>
    Active,

    /// <summary>
    /// The machine is paused; transition and trigger requests are reported through the configured failure policy and otherwise ignored.
    /// </summary>
    Paused
}
