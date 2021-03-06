﻿using Aix.SocketCore.Buffers;
using Aix.SocketCore.Config;
using Aix.SocketCore.EventLoop;
using Aix.SocketCore.Foundation;
using Aix.SocketCore.Utils;
using AixSocket.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.SocketCore.Channels.Sockets
{
    public class TcpSocketChannel : AbstractChannel, IChannelUnsafe
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<TcpServerSocketChannel>();
        public override bool Open { get; protected set; }
        private Socket Socket = null;
        private volatile bool IsBeginRead = false;

        private SocketChannelAsyncOperation ReceiveEventArgs;

        public TcpSocketChannel()
           : this(new Socket(SocketType.Stream, ProtocolType.Tcp))
        {
        }

        public TcpSocketChannel(AddressFamily addressFamily)
            : this(new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
        }

        public TcpSocketChannel(Socket socket)
            : this(null, socket, false)
        {
        }

        public TcpSocketChannel(IChannel parent, Socket socket, bool connected)
           : base(parent)
        {
            Socket = socket;
            this.Open = connected;

            this.CacheLocalAddress();
            this.CacheRemoteAddress();
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

        protected override EndPoint LocalAddressInternal => this.Socket.LocalEndPoint;

        protected override EndPoint RemoteAddressInternal
        {
            get
            {
                try
                {
                    return this.Socket.RemoteEndPoint;
                }
                catch
                {
                    return RequestRemoteAddress;
                }
            }

        }

        private EndPoint RequestRemoteAddress;

        private TaskCompletionSource ConnectPromise;

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
            throw new NotSupportedException();
        }

        public Task UnsafeConnectAsync(EndPoint remoteAddress)
        {
            if (Open) return Task.CompletedTask;

            if (ConnectPromise != null)
            {
                throw new InvalidOperationException("connection attempt already made");
            }
            RequestRemoteAddress = remoteAddress;
            var connectEventArgs = new SocketChannelAsyncOperation(this);
            connectEventArgs.RemoteEndPoint = remoteAddress;
            connectEventArgs.Completed += IO_Completed;
            bool connected = !this.Socket.ConnectAsync(connectEventArgs);
            if (connected)
            {
                ConnectFinish(connectEventArgs);
            }
            else
            {
                var timeout = ConfigContainer.Instance.ConnectTimeoutSecond;//10秒
                ConnectPromise = new TaskCompletionSource(remoteAddress);
                var scheduled = this.EventExecutor.Schedule(() =>
                {
                    var cause = new TimeoutException("connection timed out: " + timeout.ToString());
                    if (ConnectPromise != null && ConnectPromise.TrySetException(cause))
                    {
                        Util.CloseSafe(this);
                    }
                }, TimeSpan.FromSeconds(timeout));

                ConnectPromise.Task.ContinueWith((t, s) =>
                {
                    scheduled?.Cancel();
                    ConnectPromise = null;

                }, null);
                return ConnectPromise.Task;
                //这里做个超时处理
                //try
                //{
                //    await ConnectPromise.Task.TimeoutAfter(TimeSpan.FromSeconds(timeout));
                //}
                //catch (TimeoutException)
                //{
                //    ConnectPromise.SetException(new TimeoutException("连接超时"));
                //}
                //await ConnectPromise.Task;
            }

            return Task.CompletedTask;
        }

        private void ConnectFinish(SocketAsyncEventArgs e)
        {
            TaskCompletionSource promise = this.ConnectPromise;
            if (e.SocketError == SocketError.Success)
            {
                Open = true;
                this.Pipeline.FireChannelActive();
                promise.TryComplete();
            }
            else
            {
                this.UnsafeCloseAsync();
                // this.Pipeline.FireExceptionCaught(new SocketException((int)e.SocketError));
                promise.TrySetException(new SocketException((int)e.SocketError));
            }

            e.Dispose();
        }

        public Task UnsafeDisconnectAsync()
        {
            return this.UnsafeCloseAsync();
        }

        public Task UnsafeCloseAsync()
        {
            // var promise = new TaskCompletionSource();
            //if (this.Open)
            //{
            //    With.NoException(() => this.Socket.Shutdown(SocketShutdown.Both));
            //}
            With.NoException(() => this.Socket.Dispose());
            if (ReceiveEventArgs != null)
            {
                With.NoException(() => ReceiveEventArgs.Dispose());
                ReceiveEventArgs = null;
            }


            if (this.Open)  //只有有效的连接 才触发连接断开事件
            {
                this.Open = false;
                this.Pipeline.FireChannelInactive();
            }
            //promise.TryComplete();
            //return promise.Task;

            return Task.CompletedTask;

        }

        public Task UnsafeWriteAsync(object message)
        {
            var data = message as byte[];
            if (data != null && data.Length > 0)
            {
                Send(data, 0, data.Length);
            }
            return Task.CompletedTask;
        }

        public void Send(byte[] data, int offset, int size)
        {
            if (!Open) return;
            WithSocketException(() =>
            {
                var sent = this.Socket.Send(data, offset, size, SocketFlags.None, out SocketError errorCode);
                if (errorCode != SocketError.Success)
                {
                    Logger.LogError($"Send SocketError:{errorCode}");
                    throw new SocketException((int)errorCode);
                }
                //if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock)
                //{
                //    throw new SocketException((int)errorCode);
                //}
                //if (errorCode == SocketError.WouldBlock)
                //{
                //    throw new SocketException((int)errorCode);
                //}
                return false;
            });

        }

        /// <summary>
        /// 开始读取  （开始接收连接或开始接收数据）
        /// </summary>
        /// <returns></returns>
        public IChannel UnsafeBeginRead()
        {
            if (IsBeginRead) return this;
            IsBeginRead = true;

            ReceiveEventArgs = new SocketChannelAsyncOperation(this);
            ReceiveEventArgs.Completed += IO_Completed;
            //var buffer = new byte[256];
            //ReceiveEventArgs.SetBuffer(buffer, 0, buffer.Length);
            ReceiveEventArgs.SetBuffer(null, 0, 0);
            ReceiveEventArgs.AcceptSocket = this.Socket;
            StartReceive(ReceiveEventArgs);

            return this;
        }

        #region 接收数据

        private void StartReceive(SocketAsyncEventArgs e)
        {
            if (!e.AcceptSocket.ReceiveAsync(e))
            {
                ProcessReceive(e);
            }
        }

        private void AyncProcessReceive(SocketAsyncEventArgs e)
        {
            if (this.EventExecutor.InEventLoop)
            {
                ProcessReceive(e);
            }
            else
            {
                this.EventExecutor.Execute(() => { ProcessReceive(e); });
            }
        }

        private void ProcessReceive1(SocketAsyncEventArgs e)
        {
            if (this.Open == false) return;

            bool closed = false;
            try
            {
                do
                {
                    if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)//SocketAsyncEventArgs 
                                                                                       // if (e.SocketError == SocketError.Success) //不从SocketAsyncEventArgs的buff获取数据了
                    {
                        ProcessReceiveMessage(e);
                    }
                    else
                    {
                        closed = true;
                    }
                } while (Open && closed == false && e.AcceptSocket.ReceiveAsync(e) == false);
            }
            catch (SocketException ex)
            {
                Logger.LogError($"SocketError:{ex.SocketErrorCode}");
                if (ex.SocketErrorCode != SocketError.WouldBlock)
                {
                    closed = true;
                }
            }
            catch (ObjectDisposedException)
            {
                closed = true;
            }
            catch (Exception ex)
            {
                this.Pipeline.FireExceptionCaught(ex);
            }
            if (closed)
            {
                this.UnsafeCloseAsync();
            }
        }

        private void ProcessReceiveMessage(SocketAsyncEventArgs e)
        {
            //这里可以调用解码链
            // channel.ChannelHandler.Read(channel, e.Buffer, e.Offset, e.BytesTransferred);
            var data = new byte[e.BytesTransferred];
            Array.Copy(e.Buffer, 0, data, 0, data.Length);
            this.Pipeline.FireChannelRead(data);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (this.Open == false) return;
            bool closed = false;
            WithSocketException(() =>
            {
                do
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        IByteBuffer byteBuf = new ByteBuffer(ConfigContainer.Instance.BufferSize, int.MaxValue);
                        int received = e.AcceptSocket.Receive(byteBuf.Array, byteBuf.WriterIndex, byteBuf.WritableBytes, SocketFlags.None, out SocketError errorCode);
                        if (errorCode != SocketError.Success)
                        {
                            Logger.LogError($"Receive SocketError:{errorCode}");
                            throw new SocketException((int)errorCode);
                        }
                        if (received == 0)
                        {
                            closed = true;
                            break;
                        }
                        byteBuf.SetWriterIndex(byteBuf.WriterIndex + received);
                        this.Pipeline.FireChannelRead(byteBuf);
                        byteBuf = null;
                    }
                    else
                    {
                        closed = true;
                    }
                } while (Open && closed == false && e.AcceptSocket.ReceiveAsync(e) == false);
                return closed;
            });
        }
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    if (this.EventExecutor.InEventLoop)
                    {
                        ConnectFinish(e);
                    }
                    else
                    {
                        this.EventExecutor.Execute(() => { ConnectFinish(e); });
                    }

                    break;
                case SocketAsyncOperation.Receive:
                    AyncProcessReceive(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void WithSocketException(Func<bool> action)
        {
            bool closed = false;
            try
            {
                closed = action();
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.WouldBlock)
                {
                    closed = true;
                }
                else
                {
                    Logger.LogError(ex, $"socket阻塞,SocketError={ex.SocketErrorCode}");
                    //MyThread.Sleep(1000);
                }
            }
            catch (ObjectDisposedException)
            {
                closed = true;
            }
            catch (Exception ex)
            {
                this.Pipeline.FireExceptionCaught(ex); //触发异常事件，是否需要关闭 由异常事件中处理决定
            }
            if (closed)
            {
                this.UnsafeCloseAsync(); //由于网络原因需要关闭
            }
        }
        #endregion

        #endregion

    }
}
