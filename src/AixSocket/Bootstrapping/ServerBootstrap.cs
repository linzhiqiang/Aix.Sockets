using AixSocket.Channels;
using AixSocket.EventLoop;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AixSocket.Bootstrapping
{
    public class ServerBootstrap: AbstractBootstrap<ServerBootstrap>
    {
        MultithreadEventLoopGroup _bossGroup;

        Action<IChannel> _bossHandler;
      
        public ServerBootstrap Group(MultithreadEventLoopGroup bossGroup, MultithreadEventLoopGroup workerGroup)
        {
            _bossGroup = bossGroup;
            base.Group(workerGroup);
            return this;
        }

        public ServerBootstrap BossHandler(Action<IChannel> bossHandler)
        {
            _bossHandler = bossHandler;
            return this;
        }
        public async Task<IChannel> BindAsync(EndPoint localAddress)
        {
            var channel = this._channelFactory();

            //初始化handler
            _bossHandler(channel);
            channel.Pipeline.AddLast("ServerBootstrapAcceptor", new ServerBootstrapAcceptor(_workerGroup, _workerHandler));


            //注册事件循环
            await ((IChannelUnsafe)channel).UnsafeRegisterAsync(_bossGroup.GetNext());

            await channel.BindAsync(localAddress);
            return channel;
        }

    }
}
