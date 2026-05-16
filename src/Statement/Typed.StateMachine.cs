namespace Statement;

public class StateMachine<T> : StateMachine where T : class
{
    internal StateMachine() { }

    public new T GetCurrentState() => GetCurrentState<T>();

    public bool TryGetCurrentState(out T? result) => TryGetCurrentState<T>(out result);

    public new void SetCurrentState<TState>() where TState : T => SetCurrentStateByType(typeof(TState));
}
