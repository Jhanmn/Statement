using Statement.Failures;
using Statement.Fluent.Api;
using Statement.Tests.TestStates;

namespace Statement.Tests;

[TestFixture]
public class PauseTests
{
    public sealed record Go;

    [Test]
    public void Deactivate_SetsExecutionStateToPaused()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Pause();

        Assert.That(machine.ExecutionState, Is.EqualTo(StateMachineState.Paused));
    }

    [Test]
    public void Activate_RestoresExecutionStateToActive()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Pause();
        machine.Resume();

        Assert.That(machine.ExecutionState, Is.EqualTo(StateMachineState.Active));
    }

    [Test]
    public void DefaultExecutionState_IsActive()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        Assert.That(machine.ExecutionState, Is.EqualTo(StateMachineState.Active));
    }

    [Test]
    public void SetCurrentState_WhenPaused_DoesNotTransition()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void SetCurrentState_WhenPaused_InvokesFailurePolicyWithMachinePausedReason()
    {
        TransitionFailureInfo? captured = null;
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Invoke(info => captured = info))
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Reason, Is.EqualTo(TransitionFailureReason.MachinePaused));
        Assert.That(captured.To, Is.EqualTo(typeof(AdvancedUnitTestState)));
    }

    [Test]
    public void SetCurrentState_WhenPaused_WithThrowPolicy_RaisesException()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Throw)
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();

        var ex = Assert.Throws<TransitionFailedException>(
            () => machine.SetCurrentState<AdvancedUnitTestState>());
        Assert.That(ex!.Info.Reason, Is.EqualTo(TransitionFailureReason.MachinePaused));
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public async Task SetCurrentStateAsync_WhenPaused_DoesNotTransition()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        await machine.SetCurrentStateAsync<AdvancedUnitTestState>();

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void Fire_WhenPaused_DoesNotTransition()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(s => s.On<Go>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        machine.Fire(new Go());

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void Fire_WhenPaused_InvokesTriggerFailurePolicyWithMachinePausedReason()
    {
        TriggerFailureInfo? captured = null;
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTriggerFailure(TriggerFailurePolicy.Invoke(info => captured = info))
            .AddState<SimpleUnitTestState>(s => s.On<Go>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        var trigger = new Go();
        machine.Pause();
        machine.Fire(trigger);

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Reason, Is.EqualTo(TriggerFailureReason.MachinePaused));
        Assert.That(captured.Trigger, Is.SameAs(trigger));
    }

    [Test]
    public async Task FireAsync_WhenPaused_DoesNotTransition()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(s => s.On<Go>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        await machine.FireAsync(new Go());

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void Activate_AfterDeactivate_ResumesTransitions()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        machine.SetCurrentState<AdvancedUnitTestState>();
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());

        machine.Resume();
        machine.SetCurrentState<AdvancedUnitTestState>();
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void SetCurrentState_WithPayload_WhenPaused_DoesNotTransition()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        machine.SetCurrentState<AdvancedUnitTestState>(payload: "data");

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void SetCurrentState_WithPayload_WhenPaused_InvokesFailurePolicyOnceWithMachinePausedReason()
    {
        var calls = new List<TransitionFailureInfo>();
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Invoke(info => calls.Add(info)))
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        machine.SetCurrentState<AdvancedUnitTestState>(payload: "data");

        Assert.That(calls, Has.Count.EqualTo(1));
        Assert.That(calls[0].Reason, Is.EqualTo(TransitionFailureReason.MachinePaused));
    }

    [Test]
    public void SetCurrentState_WithPayload_WhenActive_DoesNotInvokeFailurePolicy()
    {
        var calls = new List<TransitionFailureInfo>();
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Invoke(info => calls.Add(info)))
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<AdvancedUnitTestState>(payload: "data");

        Assert.That(calls, Is.Empty);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void SetCurrentState_NoPayload_WhenActive_DoesNotInvokeFailurePolicy()
    {
        var calls = new List<TransitionFailureInfo>();
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Invoke(info => calls.Add(info)))
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(calls, Is.Empty);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public async Task SetCurrentStateAsync_WithPayload_WhenPaused_DoesNotTransition()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        await machine.SetCurrentStateAsync<AdvancedUnitTestState>(payload: "data");

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public async Task SetCurrentStateAsync_WithPayload_WhenPaused_InvokesFailurePolicyOnceWithMachinePausedReason()
    {
        var calls = new List<TransitionFailureInfo>();
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Invoke(info => calls.Add(info)))
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        await machine.SetCurrentStateAsync<AdvancedUnitTestState>(payload: "data");

        Assert.That(calls, Has.Count.EqualTo(1));
        Assert.That(calls[0].Reason, Is.EqualTo(TransitionFailureReason.MachinePaused));
    }

    [Test]
    public async Task SetCurrentStateAsync_WithPayload_WhenActive_DoesNotInvokeFailurePolicy()
    {
        var calls = new List<TransitionFailureInfo>();
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Invoke(info => calls.Add(info)))
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        await machine.SetCurrentStateAsync<AdvancedUnitTestState>(payload: "data");

        Assert.That(calls, Is.Empty);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public async Task SetCurrentStateAsync_NoPayload_WhenActive_DoesNotInvokeFailurePolicy()
    {
        var calls = new List<TransitionFailureInfo>();
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Invoke(info => calls.Add(info)))
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        await machine.SetCurrentStateAsync<AdvancedUnitTestState>();

        Assert.That(calls, Is.Empty);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public async Task SetCurrentStateAsync_WhenPaused_InvokesFailurePolicyOnlyOnce()
    {
        var calls = new List<TransitionFailureInfo>();
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTransitionFailure(TransitionFailurePolicy.Invoke(info => calls.Add(info)))
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        await machine.SetCurrentStateAsync<AdvancedUnitTestState>();

        Assert.That(calls, Has.Count.EqualTo(1));
        Assert.That(calls[0].Reason, Is.EqualTo(TransitionFailureReason.MachinePaused));
    }

    [Test]
    public void Fire_WhenActive_DoesNotInvokeTriggerFailurePolicy()
    {
        var calls = new List<TriggerFailureInfo>();
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTriggerFailure(TriggerFailurePolicy.Invoke(info => calls.Add(info)))
            .AddState<SimpleUnitTestState>(s => s.On<Go>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Fire(new Go());

        Assert.That(calls, Is.Empty);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public async Task FireAsync_WhenActive_DoesNotInvokeTriggerFailurePolicy()
    {
        var calls = new List<TriggerFailureInfo>();
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .OnTriggerFailure(TriggerFailurePolicy.Invoke(info => calls.Add(info)))
            .AddState<SimpleUnitTestState>(s => s.On<Go>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        await machine.FireAsync(new Go());

        Assert.That(calls, Is.Empty);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void SetCurrentStateAsync_WhenPausedAndCancelled_HonorsCancellation()
    {
        StateMachine machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        machine.Pause();

        Assert.That(
            async () => await machine.SetCurrentStateAsync<AdvancedUnitTestState>(cts.Token),
            Throws.InstanceOf<OperationCanceledException>(),
            "Paused async path should still honor an already-cancelled token, " +
            "matching the non-paused path's contract.");
    }

    [Test]
    public void FireAsync_WhenPausedAndCancelled_HonorsCancellation()
    {
        StateMachine machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<Go>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        machine.Pause();

        Assert.That(
            async () => await machine.FireAsync(new Go(), cts.Token),
            Throws.InstanceOf<OperationCanceledException>(),
            "Paused async path should still honor an already-cancelled token.");
    }

    [Test]
    public void Deactivate_RacingWithInFlightTransition_DoesNotCorruptState()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        const int iterations = 200;
        Exception? error = null;

        var transitions = Task.Run(() =>
        {
            try
            {
                for (var i = 0; i < iterations; i++)
                {
                    machine.SetCurrentState<AdvancedUnitTestState>();
                    machine.SetCurrentState<SimpleUnitTestState>();
                }
            }
            catch (Exception ex) { error = ex; }
        });

        var toggles = Task.Run(() =>
        {
            try
            {
                for (var i = 0; i < iterations; i++)
                {
                    machine.Pause();
                    machine.Resume();
                }
            }
            catch (Exception ex) { error = ex; }
        });

        Task.WaitAll(transitions, toggles);

        Assert.That(error, Is.Null, "Concurrent Deactivate/Activate must not corrupt or throw.");
        machine.Resume();
        Assert.That(
            machine.GetCurrentState(),
            Is.TypeOf<SimpleUnitTestState>().Or.TypeOf<AdvancedUnitTestState>(),
            "Machine must remain in a registered state after concurrent pause toggling.");
    }

    [Test]
    public void Activate_AfterDeactivate_ResumesTriggers()
    {
        var machine = StateMachineBuilder.For<IUnitTestState>()
            .AddState<SimpleUnitTestState>(s => s.On<Go>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .BuildTyped();

        machine.Pause();
        machine.Fire(new Go());
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());

        machine.Resume();
        machine.Fire(new Go());
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }
}
