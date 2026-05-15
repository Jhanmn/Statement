using Builder.Tests.TestStates;
using Statement;
using Statement.Fluent.Api;

namespace Builder.Tests;

[TestFixture]
public class CompileStateMachineTests
{
    private StateMachine _machine;
    
    [SetUp]
    public void Setup()
    {
        _machine = new StateMachine();
    }

    [Test]
    public void CompileAgainst_HappyPath()
    {
        _machine.AddState<SimpleUnitTestState>();
        _machine.AddState<AdvancedUnitTestState>();
        
        _machine.CompileAgainst<IUnitTestState>();
        Assert.Pass();
        //assert success if no exception is thrown
    }
    
    [Test]
    public void CompileAgainst_InvalidState_ThrowsException()
    {
        _machine.AddState<SimpleUnitTestState>();
        _machine.AddState<AdvancedUnitTestState>();
        _machine.AddState<StatementStateMachineTests>();//invalid - not a state
        
        Assert.Throws<InvalidOperationException>(() => _machine.CompileAgainst<IUnitTestState>());
    }
    
    [Test]
    public void Compile_WrongState_ThrowsNoException()
    {
        _machine.AddState<SimpleUnitTestState>();
        _machine.AddState<AdvancedUnitTestState>();
        _machine.AddState<StatementStateMachineTests>();

        _machine.Compile();
        Assert.Pass();        
    }
    
    [Test]
    public void Compile_RightState_ThrowsNoException()
    {
        _machine.AddState<SimpleUnitTestState>();
        _machine.AddState<AdvancedUnitTestState>();

        _machine.Compile();
        Assert.Pass();        
    }
}