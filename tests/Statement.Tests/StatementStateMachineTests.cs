using Builder.Tests.TestStates;
using Builder.Tests.TestStates.Statement;
using Statement;
using Statement.Fluent.Api;

namespace Builder.Tests;

[TestFixture]
public class StatementStateMachineTests
{
    private StateMachine _machine;
    
    [SetUp]
    public void Setup()
    {
        _machine = new StateMachine();
    }
    
    [Test]
    public void OnEntryCalledOnStatement()
    {
        _machine.AddState<SimpleStatement>();
        _machine.Compile();
        
        _machine.SetCurrentState<SimpleStatement>();

        var result = _machine.GetCurrentState<SimpleStatement>();
        
        Assert.That(result.OnEntryCalled, Is.True);
    }

    [Test]
    public void OnExitCalledOnStatement()
    {
        _machine.AddState<SimpleStatement>();
        _machine.AddState<SimpleUnitTestState>();
        _machine.Compile();
        
        _machine.SetCurrentState<SimpleStatement>();
        var result = _machine.GetCurrentState<SimpleStatement>();
        _machine.SetCurrentState<SimpleUnitTestState>();
        
        Assert.That(result.OnExitCalled, Is.True);
    }
}