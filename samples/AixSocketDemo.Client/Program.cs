using AixSocket;
using AixSocket.Bootstrapping;
using AixSocket.Channels;
using AixSocket.Channels.Sockets;
using AixSocket.DefaultHandlers;
using AixSocket.EventLoop;
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
            MultithreadEventLoopGroup workerGroup = new MultithreadEventLoopGroup(1);
            workerGroup.Start();

            var bootstrap = new ClientBootstrap();
            bootstrap
                .Group(workerGroup)
                .Channel<TcpSocketChannel>()
                .WorkerHandler(channel =>
                {
                    channel.Pipeline.AddLast("MessageDecoder", new MessageDecoder());
                    channel.Pipeline.AddLast("MessageDecoder", new MessageEncoder());
                    channel.Pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, 60));
                    channel.Pipeline.AddLast("ClientHeartbeatHandler", new ClientHeartbeatHandler());
                    channel.Pipeline.AddLast("ServerHandler", new ClientHandler());
                });
            var ip = "127.0.0.1";
			//ip="192.168.111.133";
            int port = 8007;

            for (int i = 0; i < 100; i++)
            {
                Task.Run(async () =>
                {
                    var client = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
                    await Test(1000, client);
                });
            }

        }

        private static async Task Test(int count, IChannel client)
        {
            for (int i = 0; i < count; i++)
            {
                Message message = new Message() { MessageType = MessageType.Request };
                message.Data = Encoding.UTF8.GetBytes(i + GetLargeMsg(10));
               await  client.WriteAsync(message);
                //await Task.Delay(3000);
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
