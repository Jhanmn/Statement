using System;
using System.Threading;
using System.Threading.Tasks;

namespace Statement.Utils;

public static class Extensions
{
    extension(SemaphoreSlim semaphore)
    {
        public void RunAction(Action action, CancellationToken cancellationToken = default)
        {
            semaphore.Wait(cancellationToken);
            try
            {
                action.Invoke();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task RunActionAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken);
        
            try
            {
                await action.Invoke();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    extension(AsyncLocal<bool> isInTransition)
    {
        public void ThrowIfActive()
        {
            if (isInTransition.Value)
            {
                throw new InvalidOperationException(
                    "Cannot call fire (and all its (async) overloads) from within a transition callback. " +
                    "-> potential deadlock. Use Enqueue instead");
            }
        }
    }
}