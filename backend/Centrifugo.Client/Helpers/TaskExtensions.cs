using System;
using System.Threading;
using System.Threading.Tasks;

namespace Centrifugo.Client.Helpers
{
    public static class TaskExtensions
    {
        public static async Task<TResult> TimeoutAfterAsync<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
           
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();

                return await task;
            }

            throw new TimeoutException("The operation has timed out.");
        }

        public static async Task TimeoutAfterAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));

            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                await task;

                return;
            }

            throw new TimeoutException("The operation has timed out.");
        }
    }
}