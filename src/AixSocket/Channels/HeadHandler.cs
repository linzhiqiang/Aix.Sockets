using System;
using System.Net;
using System.Threading.Tasks;

namespace AixSocket.Channels
{
    /// <summary>
    /// 这里是出站事件的 最后一步，所以在这里调用底层操作了。而入站事件在channel中通过调用pipeline.Fire***来主动触发了。
    /// 在外部 只能通过pipeline触发入站事件，而出站事件通过channel触发。context只是管道内部决定是否触发下一个handler
    /// </summary>
    public class HeadHandler : ChannelHandlerAdapter
    {
        public override Task DeregisterAsync(IChannelHandlerContext context)
        {
            IChannelUnsafe @unsafe = context.Channel as IChannelUnsafe;
            return @unsafe.UnsafeDeregisterAsync();
        }
        public override Task BindAsync(IChannelHandlerContext context, EndPoint localAddress)
        {
            IChannelUnsafe @unsafe = context.Channel as IChannelUnsafe;
            return @unsafe.UnsafeBindAsync(localAddress);
        }

        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress)
        {
            IChannelUnsafe @unsafe = context.Channel as IChannelUnsafe;
            return @unsafe.UnsafeConnectAsync(remoteAddress);
        }

        public override Task DisconnectAsync(IChannelHandlerContext context)
        {
            IChannelUnsafe @unsafe = context.Channel as IChannelUnsafe;
            return @unsafe.UnsafeDisconnectAsync();
        }

        public override Task CloseAsync(IChannelHandlerContext context)
        {
            IChannelUnsafe @unsafe = context.Channel as IChannelUnsafe;
            return @unsafe.UnsafeCloseAsync();
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            IChannelUnsafe @unsafe = context.Channel as IChannelUnsafe;
            return @unsafe.UnsafeWriteAsync(message);
        }

        public override void Read(IChannelHandlerContext context)
        {
            //触发开始read，一般都是自动开始接收，所以这里一般不会主动调用的
            IChannelUnsafe @unsafe = context.Channel as IChannelUnsafe;
            @unsafe.UnsafeBeginRead();
        }

    }

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
