using Statement.Failures;
using Statement.Fluent.Api;
using Statement.Tests.TestStates;

namespace Statement.Tests;

file sealed class GoTrigger;
file sealed class NextTrigger;
file sealed class ThrowingTrigger;
file sealed class FollowupTrigger;

[TestFixture]
public class FireDeadlockTests
{
    /// <summary>
    /// Calling <c>Fire</c> from inside an OnEntry callback now throws
    /// <see cref="InvalidOperationException"/> with a clear diagnostic message
    /// directing the caller to use <see cref="StateMachine.Enqueue"/> instead.
    /// This prevents silent deadlock on the non-reentrant semaphore.
    /// </summary>
    [Test]
    public void Fire_FromInsideOnEntryCallback_ThrowsInvalidOperationException()
    {
        StateMachine machine = null!;
        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<GoTrigger>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s
                .OnEntry(() =>
                {
                    // Attempting to Fire from within a callback should throw,
                    // not deadlock. Users should use Enqueue instead.
                    machine.Fire(new NextTrigger());
                })
                .On<NextTrigger>().GoTo<ExtraUnitTestState>())
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() => machine.Fire(new GoTrigger()));
        Assert.That(exception.Message, Does.Contain("Enqueue"));
    }

    /// <summary>
    /// Calling <c>SetCurrentState</c> from inside an OnEntry callback also throws
    /// the same re-entry diagnostic, instead of deadlocking on the semaphore.
    /// </summary>
    [Test]
    public void SetCurrentState_FromInsideOnEntryCallback_ThrowsInvalidOperationException()
    {
        StateMachine machine = null!;
        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<GoTrigger>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s
                .OnEntry(() => machine.SetCurrentState<ExtraUnitTestState>()))
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() => machine.Fire(new GoTrigger()));
        Assert.That(exception.Message, Does.Contain("Enqueue"));
    }

    /// <summary>
    /// Enqueueing a trigger from inside a callback fired via <c>SetCurrentState</c>
    /// works the same as from a <c>Fire</c>-driven callback — the queued trigger
    /// runs after the current transition completes.
    /// </summary>
    [Test]
    public void Enqueue_FromInsideSetCurrentStateCallback_ChainsTransition()
    {
        StateMachine machine = null!;
        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>(s => s
                .OnEntry(() => machine.Enqueue(new NextTrigger(), null))
                .On<NextTrigger>().GoTo<ExtraUnitTestState>())
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.SetCurrentState<AdvancedUnitTestState>();

        Assert.That(machine.GetCurrentState(), Is.TypeOf<ExtraUnitTestState>());
    }

    /// <summary>
    /// Using <c>Enqueue</c> from inside an OnEntry callback allows safe chaining
    /// of transitions without re-entrance or deadlock. The enqueued trigger fires
    /// after the current transition completes.
    /// </summary>
    [Test]
    public void Enqueue_FromInsideOnEntryCallback_ChainsTransition()
    {
        StateMachine machine = null!;
        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<GoTrigger>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s
                .OnEntry(() =>
                {
                    // Enqueueing a trigger from within a callback is safe.
                    // It will fire after this transition completes.
                    machine.Enqueue(new NextTrigger(), null);
                })
                .On<NextTrigger>().GoTo<ExtraUnitTestState>())
            .AddState<ExtraUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new GoTrigger());

        // Both transitions should have completed: GoTrigger → AdvancedUnitTestState,
        // then the enqueued NextTrigger → ExtraUnitTestState.
        Assert.That(machine.GetCurrentState(), Is.TypeOf<ExtraUnitTestState>());
    }

    /// <summary>
    /// When a queued trigger's handler throws during drain, the failure must be
    /// surfaced through the configured <see cref="TriggerFailurePolicy"/> with
    /// <see cref="TriggerFailureReason.HandlerThrew"/>, not silently swallowed.
    /// </summary>
    [Test]
    public void DrainQueue_HandlerThrows_ReportsHandlerThrewViaPolicy()
    {
        TriggerFailureInfo? captured = null;
        StateMachine machine = null!;
        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<GoTrigger>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s =>
            {
                // OnEntry enqueues a follow-up trigger whose Do handler throws.
                s.OnEntry(() => machine.Enqueue(new ThrowingTrigger(), null));
                s.On<ThrowingTrigger>().Do(t => throw new InvalidOperationException("boom")).Ignore();
            })
            .OnTriggerFailure(TriggerFailurePolicy.Invoke(info => captured = info))
            .StartIn<SimpleUnitTestState>()
            .Build();

        // Outer Fire must not throw — the failure is reported through the policy.
        Assert.DoesNotThrow(() => machine.Fire(new GoTrigger()));

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Reason, Is.EqualTo(TriggerFailureReason.HandlerThrew));
        Assert.That(captured.Trigger, Is.TypeOf<ThrowingTrigger>());
    }

    /// <summary>
    /// After a queued trigger's handler throws, the drain must continue processing
    /// the remaining queued triggers rather than aborting the whole queue.
    /// </summary>
    [Test]
    public void DrainQueue_HandlerThrows_ContinuesDrainingRemainingTriggers()
    {
        StateMachine machine = null!;
        machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<GoTrigger>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s =>
            {
                // Enqueue two triggers: the first throw's, the second should still run.
                s.OnEntry(() =>
                {
                    machine.Enqueue(new ThrowingTrigger(), null);
                    machine.Enqueue(new FollowupTrigger(), null);
                });
                s.On<ThrowingTrigger>().Do(t => throw new InvalidOperationException("boom")).Ignore();
                s.On<FollowupTrigger>().GoTo<ExtraUnitTestState>();
            })
            .AddState<ExtraUnitTestState>()
            .OnTriggerFailure(TriggerFailurePolicy.Silent)
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new GoTrigger());

        // FollowupTrigger ran after ThrowingTrigger failed → final state is ExtraUnitTestState.
        Assert.That(machine.GetCurrentState(), Is.TypeOf<ExtraUnitTestState>());
    }
}
