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

    /*
     *context 是触发下一个（管道的下一个）
     * pipeline是从头触发
     * channel也是从头触发的（内部调用pipeline触发）
     */

    public class ServerHandler : ChannelHandlerAdapter
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<ServerHandler>();
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

            if (msg.MessageType == MessageType.Request || msg.MessageType == MessageType.Notify)
            {
                //var data = msg.Data;
                //var str = Encoding.UTF8.GetString(data);
                //// Console.WriteLine(str);
                //var count = Interlocked.Increment(ref Count);
                ////Console.WriteLine("接收数据：" + (count));
                //Logger.LogInformation("接收数据：" + count);

                //msg.MessageType = MessageType.Response;
                //context.Channel.WriteAsync(message).Wait();
                Task.Run(async () =>
                {
                    await BusinessProcess(context, msg);
                });

            }
            else
            {
                context.FireChannelRead(message);
            }
        }

        private async Task BusinessProcess(IChannelHandlerContext context, Message msg)
        {
            var data = msg.Data;
            var str = Encoding.UTF8.GetString(data);
            var count = Interlocked.Increment(ref Count);
            Logger.LogInformation("接收数据：" + count);

            //返回结果
            if (msg.MessageType == MessageType.Request)
            {
                msg.MessageType = MessageType.Response;
                await context.Channel.WriteAsync(msg);
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

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {

            Logger.LogInformation("读超时:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
            base.UserEventTriggered(context, evt);
        }
    }
}
