using AixSocket.Channels;
using AixSocket.Logging;
using AixSocketDemo.Common.Codecs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AixSocketDemo.Client.Handlers
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<ClientHandler>();
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

            if (msg.MessageType == MessageType.Response)
            {
                var data = msg.Data;
                var str = Encoding.UTF8.GetString(data);
                var count = Interlocked.Increment(ref Count);
                Logger.LogInformation("接收数据：" + (count));
                //Console.WriteLine(str);
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
    }
}
