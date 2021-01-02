using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.EventLoop
{
    public interface IScheduledRunnable : IRunnable, IComparable<IScheduledRunnable>
    {
        long TimeStamp { get; }

        /// <summary>
        /// 该任务是否执行过
        /// </summary>
        bool Executed { get; }

        bool Cancel();
    }

    public class ScheduledRunnable : IScheduledRunnable
    {
        private IEventExecutor _executor;
        public long TimeStamp { get; }

        public bool Executed { get; private set; }

        IRunnable _action;

        public ScheduledRunnable(IEventExecutor executor, IRunnable runnable, long timeStamp)
        {
            _executor = executor;
            _action = runnable;
            TimeStamp = timeStamp;
            Executed = false;
        }

        public int CompareTo(IScheduledRunnable other)
        {
            return (int)(this.TimeStamp - other.TimeStamp);
        }

        public void Run()
        {
            Executed = true;
            _action.Run();
        }

        public bool Cancel()
        {
            _executor.RemoveScheduled(this);
            return !Executed;
        }
    }
}
