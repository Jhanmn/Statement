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

    private sealed record FileData(string Path);

    [Test]
    public void SetCurrentState_WithPayload_TransitionsAndDeliversPayload()
    {
        FileData? captured = null;
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>(s => s.OnEntryWith<FileData>(p => captured = p))
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        var payload = new FileData("foo.txt");
        machine.SetCurrentState<AdvancedUnitTestState>(payload);

        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
        Assert.That(captured, Is.SameAs(payload));
    }

    [Test]
    public void SetCurrentState_WithPayload_RespectsTransitionRules()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(s => s.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<AdvancedUnitTestState>(new FileData("blocked.txt"));

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public async Task SetCurrentStateAsync_TransitionsToRegisteredState()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        await machine.SetCurrentStateAsync<AdvancedUnitTestState>();

        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public async Task SetCurrentStateAsync_WithPayload_TransitionsAndDeliversPayload()
    {
        FileData? captured = null;
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>(s => s.OnEntryWith<FileData>(p => captured = p))
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        var payload = new FileData("bar.txt");
        await machine.SetCurrentStateAsync<AdvancedUnitTestState>(payload);

        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
        Assert.That(captured, Is.SameAs(payload));
    }

    [Test]
    public async Task SetCurrentStateAsync_RespectsTransitionRules()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(s => s.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        await machine.SetCurrentStateAsync<AdvancedUnitTestState>();

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void GetAllRegisteredStateInstances_ReturnsInstancesTypedAsBase()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        IList<IUnitTestState> instances = machine.GetAllRegisteredStateInstances();

        Assert.That(instances, Has.Count.EqualTo(3));
        Assert.That(instances, Has.All.InstanceOf<IUnitTestState>());
        Assert.That(instances.Select(i => i.GetType()), Is.EquivalentTo(new[]
        {
            typeof(SimpleUnitTestState), typeof(AdvancedUnitTestState), typeof(ExtraUnitTestState)
        }));
    }

    [Test]
    public void CanTransitionTo_Generic_ReturnsTrueWhenAllowed()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        Assert.That(machine.CanTransitionTo<AdvancedUnitTestState>(), Is.True);
    }

    [Test]
    public void CanTransitionTo_Generic_ReturnsFalseWhenForbidden()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(state
                => state.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        Assert.That(machine.CanTransitionTo<AdvancedUnitTestState>(), Is.False);
    }

    [Test]
    public void CanTransitionTo_Generic_MatchesTypeOverload()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(state
                => state.CannotTransitionTo<ExtraUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        Assert.That(machine.CanTransitionTo<AdvancedUnitTestState>(),
            Is.EqualTo(machine.CanTransitionTo(typeof(AdvancedUnitTestState))));
        Assert.That(machine.CanTransitionTo<ExtraUnitTestState>(),
            Is.EqualTo(machine.CanTransitionTo(typeof(ExtraUnitTestState))));
    }

    [Test]
    public void CanTransitionTo_Generic_ReflectsCurrentStateAfterTransition()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>(state
                => state.CannotTransitionTo<ExtraUnitTestState>())
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        Assert.That(machine.CanTransitionTo<ExtraUnitTestState>(), Is.True);

        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(machine.CanTransitionTo<ExtraUnitTestState>(), Is.False);
    }

    [Test]
    public void PossibleNextTransitions_OnTypedMachine_ReflectsRules()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(state
                => state.CannotTransitionTo<ExtraUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        var possible = machine.PossibleNextTransitions().ToList();

        Assert.That(possible, Does.Contain(typeof(SimpleUnitTestState)));
        Assert.That(possible, Does.Contain(typeof(AdvancedUnitTestState)));
        Assert.That(possible, Does.Not.Contain(typeof(ExtraUnitTestState)));
    }

    [Test]
    public void GetAllRegisteredStateTypes_OnTypedMachine_ReturnsAllTypes()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        Assert.That(machine.GetAllRegisteredStateTypes(),
            Is.EquivalentTo(new[] { typeof(SimpleUnitTestState), typeof(AdvancedUnitTestState) }));
    }
}
