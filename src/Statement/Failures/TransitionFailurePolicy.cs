using System;

namespace Statement.Failures;

/// <summary>
/// Controls how the state machine reacts when a transition is blocked by a configured rule.
/// Pick one of <see cref="Silent"/>, <see cref="Throw"/>, or <see cref="Invoke"/> and pass it to
/// <c>StateMachineBuilder.OnTransitionFailure</c>.
/// </summary>
public abstract class TransitionFailurePolicy
{
    /// <summary>Default policy: blocked transitions are silently ignored.</summary>
    public static TransitionFailurePolicy Silent { get; } = new SilentPolicy();

    /// <summary>Blocked transitions raise a <see cref="TransitionFailedException"/>.</summary>
    public static TransitionFailurePolicy Throw { get; } = new ThrowPolicy();

    /// <summary>Blocked transitions invoke the supplied callback with details of the attempt.</summary>
    /// <param name="callback">Callback invoked for every blocked transition.</param>
    /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <c>null</c>.</exception>
    public static TransitionFailurePolicy Invoke(Action<TransitionFailureInfo> callback)
    {
        if (callback is null) throw new ArgumentNullException(nameof(callback));
        return new CallbackPolicy(callback);
    }

    internal abstract void Handle(TransitionFailureInfo info);

    private sealed class SilentPolicy : TransitionFailurePolicy
    {
        internal override void Handle(TransitionFailureInfo info) { }
    }

    private sealed class ThrowPolicy : TransitionFailurePolicy
    {
        internal override void Handle(TransitionFailureInfo info)
            => throw new TransitionFailedException(info);
    }

    private sealed class CallbackPolicy : TransitionFailurePolicy
    {
        private readonly Action<TransitionFailureInfo> _callback;
        internal CallbackPolicy(Action<TransitionFailureInfo> callback) => _callback = callback;
        internal override void Handle(TransitionFailureInfo info) => _callback(info);
    }
}
