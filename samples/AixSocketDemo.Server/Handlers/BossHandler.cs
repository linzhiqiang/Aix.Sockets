using AixSocket.Channels;
using AixSocket.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AixSocketDemo.Server.Handlers
{
    public class BossHandler : ChannelHandlerAdapter
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<BossHandler>();
        public override void ChannelActive(IChannelHandlerContext context)
        {
            Logger.LogInformation("服务器启动成功");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Logger.LogInformation("服务器关闭");
            base.ChannelInactive(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine(exception);
            base.ExceptionCaught(context, exception);
        }
    }
}
