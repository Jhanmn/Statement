using System;

namespace Statement.Failures;

/// <summary>
/// Thrown by <see cref="TransitionFailurePolicy.Throw"/> when a transition is blocked by a configured rule.
/// </summary>
public class TransitionFailedException : Exception
{
    /// <summary>Details about the blocked transition.</summary>
    public TransitionFailureInfo Info { get; }

    internal TransitionFailedException(TransitionFailureInfo info)
        : base($"Transition from {info.From?.Name ?? "<none>"} to {info.To.Name} was blocked by a rule.")
    {
        Info = info;
    }
}
