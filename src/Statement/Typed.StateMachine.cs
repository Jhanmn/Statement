using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statement;

/// <summary>
/// A strongly-typed <see cref="StateMachine"/> whose registered states all derive from or implement <typeparamref name="T"/>.
/// Provides typed overloads of the state accessors so callers don't have to specify the common base type at every call site.
/// </summary>
/// <typeparam name="T">The common base type or interface shared by every state registered on this machine.</typeparam>
public class StateMachine<T> : StateMachine where T : class
{
    internal StateMachine() { }

    /// <summary>
    /// Returns the current state instance as <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown if no state is currently set.</exception>
    public new T GetCurrentState() => GetCurrentState<T>();

    /// <summary>
    /// Attempts to get the current state instance as <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">When this method returns <c>true</c>, contains the current state instance; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if a current state is set; otherwise <c>false</c>.</returns>
    public bool TryGetCurrentState(out T? result) => TryGetCurrentState<T>(out result);

    /// <summary>
    /// Transitions the machine to the registered state of type <typeparamref name="TState"/>.
    /// The transition is silently ignored if the target state is not registered or is blocked by a transition rule.
    /// </summary>
    /// <typeparam name="TState">The state type to switch to. Must derive from or implement <typeparamref name="T"/>.</typeparam>
    public new void SetCurrentState<TState>() where TState : T
        => base.SetCurrentState<TState>();

    /// <summary>
    /// Transitions to <typeparamref name="TState"/> and carries a typed <paramref name="payload"/> through to
    /// the target state's <c>OnEntryWith</c> callback.
    /// </summary>
    public new void SetCurrentState<TState>(object? payload) where TState : T 
        => base.SetCurrentState<TState>( payload);

    /// <summary>
    /// Asynchronously transitions to <typeparamref name="TState"/>.
    /// </summary>
    public Task SetCurrentStateAsync<TState>() where TState : T => base.SetCurrentStateAsync<TState>();

    /// <summary>
    /// Asynchronously transitions to <typeparamref name="TState"/> with a typed <paramref name="payload"/>.
    /// </summary>
    public Task SetCurrentStateAsync<TState>(object? payload) where TState : T => base.SetCurrentStateAsync<TState>(payload);

    /// <summary>
    /// Returns all registered state instances as <typeparamref name="T"/>.
    /// </summary>
    public new IList<T> GetAllRegisteredStateInstances()
        => base.GetAllRegisteredStateInstances().Cast<T>().ToList();

    /// <summary>
    /// Checks whether a transition from the current state to <typeparamref name="TState"/> is permitted
    /// by the current state's transition rules.
    /// </summary>
    /// <typeparam name="TState">The target state type to check. Must derive from or implement <typeparamref name="T"/>.</typeparam>
    /// <remarks>will throw <see cref="System.InvalidOperationException"/> if method was called before final build of <see cref="StateMachine"/></remarks>
    /// <returns><c>true</c> if the transition is allowed; otherwise <c>false</c>.</returns>
    public bool CanTransitionTo<TState>() where TState : T => CanTransitionTo(typeof(TState));
}
