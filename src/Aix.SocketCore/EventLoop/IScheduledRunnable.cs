using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.EventLoop
{
    public interface IScheduledRunnable : IRunnable, IComparable<IScheduledRunnable>
    {
        long TimeStamp { get; }
    }

    
}
