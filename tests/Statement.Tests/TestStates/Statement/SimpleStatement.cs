using Statement.State;

namespace Statement.Tests.TestStates.Statement;

public class SimpleStatement : IStatement
{
    public bool OnEntryCalled { get; set; }
    public bool OnExitCalled { get; set; }
    
    public void OnEntry()
    {
        OnEntryCalled = true;
    }

    public void OnExit()
    {
        OnExitCalled = true;
    }
}