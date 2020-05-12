using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.EventLoop
{
    public interface IEventExecutor : IDisposable
    {
        bool InEventLoop { get; }
        void Execute(IRunnable task);

        void Execute(Action action);


        event Func<Exception, Task> OnException;

        /// <summary>
        /// 延时执行任务
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        void Schedule(IRunnable action, TimeSpan delay);

        void Schedule(Action action, TimeSpan delay);


        void Start();

        void Stop();

    }

   


    
}
