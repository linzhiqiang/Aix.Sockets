using Aix.SocketCore;
using Aix.SocketCore.Bootstrapping;
using Aix.SocketCore.Channels;
using Aix.SocketCore.Channels.Sockets;
using Aix.SocketCore.Codecs;
using Aix.SocketCore.Config;
using Aix.SocketCore.DefaultHandlers;
using Aix.SocketCore.EventLoop;
using AixSocket.Logging;
using AixSocketDemo.Common.Codecs;
using AixSocketDemo.Server.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AixSocketDemo.Server
{
    public class StartHostService : IHostedService
    {
        private ILoggerFactory _loggerFactory;

        public StartHostService(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await Test();
            });
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await CloseAsync();
        }

        MultithreadEventLoopGroup BossGroup = null;
        MultithreadEventLoopGroup WorkerGroup = null;
        IChannel ServerChannel = null;
        public async Task Test()
        {
            InternalLoggerFactory.DefaultFactory = _loggerFactory;//.AddConsoleLogger();

            BossGroup = new MultithreadEventLoopGroup(1);
            WorkerGroup = new MultithreadEventLoopGroup();
            BossGroup.Start();
            WorkerGroup.Start();

            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(BossGroup, WorkerGroup)
                .Config(ConfigConstant.Backlog, 10240)
                .Config(ConfigConstant.HeartbeatIntervalSecond, 60)
                .Channel<TcpServerSocketChannel>()
                .BossHandler(channel =>
                {
                    channel.Pipeline.AddLast("BossHandler", new BossHandler());
                })
                .WorkerHandler(channel =>
                {
                    // channel.Pipeline.AddLast("MessageDecoder", new TestDecoder());
                    channel.Pipeline.AddLast("MessageDecoder1", new LengthFieldBasedFrameDecoder(int.MaxValue, 4, 4));
                    channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder());


                    channel.Pipeline.AddLast("MessageDecoder", new MessageEncoder());
                    channel.Pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, 60 + 5));
                    channel.Pipeline.AddLast("ServerHeartbeatHandler", new ServerHeartbeatHandler());
                    channel.Pipeline.AddLast("ServerHandler", new ServerHandler());

                });
            var ip = "127.0.0.1";
            //ip = "192.168.111.133";
            int port = 8007;
            ServerChannel = await bootstrap.BindAsync(new IPEndPoint(IPAddress.Parse(ip), port));

            //IChannel serverChannel = new TcpServerSocketChannel();

            ////初始化handler
            //Action<IChannel> child = (channel) =>
            //{

            //    channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder());
            //    channel.Pipeline.AddLast("MessageDecoder", new MessageEncoder());
            //    channel.Pipeline.AddLast("ServerHandler", new ServerHandler());
            //};

            ////服务端监听socket的channelhandler 
            //serverChannel.Pipeline.AddLast("ServerBootstrapAcceptor", new ServerBootstrapAcceptor(workerGroup, child));
            ////注册事件循环
            //await ((IChannelUnsafe)serverChannel).UnsafeRegisterAsync(bossGroup.GetNext());

            //var ip = "127.0.0.1";
            //int port = 8007;
            //await serverChannel.BindAsync(new IPEndPoint(IPAddress.Parse(ip), port));

        }

        public async Task CloseAsync()
        {
            await ServerChannel.CloseAsync();
            BossGroup.Stop();
            WorkerGroup.Stop();
        }
    }
}
