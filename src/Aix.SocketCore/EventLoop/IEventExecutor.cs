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

    public class ActionRunnable : IRunnable
    {
        Action _action;
        public ActionRunnable(Action action)
        {
            _action = action;

        }
        public void Run()
        {
            _action();
        }
    }


    public  class ScheduledRunnable : IScheduledRunnable
    {
        public long TimeStamp { get; }
        IRunnable _action;

        public ScheduledRunnable(IRunnable runnable, long timeStamp)
        {
            _action = runnable;
            TimeStamp = timeStamp;
        }

        public int CompareTo(IScheduledRunnable other)
        {
            return (int)(this.TimeStamp - other.TimeStamp);
        }

        public void  Run()
        {
             _action.Run();
        }

    }
}
