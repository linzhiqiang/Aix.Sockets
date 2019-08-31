using Aix.SocketCore;
using Aix.SocketCore.Bootstrapping;
using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using Aix.SocketCore.Channels.Sockets;
using Aix.SocketCore.Codecs;
using Aix.SocketCore.Config;
using Aix.SocketCore.DefaultHandlers;
using Aix.SocketCore.EventLoop;
using Aix.SocketCore.Utils;
using AixSocket.Logging;
using AixSocketDemo.Client.Handlers;
using AixSocketDemo.Common.Codecs;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AixSocketDemo.Client
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
                    channel.Pipeline.AddLast("MessageDecoder1", new LengthFieldBasedFrameDecoder(int.MaxValue, 4, 4));
                    channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder());

                    channel.Pipeline.AddLast("MessageDecoder", new MessageEncoder());
                    channel.Pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, 60));
                    channel.Pipeline.AddLast("ClientHeartbeatHandler", new ClientHeartbeatHandler());
                    channel.Pipeline.AddLast("ServerHandler", new ClientHandler());
                });
            var ip = "127.0.0.1";
			//ip="192.168.111.133";
            int port = 8007;

            for (int i = 0; i < 16; i++)
            {
                Task.Run(async () =>
                {
                    var client = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
                    await Test(100000, client);
                });

               // await Task.Delay(10);
            }

        }

        private static async Task Test(int count, IChannel client)
        {
            for (int i = 0; i < count; i++)
            {
                Message message = new Message() { MessageType = MessageType.Request };
                message.Data = Encoding.UTF8.GetBytes(i + GetLargeMsg(10000));
                await client.WriteAsync(message);
                await Task.Delay(1);
            }
            GC.Collect();
        }

        private static string GetLargeMsg(int length)
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
