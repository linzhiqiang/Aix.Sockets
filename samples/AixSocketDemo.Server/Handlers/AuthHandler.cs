using Aix.SocketCore.Channels;
using AixSocket.Logging;
using AixSocketDemo.Common.Codecs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Server.Handlers
{
  public  class AuthHandler : ChannelHandlerAdapter
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<ServerHandler>();
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Message msg = message as Message;
            if (msg == null) return;
            if (msg.MessageType != MessageType.Auth)
            {
                msg.Data = Encoding.UTF8.GetBytes("-9999,请认证");
                msg.MessageType = MessageType.Response;
                context.Channel.WriteAsync(msg);
            }
            else
            {
                // 开始认证了...
                Logger.LogInformation("认证成功");
                msg.Data = Encoding.UTF8.GetBytes("认证成功");
                msg.MessageType = MessageType.Response;
                context.Channel.WriteAsync(msg);
                context.Channel.Pipeline.Remove(this);
            }
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            Logger.LogInformation("HandlerAdded");
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            Logger.LogInformation("HandlerRemoved");
        }
    }
}
