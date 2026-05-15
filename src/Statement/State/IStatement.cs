namespace Statement.State;

public interface IStatement
{
    void OnEntry();
    void OnExit();
}