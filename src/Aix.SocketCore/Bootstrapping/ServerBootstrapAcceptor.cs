using Aix.SocketCore.Channels;
using Aix.SocketCore.EventLoop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Bootstrapping
{
    public class ServerBootstrapAcceptor : ChannelHandlerAdapter
    {
        private Action<IChannel> InitChildChannel;
        private MultithreadEventLoopGroup WorkerGroup;

        public ServerBootstrapAcceptor(MultithreadEventLoopGroup workerGroup, Action<IChannel> initChildChannel)
        {
            WorkerGroup = workerGroup;
            InitChildChannel = initChildChannel;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var child = (IChannel)message;
            //在这里初始化 服务端接收的客户端连接
            InitChildChannel(child);

            //这个时候还没有事件循环呢
            ((IChannelUnsafe)child).UnsafeRegisterAsync(WorkerGroup.GetNext());

            //触发连接事件
            child.EventExecutor.Execute(() =>
            {
                child.Pipeline.FireChannelActive();
            });

            context.FireChannelRead(message);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            //记录启动异常
            base.ExceptionCaught(context, exception);
        }


    }
}
