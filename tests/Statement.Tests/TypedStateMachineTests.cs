using Statement.Tests.TestStates;
using Statement;
using Statement.Fluent.Api;

namespace Statement.Tests;

[TestFixture]
public class TypedStateMachineTests
{
    [Test]
    public void BuildTyped_ReturnsStateMachineOfBase()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .StartIn<SimpleUnitTestState>()
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
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

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
    public void BuildTyped_OnUntypedBuilder_Throws()
    {
        var builder = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .StartIn<SimpleUnitTestState>();

        Assert.Throws<InvalidOperationException>(() => builder.BuildTyped());
    }

    [Test]
    public void Build_OnTypedBuilder_StillReturnsStateMachineOfBase()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        Assert.That(machine, Is.InstanceOf<StateMachine<IUnitTestState>>());
    }

    [Test]
    public void TypedMachine_RespectsTransitionRules()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(state
                => state.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void CannotTransitionTo_ChainedCalls_ForbidsAllTargets()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(state => state
                .CannotTransitionTo<AdvancedUnitTestState>()
                .CannotTransitionTo<ExtraUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<AdvancedUnitTestState>();
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());

        machine.SetCurrentState<ExtraUnitTestState>();
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }
}
