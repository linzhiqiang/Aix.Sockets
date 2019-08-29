﻿using AixSocket;
using AixSocket.Bootstrapping;
using AixSocket.Channels;
using AixSocket.Channels.Sockets;
using AixSocket.Config;
using AixSocket.DefaultHandlers;
using AixSocket.EventLoop;
using AixSocket.Logging;
using AixSocketDemo.Common.Codecs;
using AixSocketDemo.Server.Handlers;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AixSocketDemo.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Test().Wait();

            Console.Read();
        }

        public static async Task Test()
        {
            InternalLoggerFactory.DefaultFactory.AddConsoleLogger();
            MultithreadEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
            MultithreadEventLoopGroup workerGroup = new MultithreadEventLoopGroup();
            bossGroup.Start();
            workerGroup.Start();

            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup, workerGroup)
                .Config(ConfigConstant.Backlog,10240)
                .Channel<TcpServerSocketChannel>()
                .BossHandler(channel =>
                {
                    channel.Pipeline.AddLast("BossHandler", new BossHandler());
                })
                .WorkerHandler(channel =>
                {
                    channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder());
                    channel.Pipeline.AddLast("MessageDecoder", new MessageEncoder());
                    channel.Pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, 60+10));
                    channel.Pipeline.AddLast("ServerHeartbeatHandler", new ServerHeartbeatHandler());
                    channel.Pipeline.AddLast("ServerHandler", new ServerHandler());
                    
                });
            var ip = "127.0.0.1";
            //ip = "192.168.111.133";
            int port = 8007;
            await bootstrap.BindAsync(new IPEndPoint(IPAddress.Parse(ip), port));

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
    }
}