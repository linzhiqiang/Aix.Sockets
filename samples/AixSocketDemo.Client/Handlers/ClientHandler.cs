using Aix.SocketCore.Channels;
using AixSocket.Logging;
using AixSocketDemo.Common.Codecs;
using AixSocketDemo.Common.Invokes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace AixSocketDemo.Client.Handlers
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<ClientHandler>();
        public override void ChannelActive(IChannelHandlerContext context)
        {
            IPEndPoint remoteIp = context.Channel.RemoteAddress as IPEndPoint;
            string ip = remoteIp.Address.MapToIPv4().ToString() + ":" + remoteIp.Port;
            Logger.LogInformation($"连接建立：{ip}");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            IPEndPoint remoteIp = context.Channel.RemoteAddress as IPEndPoint;
            string ip = remoteIp.Address.MapToIPv4().ToString() + ":" + remoteIp.Port;
            Logger.LogInformation($"连接关闭：{ip}");
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
                ResponseManage.Instance.SetResult(msg.RequestId, msg);
                /*
                var data = msg.Data;
                var str = Encoding.UTF8.GetString(data);
                var count = Interlocked.Increment(ref Count);
               // if(count % 10000==0 || count +10 >= 8000 * 10000)
                Logger.LogInformation("接收数据：" + (count));
                //Console.WriteLine(str);
                */
            }
            else
            {
                context.FireChannelRead(message);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            IPEndPoint remoteIp = context.Channel.RemoteAddress as IPEndPoint;
            string ip = remoteIp.Address.MapToIPv4().ToString() + ":" + remoteIp.Port;
            Logger.LogError(exception, $"异常：{ip}");
            context.CloseAsync();
            //base.ExceptionCaught(context, exception);
        }
    }
}
