using System;
using System.Net;
using System.Threading.Tasks;

namespace Aix.SocketCore.Channels
{

    public class SkipAttribute : Attribute
    {

    }
    public interface IChannelHandler
    {
        #region 入站
        void ChannelActive(IChannelHandlerContext context);

        void ChannelInactive(IChannelHandlerContext context);

        void ChannelRead(IChannelHandlerContext context, object message);

        void ExceptionCaught(IChannelHandlerContext context, Exception exception);

        void UserEventTriggered(IChannelHandlerContext context, object evt);

        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region 出站

        Task DeregisterAsync(IChannelHandlerContext context);

        Task BindAsync(IChannelHandlerContext context, EndPoint localAddress);

        Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress);

        Task DisconnectAsync(IChannelHandlerContext context);

        Task CloseAsync(IChannelHandlerContext context);

        Task WriteAsync(IChannelHandlerContext context, object message);

        void Read(IChannelHandlerContext context);

        #endregion
    }

}
