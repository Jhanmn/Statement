using Statement.Tests.TestStates;
using Statement.Tests.TestStates.Statement;
using Statement.Fluent.Api;

namespace Statement.Tests;

public class StateMachineTests
{
    private StateMachine _machine = null!;

    [SetUp]
    public void Setup()
    {
        _machine = StateMachineBuilder.New()
            .AddState<InitialUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<InitialUnitTestState>()
            .Build();
    }

    [Test]
    public void SetCurrentState_FromInitial_SetsState()
    {
        _machine.SetCurrentState<SimpleUnitTestState>();

        Assert.That(_machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void SetCurrentState_BetweenStates_SwitchesState()
    {
        _machine.SetCurrentState<SimpleUnitTestState>();
        _machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(_machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void SetCurrentState_UnregisteredType_Throws()
    {
        _machine.SetCurrentState<SimpleUnitTestState>();

        Assert.Throws<InvalidOperationException>(() => _machine.SetCurrentState<UnregisteredState>());
        Assert.That(_machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void SetCurrentState_FirstTransition_DoesNotInvokeOnExit()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleStatement>()
            .StartIn<SimpleStatement>()
            .Build();

        var current = machine.GetCurrentState<SimpleStatement>();
        Assert.That(current.OnEntryCalled, Is.True);
        Assert.That(current.OnExitCalled, Is.False);
    }

    [Test]
    public void SetCurrentState_TransitionInvokesOnExitThenOnEntry()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleStatement>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleStatement>()
            .Build();

        var simple = machine.GetCurrentState<SimpleStatement>();

        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(simple.OnExitCalled, Is.True);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void SetCurrentState_DuringOnExit_CurrentStateIsStillExitingState()
    {
        object? currentDuringExit = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.OnExit((_, m) => currentDuringExit = m.GetCurrentState()))
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(currentDuringExit, Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void SetCurrentState_DuringOnEntry_CurrentStateIsNewState()
    {
        object? currentDuringEntry = null;
        var machine = StateMachineBuilder.New()
            .AddState<InitialUnitTestState>()
            .AddState<SimpleUnitTestState>(s => s.OnEntry((_, m) => currentDuringEntry = m.GetCurrentState()))
            .AddState<AdvancedUnitTestState>()
            .StartIn<InitialUnitTestState>()
            .Build();

        machine.SetCurrentState<SimpleUnitTestState>();

        Assert.That(currentDuringEntry, Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void SetCurrentState_ForbiddenTransition_DoesNotChangeStateOrCallCallbacks()
    {
        var onExitCalled = false;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .OnExit(() => onExitCalled = true)
                .CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
        Assert.That(onExitCalled, Is.False);
    }

    [Test]
    public void GetCurrentStateGeneric_WhenMatchingType_ReturnsInstance()
    {
        _machine.SetCurrentState<SimpleUnitTestState>();

        var state = _machine.GetCurrentState<IUnitTestState>();

        Assert.That(state, Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void GetCurrentStateGeneric_WhenNoMatchingType_Throws()
    {
        _machine.SetCurrentState<SimpleUnitTestState>();

        Assert.Throws<InvalidOperationException>(() => _machine.GetCurrentState<AdvancedUnitTestState>());
    }

    [Test]
    public void TryGetCurrentState_WhenMatching_ReturnsTrueAndInstance()
    {
        _machine.SetCurrentState<SimpleUnitTestState>();

        var success = _machine.TryGetCurrentState<IUnitTestState>(out var state);

        Assert.That(success, Is.True);
        Assert.That(state, Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void TryGetCurrentState_WhenNoMatch_ReturnsFalseAndNull()
    {
        _machine.SetCurrentState<SimpleUnitTestState>();

        var success = _machine.TryGetCurrentState<AdvancedUnitTestState>(out var state);

        Assert.That(success, Is.False);
        Assert.That(state, Is.Null);
    }

    [Test]
    public void CannotTransitionTo_BlocksTransition()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void OnEntry_TypedCallback_ReceivesStateInstance()
    {
        SimpleUnitTestState? captured = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.OnEntry((state, _) => captured = state))
            .StartIn<SimpleUnitTestState>()
            .Build();

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured, Is.SameAs(machine.GetCurrentState()));
    }

    [Test]
    public void OnExit_TypedCallback_FiresOnTransition()
    {
        var exited = false;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.OnExit((_, _) => exited = true))
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(exited, Is.True);
    }

    [Test]
    public void GlobalOnTransitionMethod_GetsCalledOnEntry()
    {
        var onEntryCalled = false;

        var machine = StateMachineBuilder.New()
            .AddOnStateChangedCallback(_ => onEntryCalled = true)
            .AddState<InitialUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<InitialUnitTestState>()
            .Build();

        machine.SetCurrentState<SimpleUnitTestState>();

        Assert.That(onEntryCalled, Is.True);
    }

    [Test]
    public void GlobalOnTransitionMethod_GetsCalledMultipleTimes()
    {
        var callCount = 0;

        var machine = StateMachineBuilder.New()
            .AddOnStateChangedCallback(_ => callCount++)
            .AddState<InitialUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<InitialUnitTestState>()
            .Build();

        callCount = 0;
        machine.SetCurrentState<SimpleUnitTestState>();
        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(callCount, Is.EqualTo(2));
    }

    [Test]
    public void GlobalTransitionMethod_CalledBefore_StateCallback()
    {
        DateTime globalTransitionTimeStamp = default;
        DateTime stateTransitionTimeStamp = default;

        var machine = StateMachineBuilder.New()
            .AddOnStateChangedCallback(_ => globalTransitionTimeStamp = DateTime.Now)
            .AddState<InitialUnitTestState>()
            .AddState<SimpleUnitTestState>(state => state.OnEntry(() => stateTransitionTimeStamp = DateTime.Now))
            .AddState<AdvancedUnitTestState>()
            .StartIn<InitialUnitTestState>()
            .Build();

        machine.SetCurrentState<SimpleUnitTestState>();

        Assert.That(globalTransitionTimeStamp < stateTransitionTimeStamp, Is.True);
    }

    [Test]
    public void GlobalTransitionMethod_TransitionInfo_ContainsCorrectState()
    {
        TransitionInformation? captured = null;
        var machine = StateMachineBuilder.New()
            .AddOnStateChangedCallback(info => captured = info)
            .AddState<InitialUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<InitialUnitTestState>()
            .Build();

        machine.SetCurrentState<SimpleUnitTestState>();

        Assert.That(captured!.Value.To, Is.TypeOf<SimpleUnitTestState>());
    }

    private class UnregisteredState
    {
    }

    [Test]
    public void GetAllRegisteredStateTypes_ReturnsAllRegisteredTypes()
    {
        var machine = StateMachineBuilder.New()
        .AddState<InitialUnitTestState>()
        .AddState<SimpleUnitTestState>()
        .AddState<AdvancedUnitTestState>()
        .StartIn<InitialUnitTestState>()
        .Build();

        var states = machine.GetAllRegisteredStateTypes();
        Assert.That(states, Has.Count.EqualTo(3));
        Assert.That(states, Contains.Item(typeof(InitialUnitTestState)));
        Assert.That(states, Contains.Item(typeof(SimpleUnitTestState)));
        Assert.That(states, Contains.Item(typeof(AdvancedUnitTestState)));
    }

    [Test]
    public void GetAllRegisteredStateInstances_ReturnsAllInstances()
    {
        var machine = StateMachineBuilder.New()
            .AddState<InitialUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<InitialUnitTestState>()
            .Build();

        var instances = machine.GetAllRegisteredStateInstances();

        Assert.That(instances, Has.Count.EqualTo(3));
        Assert.That(instances[0], Is.InstanceOf<InitialUnitTestState>());
        Assert.That(instances[1], Is.InstanceOf<SimpleUnitTestState>());
        Assert.That(instances[2], Is.InstanceOf<AdvancedUnitTestState>());
    }

    #region CanTransitionTo

    [Test]
    public void CanTransitionTo_AllowedTransition_ReturnsTrue()
    {
        _machine.SetCurrentState<SimpleUnitTestState>();

        Assert.That(_machine.CanTransitionTo(typeof(AdvancedUnitTestState)), Is.True);
    }

    [Test]
    public void CanTransitionTo_ForbiddenTransition_ReturnsFalse()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        Assert.That(machine.CanTransitionTo(typeof(AdvancedUnitTestState)), Is.False);
    }

    #endregion

    #region PossibleNextTransitions

    [Test]
    public void PossibleNextTransitions_NoRules_ReturnsAllStates()
    {
        _machine.SetCurrentState<SimpleUnitTestState>();

        var possible = _machine.PossibleNextTransitions().ToList();

        Assert.That(possible, Has.Count.EqualTo(3));
        Assert.That(possible, Contains.Item(typeof(InitialUnitTestState)));
        Assert.That(possible, Contains.Item(typeof(SimpleUnitTestState)));
        Assert.That(possible, Contains.Item(typeof(AdvancedUnitTestState)));
    }

    [Test]
    public void PossibleNextTransitions_WithForbidden_ExcludesForbiddenState()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .AddState<InitialUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        var possible = machine.PossibleNextTransitions().ToList();

        Assert.That(possible, Does.Not.Contain(typeof(AdvancedUnitTestState)));
        Assert.That(possible, Contains.Item(typeof(SimpleUnitTestState)));
        Assert.That(possible, Contains.Item(typeof(InitialUnitTestState)));
    }

    #endregion
}
