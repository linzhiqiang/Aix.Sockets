using Aix.SocketCore.Channels;
using Aix.SocketCore.Logging;
using AixSocket.Logging;
using AixSocketDemo.Common.Codecs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AixSocketDemo.Server.Handlers
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<ServerHandler>();
        public override void ChannelActive(IChannelHandlerContext context)
        {
            Logger.LogInformation("连接建立");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Logger.LogInformation("连接关闭");
            base.ChannelInactive(context);
        }
        static int Count = 0;
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = message as Message;
            if (msg == null)
            {
                context.FireChannelRead(message);
                return;
            }

            if (msg.MessageType == MessageType.Request || msg.MessageType == MessageType.Notify)
            {
                var data = msg.Data;
                var str = Encoding.UTF8.GetString(data);
                // Console.WriteLine(str);
                var count = Interlocked.Increment(ref Count);
                //Console.WriteLine("接收数据：" + (count));
                Logger.LogInformation("接收数据：" + count);

                msg.MessageType = MessageType.Response;
                context.Channel.WriteAsync(message).Wait();
            }
            else
            {
                context.FireChannelRead(message);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine(exception);
            base.ExceptionCaught(context, exception);
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {

            Logger.LogInformation("读超时:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
            base.UserEventTriggered(context, evt);
        }
    }
}
