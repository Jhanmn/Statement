using Statement.Fluent.Api;
using Statement.Tests.TestStates;

namespace Statement.Tests;

file sealed class TriggerA;
file sealed class TriggerB;
file sealed class TriggerC;

[TestFixture]
public class EnqueueTests
{
    /// <summary>
    /// Called outside any transition, <c>Enqueue</c> behaves like <c>Fire</c>:
    /// the trigger is processed immediately and the state has changed by the
    /// time the call returns.
    /// </summary>
    [Test]
    public void Enqueue_OutsideTransition_FiresImmediately()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<TriggerA>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Enqueue(new TriggerA(), null);

        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    /// <summary>
    /// Called outside any transition, <c>EnqueueAsync</c> behaves like
    /// <c>FireAsync</c>: the returned task completes when the trigger has been
    /// processed.
    /// </summary>
    [Test]
    public async Task EnqueueAsync_OutsideTransition_FiresImmediately()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<TriggerA>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        await machine.EnqueueAsync(new TriggerA(), null);

        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    /// <summary>
    /// When <c>Enqueue</c> is called from inside an <c>OnEntry</c> callback the
    /// trigger must NOT execute synchronously inside the callback. The state
    /// observed at the moment of the call is still the state currently being
    /// entered; the queued trigger runs only after the current transition has
    /// finished.
    /// </summary>
    [Test]
    public void Enqueue_FromInsideOnEntry_DefersUntilCurrentTransitionCompletes()
    {
        StateMachine machine = null!;
        Type? stateAtEnqueueTime = null;

        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<TriggerA>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s
                .OnEntry(() =>
                {
                    machine.Enqueue(new TriggerB(), null);
                    // The queued trigger must not have run yet — we are still
                    // inside the AdvancedUnitTestState entry callback.
                    stateAtEnqueueTime = machine.GetCurrentState()?.GetType();
                })
                .On<TriggerB>().GoTo<ExtraUnitTestState>())
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new TriggerA());

        Assert.Multiple(() =>
        {
            Assert.That(stateAtEnqueueTime, Is.EqualTo(typeof(AdvancedUnitTestState)),
                "Enqueue must not run the trigger synchronously inside the callback.");
            Assert.That(machine.GetCurrentState(), Is.TypeOf<ExtraUnitTestState>(),
                "The queued trigger must run once the current transition completes.");
        });
    }

    /// <summary>
    /// Multiple triggers enqueued from inside a single callback must be drained
    /// in FIFO order.
    /// </summary>
    [Test]
    public void Enqueue_MultipleFromInsideCallback_DrainedInFifoOrder()
    {
        StateMachine machine = null!;
        var order = new List<string>();

        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<TriggerA>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s =>
            {
                s.OnEntry(() =>
                {
                    order.Add("entry:Advanced");
                    machine.Enqueue(new TriggerB(), null);
                    machine.Enqueue(new TriggerC(), null);
                });
                s.On<TriggerB>().Do(_ => order.Add("handler:B")).Ignore();
                s.On<TriggerC>().Do(_ => order.Add("handler:C")).Ignore();
            })
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new TriggerA());

        Assert.That(order, Is.EqualTo(new[] { "entry:Advanced", "handler:B", "handler:C" }));
    }

    /// <summary>
    /// A trigger drained from the queue may itself enqueue further triggers,
    /// and those must also be drained before the original entry point returns.
    /// </summary>
    [Test]
    public void Enqueue_ChainedFromDrainedTrigger_StillDrained()
    {
        StateMachine machine = null!;

        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<TriggerA>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s
                .OnEntry(() => machine.Enqueue(new TriggerB(), null))
                .On<TriggerB>().GoTo<ExtraUnitTestState>())
            .AddState<ExtraUnitTestState>(s => s
                .OnEntry(() => machine.Enqueue(new TriggerC(), null))
                .On<TriggerC>().GoTo<InitialUnitTestState>())
            .AddState<InitialUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new TriggerA());

        Assert.That(machine.GetCurrentState(), Is.TypeOf<InitialUnitTestState>(),
            "Triggers enqueued by drained triggers must also be drained before Fire returns.");
    }

    /// <summary>
    /// <c>Enqueue</c> from inside an <c>OnExit</c> callback is also deferred
    /// and processed after the surrounding transition completes.
    /// </summary>
    [Test]
    public void Enqueue_FromInsideOnExit_DefersUntilCurrentTransitionCompletes()
    {
        StateMachine machine = null!;

        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s =>
            {
                s.OnExit(() => machine.Enqueue(new TriggerB(), null));
                s.On<TriggerA>().GoTo<AdvancedUnitTestState>();
            })
            .AddState<AdvancedUnitTestState>(s => s.On<TriggerB>().GoTo<ExtraUnitTestState>())
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new TriggerA());

        Assert.That(machine.GetCurrentState(), Is.TypeOf<ExtraUnitTestState>());
    }

    /// <summary>
    /// <c>Enqueue</c> from inside a global transition callback also defers
    /// the trigger; the queued trigger must run after the current transition.
    /// </summary>
    [Test]
    public void Enqueue_FromInsideGlobalTransitionCallback_DefersUntilCurrentTransitionCompletes()
    {
        StateMachine machine = null!;

        machine = StateMachineBuilder.New()
            .AddOnStateChangedCallback(info =>
            {
                // Only react to the AdvancedUnitTestState entry — otherwise
                // the initial StartIn transition would consume our one-shot
                // enqueue and we'd recurse forever via the ExtraUnitTestState
                // entry too.
                if (info.ToType == typeof(AdvancedUnitTestState))
                {
                    machine.Enqueue(new TriggerB(), null);
                }
            })
            .AddState<SimpleUnitTestState>(s => s.On<TriggerA>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s.On<TriggerB>().GoTo<ExtraUnitTestState>())
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new TriggerA());

        Assert.That(machine.GetCurrentState(), Is.TypeOf<ExtraUnitTestState>());
    }

    /// <summary>
    /// <c>EnqueueAsync</c> called from inside a callback queues the trigger
    /// and the queued trigger ultimately runs after the surrounding transition
    /// completes — same scheduling guarantee as the synchronous <c>Enqueue</c>.
    /// </summary>
    [Test]
    public async Task EnqueueAsync_FromInsideOnEntry_DefersUntilCurrentTransitionCompletes()
    {
        StateMachine machine = null!;

        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<TriggerA>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s
                .OnEntry(() =>
                {
                    // Fire-and-forget on purpose: in-transition EnqueueAsync
                    // returns a completed task immediately and the queued
                    // trigger is drained by the surrounding FireAsync.
                    _ = machine.EnqueueAsync(new TriggerB(), null);
                })
                .On<TriggerB>().GoTo<ExtraUnitTestState>())
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        await machine.FireAsync(new TriggerA());

        Assert.That(machine.GetCurrentState(), Is.TypeOf<ExtraUnitTestState>());
    }
}
