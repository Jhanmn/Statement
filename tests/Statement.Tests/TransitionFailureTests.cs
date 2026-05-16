using System;
using Statement.Tests.TestStates;
using Statement.Failures;
using Statement.Fluent.Api;

namespace Statement.Tests;

[TestFixture]
public class TransitionFailureTests
{
    [Test]
    public void DefaultPolicy_IsSilent_BlockedTransitionIgnored()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(s => s.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<SimpleUnitTestState>();
        Assert.DoesNotThrow(() => machine.SetCurrentState<AdvancedUnitTestState>());
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void ThrowPolicy_BlockedTransition_RaisesTransitionFailedException()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Throw)
            .AddState<SimpleUnitTestState>(s => s.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<SimpleUnitTestState>();

        var ex = Assert.Throws<TransitionFailedException>(
            () => machine.SetCurrentState<AdvancedUnitTestState>());
        Assert.That(ex!.Info.From, Is.EqualTo(typeof(SimpleUnitTestState)));
        Assert.That(ex.Info.To, Is.EqualTo(typeof(AdvancedUnitTestState)));
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void InvokePolicy_BlockedTransition_FiresCallbackWithDetails()
    {
        TransitionFailureInfo? captured = null;
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Invoke(info => captured = info))
            .AddState<SimpleUnitTestState>(s => s.CannotTransitionTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<SimpleUnitTestState>();
        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.From, Is.EqualTo(typeof(SimpleUnitTestState)));
        Assert.That(captured.To, Is.EqualTo(typeof(AdvancedUnitTestState)));
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void UnregisteredTarget_AlwaysThrows_RegardlessOfPolicy()
    {
        var machine = StateMachineBuilder.New()
            .OnTransitionFailure(TransitionFailurePolicy.Silent)
            .AddState<SimpleUnitTestState>()
            .Build();

        Assert.Throws<InvalidOperationException>(
            () => machine.SetCurrentState<AdvancedUnitTestState>());
    }
}
