using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.SocketCore
{
    internal static class TaskExtensions
    {
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationToken token = default(CancellationToken))
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                using (var tokenWrap = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationTokenSource.Token, token))
                {
                    var completedTask = await Task.WhenAny(task, Task.Delay(timeout, tokenWrap.Token));
                    if (completedTask == task)
                    {
                        timeoutCancellationTokenSource.Cancel(); //取消Task.Delay
                        return await task;  // Very important in order to propagate exceptions
                    }
                    else
                    {
                        throw new TimeoutException("The operation has timed out.");
                    }
                }
            }
        }


        public static async Task TimeoutAfter(this Task task, TimeSpan timeout, CancellationToken token = default(CancellationToken))
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                using (var tokenWrap = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationTokenSource.Token, token))
                {
                    var completedTask = await Task.WhenAny(task, Task.Delay(timeout, tokenWrap.Token));
                    if (completedTask == task)
                    {
                        timeoutCancellationTokenSource.Cancel(); //取消Task.Delay
                        await task;  // Very important in order to propagate exceptions
                    }
                    else
                    {
                        throw new TimeoutException("The operation has timed out.");
                    }
                }
            }
        }


    }
}
