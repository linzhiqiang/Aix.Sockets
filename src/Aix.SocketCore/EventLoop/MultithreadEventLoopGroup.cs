using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.SocketCore.EventLoop
{
    public class MultithreadEventLoopGroup //: IEventExecutor
    {
        static readonly int DefaultEventLoopThreadCount = Environment.ProcessorCount * 2;
        readonly SingleThreadEventExecutor[] EventLoops;
        int requestId;

        public MultithreadEventLoopGroup() : this(DefaultEventLoopThreadCount)
        {

        }
        public MultithreadEventLoopGroup(int threadCount)
        {
            this.EventLoops = new SingleThreadEventExecutor[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var eventLoop = new SingleThreadEventExecutor();
                this.EventLoops[i] = eventLoop;
                eventLoop.OnException += EventLoop_OnException;
            }
        }

        public IEventExecutor GetNext()
        {
            int id = Interlocked.Increment(ref this.requestId);
            return this.EventLoops[Math.Abs(id % this.EventLoops.Length)];
        }

        private async Task EventLoop_OnException(Exception ex)
        {
            if (OnException != null) await OnException(ex);
        }

        public event Func<Exception, Task> OnException;

        public void Start()
        {
            foreach (var item in this.EventLoops)
            {
                item.Start();
            }
        }

        public void Stop()
        {
            foreach (var item in this.EventLoops)
            {
                item.Stop();
            }
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
