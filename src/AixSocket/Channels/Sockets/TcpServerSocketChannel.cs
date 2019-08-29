using AixSocket.Config;
using AixSocket.EventLoop;
using AixSocket.Foundation;
using AixSocket.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AixSocket.Channels.Sockets
{
    public class TcpServerSocketChannel : AbstractChannel, IChannelUnsafe
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<TcpServerSocketChannel>();
        private Socket Socket = null;
        public override bool Open { get; protected set; }

        private volatile bool IsBeginRead = false;
        public TcpServerSocketChannel()
           : this(new Socket(SocketType.Stream, ProtocolType.Tcp))
        {
        }

        public TcpServerSocketChannel(AddressFamily addressFamily)
            : this(new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
        }
   
        public TcpServerSocketChannel(Socket socket)
            : base(null)
        {
            this.Socket = socket;
            this.Open = true;
            try
            {
                this.Socket.Blocking = false;
            }
            catch (SocketException ex)
            {
                try
                {
                    socket.Dispose();
                }
                catch (SocketException ex2)
                {
                    Logger.LogError(ex2, "Failed to close a partially initialized socket.");
                }

                throw new Exception("Failed to enter non-blocking mode.", ex);
            }
        }

        protected override EndPoint GetLocalAddress()
        {
            return this.Socket.LocalEndPoint;
        }

        protected override EndPoint GetRemoteAddress()
        {
            return null;
        }


        #region IChannelUnsafe

        public Task UnsafeRegisterAsync(IEventExecutor eventExecutor)
        {
            this.EventExecutor = eventExecutor;
            return Task.CompletedTask;
        }

        public Task UnsafeDeregisterAsync()
        {
            //this.EventExecutor = null;
            return Task.CompletedTask;
        }

        public Task UnsafeBindAsync(EndPoint localAddress)
        {
            this.Socket.Bind(localAddress);
            this.Socket.Listen(ConfigContainer.Instance.Backlog);

            this.Pipeline.FireChannelActive();
            // this.Read();//在FireChannelActive里面开始读了

            return Task.CompletedTask;
        }

        public Task UnsafeConnectAsync(EndPoint remoteAddress)
        {
            throw new NotSupportedException();
        }

        public Task UnsafeDisconnectAsync()
        {
            throw new NotSupportedException();
        }

        public Task UnsafeCloseAsync()
        {
            if (this.Open)
            {
                this.Open = false;
                this.Socket.Dispose();
                this.Pipeline.FireChannelInactive();
            }
            return Task.CompletedTask;
        }

        public Task UnsafeWriteAsync(object message)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 开始读取  （开始接收连接或开始接收数据）
        /// </summary>
        /// <returns></returns>
        public IChannel UnsafeBeginRead()
        {
            //开始接收连接
            //服务端socket启动时增加handler，在channelread中，接收新接收的连接，进行初始化，注册等任务。
            if (IsBeginRead == true) return this;
            IsBeginRead = true;

            var acceptEventArg = new SocketChannelAsyncOperation(this);
            acceptEventArg.Completed += AceptOperation_Completed;
            StartAccept(acceptEventArg);

            return this;
        }
        #region 接收连接
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            WithSocketException(() =>
            {
                acceptEventArg.AcceptSocket = null;
                if (!this.Socket.AcceptAsync(acceptEventArg)) //没有挂起pending
                {
                    ProcessAccept(acceptEventArg);
                }
            });
        }
        private void AsyncProcessAccept(SocketAsyncEventArgs e)
        {
            this.EventExecutor.Execute(() =>
            {
                WithSocketException(() => { ProcessAccept(e); });
            });
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            do
            {
                if (e.SocketError == SocketError.Success)
                {
                    var clientSocket = e.AcceptSocket;
                    e.AcceptSocket = null;
                    //创建新连接，服务端channelread事件
                    var message = new TcpSocketChannel(this, clientSocket, true);
                    this.Pipeline.FireChannelRead(message);
                }

                e.AcceptSocket = null;
            }
            while (!this.Socket.AcceptAsync(e));
            // Accept 继续接收一个连接 

            //StartAccept(e);
        }

        private void WithSocketException(Action action)
        {
            bool closed = false;
            try
            {
                action();
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted || ex.SocketErrorCode == SocketError.InvalidArgument)
            {
                closed = true;
            }
            catch (SocketException ex)
            {
                Logger.LogError(ex, "socket阻塞");
            }
            catch (ObjectDisposedException)
            {
                closed = true;
            }
            catch (Exception ex)
            {
                this.Pipeline.FireExceptionCaught(ex);
                closed = true;
            }

            if (closed && this.Open)
            {
                this.UnsafeCloseAsync();
            }
        }

        private void AceptOperation_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    AsyncProcessAccept(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }
        #endregion

        #endregion
    }
}
