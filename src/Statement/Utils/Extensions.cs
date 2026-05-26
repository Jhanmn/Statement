using System;
using System.Threading;
using System.Threading.Tasks;
using Statement.Failures;

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

    extension(StateMachineState state)
    {
        /// <summary>
        /// Reports a <see cref="TransitionFailureReason.MachinePaused"/> failure and signals the caller to abort
        /// when the machine is paused. Returns <c>true</c> when execution should continue, <c>false</c> when paused.
        /// </summary>
        public bool ReportIfPausedAndBlock(TransitionFailurePolicy policy, Type? current, Type target)
        {
            if (state != StateMachineState.Paused) return true;
            policy.Handle(new TransitionFailureInfo(current, target, TransitionFailureReason.MachinePaused));
            return false;
        }

        /// <summary>
        /// Reports a <see cref="TriggerFailureReason.MachinePaused"/> failure and signals the caller to abort
        /// when the machine is paused. Returns <c>true</c> when execution should continue, <c>false</c> when paused.
        /// </summary>
        public bool ReportIfPausedAndBlock(TriggerFailurePolicy policy, Type? current, object trigger)
        {
            if (state != StateMachineState.Paused) return true;
            policy.Handle(new TriggerFailureInfo(current, trigger, TriggerFailureReason.MachinePaused));
            return false;
        }
    }
}