using Aix.SocketCore;
using Aix.SocketCore.Bootstrapping;
using Aix.SocketCore.Channels;
using Aix.SocketCore.Channels.Sockets;
using Aix.SocketCore.Codecs;
using Aix.SocketCore.Config;
using Aix.SocketCore.DefaultHandlers;
using Aix.SocketCore.EventLoop;
using AixSocket.Logging;
using AixSocketDemo.Client.Handlers;
using AixSocketDemo.Common.Codecs;
using AixSocketDemo.Common.Invokes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AixSocketDemo.Client
{
    public class StartHostService : IHostedService
    {
        private ILoggerFactory _loggerFactory;

        private ILogger<StartHostService> _logger;

        public StartHostService(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger=_loggerFactory.CreateLogger<StartHostService>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await Test();
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task Test()
        {
            InternalLoggerFactory.DefaultFactory = _loggerFactory;//.AddConsoleLogger();

            //实际要考虑事件循环的共用，事件循环的关闭问题
            MultithreadEventLoopGroup workerGroup = new MultithreadEventLoopGroup(8);
            workerGroup.Start();

            var bootstrap = new ClientBootstrap();
            bootstrap
                .Group(workerGroup)
                .Config(ConfigConstant.HeartbeatIntervalSecond, 60)
                .Channel<TcpSocketChannel>()
                .WorkerHandler(channel =>
                {
                    //channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder());
                    channel.Pipeline.AddLast("MessageBaseDecoder", new LengthFieldBasedFrameDecoder(int.MaxValue, 4, 4));
                    channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder());

                    channel.Pipeline.AddLast("MessageEncoder", new MessageEncoder());
                    channel.Pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, 60));
                    channel.Pipeline.AddLast("ClientHeartbeatHandler", new ClientHeartbeatHandler());
                    channel.Pipeline.AddLast("ServerHandler", new ClientHandler());

                    //便于理解 这里都是addlist 入站是从上外下执行的，出站是从下往上执行的
                });
            var ip = "127.0.0.1";
            //ip="192.168.111.133";
            int port = 8007;

            for (int i = 0; i < 4; i++)
            {
                Task.Run(async () =>
                {
                    var client = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
                    await Test(3, client);
                });

                // await Task.Delay(10);
            }

        }
        static int Count = 0;
        private async Task Test(int count, IChannel client)
        {
            for (int i = 0; i < count; i++)
            {
                Message message = new Message() {
                    MessageType = MessageType.Request,
                    RequestId = RequestIdGenerator.Instance.GetNewRequestId()
                };
                if (i == 0) message.MessageType = MessageType.Auth;
                message.Data = Encoding.UTF8.GetBytes(i + GetLargeMsg(1000));

                var tcs = ResponseManage.Instance.RegisterRequest(message.RequestId, 5000);
                await client.WriteAsync(message);

                Message messageRes = null;
                try
                {
                    messageRes = await tcs.Task;
                    var str = Encoding.UTF8.GetString(messageRes.Data);
                    var countIndex = Interlocked.Increment(ref Count);
                    _logger.LogInformation("接收数据：" + (countIndex)+ str);
                }
                catch (TimeoutException ex)
                {
                    //记录log
                    _logger.LogError($"{message.RequestId}请求超时，{ex.Message}");
                    throw ex;
                }
            }
            GC.Collect();
        }

        private string GetLargeMsg(int length)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("$");
            for (int i = 0; i < length - 2; i++)
            {
                sb.Append(i % 2);
            }
            sb.Append("$");

            return sb.ToString();
        }
    }
}
