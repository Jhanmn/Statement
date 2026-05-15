namespace Statement;

public interface IStateMachine
{
    void SetCurrentState<T>();
    T GetCurrentState<T>() where T : class;
    object? GetCurrentState();
    T? TryGetCurrentState<T>(out bool result) where T : class;
}