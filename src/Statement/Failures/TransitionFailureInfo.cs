using System;

namespace Statement.Failures;

/// <summary>
/// Describes a transition attempt that was blocked by a configured rule.
/// </summary>
public sealed class TransitionFailureInfo
{
    /// <summary>The state the machine was in when the transition was attempted, or <c>null</c> if no state was set.</summary>
    public Type? From { get; }

    /// <summary>The state the machine was attempting to switch to.</summary>
    public Type To { get; }

    internal TransitionFailureInfo(Type? from, Type to)
    {
        From = from;
        To = to;
    }
}
