using AixSocket.Channels;
using AixSocket.DefaultHandlers;
using AixSocket.Logging;
using AixSocketDemo.Common.Codecs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Client.Handlers
{
    public class ClientHeartbeatHandler : ChannelHandlerAdapter
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<ClientHandler>();
        public static Message HeartbeatMessage = new Message { MessageType = MessageType.Heartbeat };

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = message as Message;
            if (msg == null)
            {
                context.FireChannelRead(message);
                return;
            }

            if (msg.MessageType == MessageType.Heartbeat)
            {
                Logger.LogInformation($"收到心跳:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}");
            }
            else
            {
                context.FireChannelRead(message);
            }
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
            if (idleStateEvent.State == IdleState.AllIdle)
            {
                Logger.LogInformation($"客户端空闲超时发送心跳：{DateTime.Now.ToString("HH:mm:ss")}");
                context.WriteAsync(HeartbeatMessage);
            }
            else if (idleStateEvent.State == IdleState.ReaderIdle)
            {
                Logger.LogInformation("客户端读超时" + DateTime.Now.ToString("HH:mm:ss fff"));
            }
            else if (idleStateEvent.State == IdleState.WriterIdle)
            {
                Logger.LogInformation("客户端写超时" + DateTime.Now.ToString("HH:mm:ss fff"));
            }

        }
    }
}
