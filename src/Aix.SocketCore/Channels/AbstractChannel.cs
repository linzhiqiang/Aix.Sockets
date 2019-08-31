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

        volatile EndPoint localAddress;
       
        public EndPoint LocalAddress
        {
            get
            {
                EndPoint address = this.localAddress;
                return address ?? this.CacheLocalAddress();
            }
        }
        protected abstract EndPoint LocalAddressInternal { get; }


        volatile EndPoint remoteAddress;
        public EndPoint RemoteAddress
        {
            get
            {
                EndPoint address = this.remoteAddress;
                return address ?? this.CacheRemoteAddress();
            }
        }
        protected abstract EndPoint RemoteAddressInternal { get; }



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

        protected EndPoint CacheLocalAddress()
        {
            try
            {
                return this.localAddress = this.LocalAddressInternal;
            }
            catch (Exception)
            {
                // Sometimes fails on a closed socket in Windows.
                return null;
            }
        }

        protected EndPoint CacheRemoteAddress()
        {
            try
            {
                return this.remoteAddress = this.RemoteAddressInternal;
            }
            catch (Exception)
            {
                // Sometimes fails on a closed socket in Windows.
                return null;
            }
        }
     
    }
}
