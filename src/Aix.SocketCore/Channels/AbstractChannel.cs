using Aix.SocketCore.EventLoop;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.Channels
{
    public abstract class AbstractChannel : IChannel
    {
        public IChannel Parent { get; }

        public abstract bool Open { get; protected set; }
        public IChannelPipeline Pipeline { get; }

        public IEventExecutor EventExecutor { get; protected set; }

        public EndPoint LocalAddress
        {
            get { return GetLocalAddress(); }
        }

        public EndPoint RemoteAddress
        {
            get { return GetRemoteAddress(); }
        }

        public AbstractChannel(IChannel parent)
        {
            this.Pipeline = new DefaultChannelPipeline(this);
            this.Parent = parent;
        }

        #region  出站
        public Task DeregisterAsync()
        {
            return this.Pipeline.DeregisterAsync();
        }
        public Task BindAsync(EndPoint localAddress)
        {
            return this.Pipeline.BindAsync(localAddress);
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            return this.Pipeline.ConnectAsync(remoteAddress);
        }

        public Task DisconnectAsync()
        {
            return this.Pipeline.DisconnectAsync();
        }

        public Task CloseAsync()
        {
            return this.Pipeline.CloseAsync();
        }

        public Task WriteAsync(object message)
        {
            return this.Pipeline.WriteAsync(message);
        }

        public IChannel Read()
        {
            this.Pipeline.Read();
            return this;
        }
        #endregion

        protected abstract EndPoint GetLocalAddress();

        protected abstract EndPoint GetRemoteAddress();
    }
}
