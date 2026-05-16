using Builder.Tests.TestStates;
using Statement;
using Statement.Fluent.Api;

namespace Builder.Tests;

[TestFixture]
public class TypedStateMachineTests
{
    [Test]
    public void BuildTyped_ReturnsStateMachineOfBase()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .BuildTyped();

        Assert.That(machine, Is.InstanceOf<StateMachine<IUnitTestState>>());
        Assert.That(machine, Is.InstanceOf<StateMachine>());
    }

    [Test]
    public void GetCurrentState_ReturnsBaseTypeWithoutGenericArg()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        IUnitTestState current = machine.GetCurrentState();

        Assert.That(current, Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void SetCurrentState_TransitionsToRegisteredState()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<SimpleUnitTestState>();
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());

        machine.SetCurrentState<AdvancedUnitTestState>();
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void TryGetCurrentState_ReturnsTrueAndStateWhenSet()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        var ok = machine.TryGetCurrentState(out IUnitTestState? current);

        Assert.That(ok, Is.True);
        Assert.That(current, Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void TryGetCurrentState_ReturnsFalseWhenNoCurrentState()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .BuildTyped();

        var ok = machine.TryGetCurrentState(out IUnitTestState? current);

        Assert.That(ok, Is.False);
        Assert.That(current, Is.Null);
    }

    [Test]
    public void BuildTyped_OnUntypedBuilder_Throws()
    {
        var builder = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>();

        Assert.Throws<InvalidOperationException>(() => builder.BuildTyped());
    }

    [Test]
    public void Build_OnTypedBuilder_StillReturnsStateMachineOfBase()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .Build();

        Assert.That(machine, Is.InstanceOf<StateMachine<IUnitTestState>>());
    }

    [Test]
    public void TypedMachine_RespectsTransitionRules()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(s => s.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<SimpleUnitTestState>();
        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }
}
