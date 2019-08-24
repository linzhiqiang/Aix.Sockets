using AixSocket.EventLoop;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AixSocket.Channels
{
    public interface IChannel
    {
        bool Open { get; }

        EndPoint LocalAddress { get; }

        EndPoint RemoteAddress { get; }

        IChannelPipeline Pipeline { get; }

        IEventExecutor EventExecutor { get; }

        Task DeregisterAsync();

        Task BindAsync(EndPoint localAddress);

        Task ConnectAsync(EndPoint remoteAddress);

        Task DisconnectAsync();

        Task CloseAsync();

        Task WriteAsync(object message);

        /// <summary>
        /// 开始读取  （开始接收连接或开始接收数据）
        /// </summary>
        /// <returns></returns>
        IChannel Read();
    }

    public interface IChannelUnsafe
    {
        Task UnsafeRegisterAsync(IEventExecutor eventExecutor);

        Task UnsafeDeregisterAsync();
        Task UnsafeBindAsync(EndPoint localAddress);

        Task UnsafeConnectAsync(EndPoint remoteAddress);

        Task UnsafeDisconnectAsync();

        Task UnsafeCloseAsync();

        Task UnsafeWriteAsync(object message);

        /// <summary>
        /// 开始读取  （开始接收连接或开始接收数据）
        /// </summary>
        /// <returns></returns>
        IChannel UnsafeBeginRead();
    }

    

   
}
