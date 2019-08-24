using AixSocket.Channels;
using AixSocket.DefaultHandlers;
using AixSocket.Logging;
using AixSocketDemo.Common.Codecs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Server.Handlers
{
    public class ServerHeartbeatHandler : ChannelHandlerAdapter
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<ServerHeartbeatHandler>();
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = message as Message;
            if (msg != null && msg.MessageType == MessageType.Heartbeat)
            {
                Logger.LogInformation($"收到心跳:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}");
                return;
            }

            base.ChannelRead(context, message);
        }
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            var idelEvent = evt as IdleStateEvent;
            if (idelEvent == null)
            {
                base.UserEventTriggered(context, evt);
                return;
            }

            IdleStateEvent idleStateEvent = evt as IdleStateEvent;
            if (idleStateEvent.State == IdleState.AllIdle && idelEvent.First==false)
            {
                Logger.LogInformation("服务端空闲超时" + DateTime.Now.ToString("HH:mm:ss fff"));
                Logger.LogInformation($"心跳停止，关闭连接:{context.Channel.RemoteAddress}");
                context.Channel.CloseAsync();
            }
            else if (idleStateEvent.State == IdleState.ReaderIdle)
            {
                Logger.LogInformation("服务端读超时" + DateTime.Now.ToString("HH:mm:ss fff"));
            }
            else if (idleStateEvent.State == IdleState.WriterIdle)
            {
                Logger.LogInformation("服务端写超时" + DateTime.Now.ToString("HH:mm:ss fff"));
            }

        }
    }
}
