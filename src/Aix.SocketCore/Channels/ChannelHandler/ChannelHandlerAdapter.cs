using System;
using System.Net;
using System.Threading.Tasks;

namespace Aix.SocketCore.Channels
{
    public class ChannelHandlerAdapter : IChannelHandler
    {
        [Skip]
        public virtual void HandlerAdded(IChannelHandlerContext context)
        {
            //这里没有管道执行，哪个ChannelHandlerAdapter中删除就在该ChannelHandlerAdapter中执行该事件
        }

        [Skip]
        public virtual void HandlerRemoved(IChannelHandlerContext context)
        {
            //这里没有管道执行，哪个ChannelHandlerAdapter中删除就在该ChannelHandlerAdapter中执行该事件
        }

        [Skip]
        public virtual void ChannelActive(IChannelHandlerContext context)
        {
            context.FireChannelActive();
        }

        [Skip]
        public virtual void ChannelInactive(IChannelHandlerContext context)
        {
            context.FireChannelInactive();
        }

        [Skip]
        public virtual void ChannelRead(IChannelHandlerContext context, object message)
        {
            context.FireChannelRead(message);
        }

        [Skip]
        public virtual void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.FireExceptionCaught(exception);
        }

        [Skip]
        public virtual void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            context.FireUserEventTriggered(evt);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [Skip]
        public virtual Task DeregisterAsync(IChannelHandlerContext context)
        {
            return context.DeregisterAsync();
        }

        [Skip]
        public virtual Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
        {
            return context.BindAsync(localAddress);
        }

        [Skip]
        public virtual Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress)
        {
            return context.ConnectAsync(remoteAddress);
        }

        [Skip]
        public virtual Task DisconnectAsync(IChannelHandlerContext context)
        {
            return context.DisconnectAsync();
        }

        [Skip]
        public virtual Task CloseAsync(IChannelHandlerContext context)
        {
            return context.CloseAsync();
        }

        [Skip]
        public virtual Task WriteAsync(IChannelHandlerContext context, object message)
        {
            return context.WriteAsync(message);
        }

        [Skip]
        public virtual void Read(IChannelHandlerContext context)
        {
            context.Read();
        }


    }
}
