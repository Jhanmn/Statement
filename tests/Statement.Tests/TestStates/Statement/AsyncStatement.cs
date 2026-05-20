using Statement.State;

namespace Statement.Tests.TestStates.Statement;

public class AsyncStatement : IAsyncStatement
{
    public bool OnEntryAsyncCalled { get; set; }
    public bool OnExitAsyncCalled { get; set; }

    public Task OnEntryAsync(CancellationToken cancellationToken = default)
    {
        OnEntryAsyncCalled = true;
        return Task.CompletedTask;
    }

    public Task OnExitAsync(CancellationToken cts = default)
    {
        OnExitAsyncCalled = true;
        return Task.CompletedTask;
    }
}
