using Statement.Tests.TestStates;
using Statement.Tests.TestStates.Statement;
using Statement.Fluent.Api;

namespace Statement.Tests;

[TestFixture]
public class StatementStateMachineTests
{
    [Test]
    public void OnEntryCalledOnStatement()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleStatement>()
            .StartIn<SimpleStatement>()
            .Build();

        var result = machine.GetCurrentState<SimpleStatement>();
        Assert.That(result.OnEntryCalled, Is.True);
    }

    [Test]
    public void OnExitCalledOnStatement()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleStatement>()
            .AddState<SimpleUnitTestState>()
            .StartIn<SimpleStatement>()
            .Build();

        var result = machine.GetCurrentState<SimpleStatement>();
        machine.SetCurrentState<SimpleUnitTestState>();

        Assert.That(result.OnExitCalled, Is.True);
    }
}
