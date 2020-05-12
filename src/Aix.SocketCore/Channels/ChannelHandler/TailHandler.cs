using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Channels
{
    public class TailHandler : ChannelHandlerAdapter
    {
        #region 入站

        [Skip]
        public override void ChannelActive(IChannelHandlerContext context)
        {
        }

        [Skip]
        public override void ChannelInactive(IChannelHandlerContext context)
        {
        }

        [Skip]
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
        }

        [Skip]
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
        }

        [Skip]
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
        }

        #endregion
    }
}
