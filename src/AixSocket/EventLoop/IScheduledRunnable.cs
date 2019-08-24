using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AixSocket.EventLoop
{
    public interface IScheduledRunnable : IRunnable, IComparable<IScheduledRunnable>
    {
        long TimeStamp { get; }
    }

    
}
