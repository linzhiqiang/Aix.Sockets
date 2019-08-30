using Aix.SocketCore.Channels;
using Aix.SocketCore.Config;
using Aix.SocketCore.EventLoop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Bootstrapping
{
    public class AbstractBootstrap<TBootstrap> where TBootstrap : AbstractBootstrap<TBootstrap>
    {
        protected MultithreadEventLoopGroup _workerGroup;

        protected Action<IChannel> _workerHandler;

        protected Func<IChannel> _channelFactory;

        public TBootstrap Config(string key, object value)
        {
            ConfigContainer.Instance.SetConfig(key,value);
            return (TBootstrap)this;
        }
        public TBootstrap Group(MultithreadEventLoopGroup workerGroup)
        {
            _workerGroup = workerGroup;
            return (TBootstrap)this;
        }
        public TBootstrap WorkerHandler(Action<IChannel> workerHandler)
        {
            _workerHandler = workerHandler;
            return (TBootstrap)this;
        }

        public TBootstrap Channel<T>() where T : IChannel, new()
        {
            _channelFactory = () => new T();
            return (TBootstrap)this;
        }

    }
}
