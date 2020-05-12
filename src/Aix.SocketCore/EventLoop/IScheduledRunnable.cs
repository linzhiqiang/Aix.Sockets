﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.EventLoop
{
    public interface IScheduledRunnable : IRunnable, IComparable<IScheduledRunnable>
    {
        long TimeStamp { get; }
    }

    public class ScheduledRunnable : IScheduledRunnable
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

        public void Run()
        {
            _action.Run();
        }

    }
}
