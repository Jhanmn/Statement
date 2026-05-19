using Statement.Fluent.Api;
using Statement.Tests.TestStates;

namespace Statement.Tests;

file sealed class GoTrigger;

[TestFixture]
public class TransitionPayloadTests
{
    private sealed record FileData(string Path);

    [Test]
    public void OnEntryWith_ReceivesPayload_FromSetCurrentState()
    {
        FileData? captured = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>(s => s.OnEntryWith<FileData>((_, p) => captured = p))
            .StartIn<SimpleUnitTestState>()
            .Build();

        var payload = new FileData("foo.txt");
        machine.SetCurrentState<AdvancedUnitTestState>(payload);

        Assert.That(captured, Is.SameAs(payload));
    }

    [Test]
    public void OnEntryWith_ReceivesPayload_FromFire()
    {
        FileData? captured = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.On<GoTrigger>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s.OnEntryWith<FileData>(p => captured = p))
            .StartIn<SimpleUnitTestState>()
            .Build();

        var payload = new FileData("bar.txt");
        machine.Fire(new GoTrigger(), payload);

        Assert.That(captured, Is.SameAs(payload));
    }

    [Test]
    public void OnEntryWith_WrongPayloadType_SkipsCallback_StillTransitions()
    {
        var invoked = false;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>(s => s.OnEntryWith<FileData>((_, _) => invoked = true))
            .StartIn<SimpleUnitTestState>()
            .Build();

        Assert.DoesNotThrow(() => machine.SetCurrentState<AdvancedUnitTestState>("not a FileData"));
        Assert.That(invoked, Is.False);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void OnEntryWith_NullPayload_SkipsCallback_StillTransitions()
    {
        var invoked = false;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>(s => s.OnEntryWith<FileData>((_, _) => invoked = true))
            .StartIn<SimpleUnitTestState>()
            .Build();

        Assert.DoesNotThrow(() => machine.SetCurrentState<AdvancedUnitTestState>());
        Assert.That(invoked, Is.False);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void OnExitWith_ReceivesPayload_FromSetCurrentState()
    {
        FileData? captured = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.OnExitWith<FileData>((_, p) => captured = p))
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        var payload = new FileData("foo.txt");
        machine.SetCurrentState<AdvancedUnitTestState>(payload);

        Assert.That(captured, Is.SameAs(payload));
    }

    [Test]
    public void OnExitWith_ReceivesPayload_FromFire()
    {
        FileData? captured = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .OnExitWith<FileData>(p => captured = p)
                .On<GoTrigger>().GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        var payload = new FileData("bar.txt");
        machine.Fire(new GoTrigger(), payload);

        Assert.That(captured, Is.SameAs(payload));
    }

    [Test]
    public void OnExitWith_WrongPayloadType_SkipsCallback_StillTransitions()
    {
        var invoked = false;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.OnExitWith<FileData>((_, _) => invoked = true))
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        Assert.DoesNotThrow(() => machine.SetCurrentState<AdvancedUnitTestState>("not a FileData"));
        Assert.That(invoked, Is.False);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void OnExitWith_NullPayload_SkipsCallback_StillTransitions()
    {
        var invoked = false;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.OnExitWith<FileData>((_, _) => invoked = true))
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        Assert.DoesNotThrow(() => machine.SetCurrentState<AdvancedUnitTestState>());
        Assert.That(invoked, Is.False);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void OnExitWith_FiresOnlyWhenIfGuardPasses()
    {
        FileData? exited = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .OnExitWith<FileData>(p => exited = p)
                .On<GoTrigger>()
                .If<FileData>(p => p.Path.EndsWith(".txt"))
                .GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new GoTrigger(), new FileData("nope.bin"));
        Assert.That(exited, Is.Null);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());

        var ok = new FileData("ok.txt");
        machine.Fire(new GoTrigger(), ok);
        Assert.That(exited, Is.SameAs(ok));
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void OnExitWith_NotInvoked_WhenIfGuardFails()
    {
        var invoked = false;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .OnExitWith<FileData>((_, _) => invoked = true)
                .On<GoTrigger>()
                .If<FileData>(p => p.Path.EndsWith(".txt"))
                .GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new GoTrigger(), new FileData("skip.bin"));

        Assert.That(invoked, Is.False);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void OnExitWith_Invoked_WhenIfGuardPasses()
    {
        FileData? exited = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .OnExitWith<FileData>((_, p) => exited = p)
                .On<GoTrigger>()
                .If<FileData>(p => p.Path.EndsWith(".txt"))
                .GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        var payload = new FileData("ok.txt");
        machine.Fire(new GoTrigger(), payload);

        Assert.That(exited, Is.SameAs(payload));
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void OnExitWith_NotInvoked_WhenIfGuardSeesWrongPayloadType()
    {
        var invoked = false;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .OnExitWith<FileData>((_, _) => invoked = true)
                .On<GoTrigger>()
                .If<FileData>(_ => true)
                .GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new GoTrigger(), "not a FileData");

        Assert.That(invoked, Is.False);
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void OnExitWith_AndOnEntryWith_BothReceivePayload_WhenIfGuardPasses()
    {
        FileData? exited = null;
        FileData? entered = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .OnExitWith<FileData>(p => exited = p)
                .On<GoTrigger>()
                .If<FileData>(p => p.Path.Length > 0)
                .GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s.OnEntryWith<FileData>(p => entered = p))
            .StartIn<SimpleUnitTestState>()
            .Build();

        var payload = new FileData("file.txt");
        machine.Fire(new GoTrigger(), payload);

        Assert.That(exited, Is.SameAs(payload));
        Assert.That(entered, Is.SameAs(payload));
    }

    [Test]
    public void OnExitWith_RunsAfterGuardPasses_OnSecondAttempt()
    {
        var exitCount = 0;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .OnExitWith<FileData>((_, _) => exitCount++)
                .On<GoTrigger>()
                .If<FileData>(p => p.Path.EndsWith(".txt"))
                .GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new GoTrigger(), new FileData("first.bin"));
        Assert.That(exitCount, Is.Zero);

        machine.Fire(new GoTrigger(), new FileData("second.txt"));
        Assert.That(exitCount, Is.EqualTo(1));
    }

    [Test]
    public void OnExitWith_RunsBeforeOnEntryWith()
    {
        var order = new List<string>();
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s.OnExitWith<FileData>(p => order.Add($"exit:{p.Path}")))
            .AddState<AdvancedUnitTestState>(s => s.OnEntryWith<FileData>(p => order.Add($"entry:{p.Path}")))
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.SetCurrentState<AdvancedUnitTestState>(new FileData("x.txt"));

        Assert.That(order, Is.EqualTo(new[] { "exit:x.txt", "entry:x.txt" }));
    }

    [Test]
    public void OnEntry_WithoutPayloadCallback_IgnoresPayload()
    {
        var entered = false;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>(s => s.OnEntry((_, _) => entered = true))
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.SetCurrentState<AdvancedUnitTestState>(new FileData("ignored"));

        Assert.That(entered, Is.True);
    }

    [Test]
    public void TriggerIfWithPayload_PassingPredicate_AllowsTransition()
    {
        FileData? captured = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .On<GoTrigger>()
                .If<FileData>(p => p.Path.EndsWith(".txt"))
                .GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>(s => s.OnEntryWith<FileData>(p => captured = p))
            .StartIn<SimpleUnitTestState>()
            .Build();

        var payload = new FileData("ok.txt");
        machine.Fire(new GoTrigger(), payload);

        Assert.That(captured, Is.SameAs(payload));
        Assert.That(machine.GetCurrentState(), Is.TypeOf<AdvancedUnitTestState>());
    }

    [Test]
    public void TriggerIfWithPayload_FailingPredicate_DoesNotTransition()
    {
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .On<GoTrigger>()
                .If<FileData>(p => p.Path.EndsWith(".txt"))
                .GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new GoTrigger(), new FileData("nope.bin"));

        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void TriggerIfWithPayload_WrongPayloadType_TreatedAsGuardFailure()
    {
        Statement.Failures.TriggerFailureInfo? captured = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>(s => s
                .On<GoTrigger>()
                .If<FileData>(_ => true)
                .GoTo<AdvancedUnitTestState>())
            .AddState<AdvancedUnitTestState>()
            .OnTriggerFailure(Statement.Failures.TriggerFailurePolicy.Invoke(info => captured = info))
            .StartIn<SimpleUnitTestState>()
            .Build();

        machine.Fire(new GoTrigger(), "not a FileData");

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Reason, Is.EqualTo(Statement.Failures.TriggerFailureReason.GuardFailed));
        Assert.That(machine.GetCurrentState(), Is.TypeOf<SimpleUnitTestState>());
    }

    [Test]
    public void TransitionInformation_CarriesPayload_ToGlobalCallback()
    {
        TransitionInformation? captured = null;
        var machine = StateMachineBuilder.New()
            .AddState<SimpleUnitTestState>()
            .AddState<AdvancedUnitTestState>()
            .StartIn<SimpleUnitTestState>()
            .AddOnStateChangedCallback(info => captured = info)
            .Build();

        var payload = new FileData("baz.txt");
        machine.SetCurrentState<AdvancedUnitTestState>(payload);

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Value.Payload, Is.SameAs(payload));
    }
}
