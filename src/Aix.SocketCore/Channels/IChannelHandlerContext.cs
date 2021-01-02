using Aix.SocketCore.EventLoop;
using Aix.SocketCore.Foundation;
using Aix.SocketCore.Utils;
using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.SocketCore.Channels
{

    public interface IChannelHandlerContext
    {
        string Name { get; }
        IChannel Channel { get; }

        IChannelHandler Handler { get; }
        IChannelHandlerContext Next { get; set; }

        IChannelHandlerContext Prev { get; set; }

        //bool Added { get; }

        #region 入站
        IChannelHandlerContext FireChannelActive();

        IChannelHandlerContext FireChannelInactive();

        IChannelHandlerContext FireChannelRead(object message);

        IChannelHandlerContext FireExceptionCaught(Exception ex);

        IChannelHandlerContext FireUserEventTriggered(object evt);

        #endregion

        #region 出站

        Task DeregisterAsync();
        Task BindAsync(EndPoint localAddress);

        Task ConnectAsync(EndPoint remoteAddress);

        Task DisconnectAsync();

        Task CloseAsync();

        Task WriteAsync(object message);

        IChannelHandlerContext Read();

        #endregion
    }
    public class ChannelHandlerContext : IChannelHandlerContext
    {
        public IChannelHandlerContext Next { get; set; }

        public IChannelHandlerContext Prev { get; set; }


        public IChannel Channel => Pipeline.Channel;

        public IChannelHandler Handler { get; }

        public string Name { get; }

        private IChannelPipeline Pipeline { get; }

        IEventExecutor EventExecutor => Channel.EventExecutor;


        public bool Removed => throw new NotImplementedException();

        public ChannelHandlerContext(IChannelPipeline pipeline, string name, IChannelHandler handler)
        {
            this.Pipeline = pipeline;
            this.Name = name;
            this.Handler = handler;
        }

        #region 入站
        public IChannelHandlerContext FireChannelActive()
        {
            var next = FindContextInbound();
            if (next == null) return this;

            if (this.EventExecutor.InEventLoop)
            {
                ((ChannelHandlerContext)next).InvokeChannelActive();
            }
            else
            {
                this.EventExecutor.Execute(() => ((ChannelHandlerContext)next).InvokeChannelActive());
            }

            return this;
        }

        void InvokeChannelActive()
        {
            try
            {
                this.Handler.ChannelActive(this);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }

        }

        public IChannelHandlerContext FireChannelInactive()
        {
            var next = FindContextInbound();
            if (next == null) return this;

            if (this.EventExecutor.InEventLoop)
            {
                ((ChannelHandlerContext)next).InvokeChannelInactive();
            }
            else
            {
                this.EventExecutor.Execute(() => ((ChannelHandlerContext)next).InvokeChannelInactive());
            }

            return this;
        }

        void InvokeChannelInactive()
        {
            try
            {
                this.Handler.ChannelInactive(this);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }

        }

        public IChannelHandlerContext FireChannelRead(object message)
        {
            var next = FindContextInbound();
            if (next == null) return this;

            if (this.EventExecutor.InEventLoop)
            {
                ((ChannelHandlerContext)next).InvokeChannelRead(message);
            }
            else
            {
                this.EventExecutor.Execute(() => ((ChannelHandlerContext)next).InvokeChannelRead(message));
            }

            return this;
        }

        void InvokeChannelRead(object message)
        {
            try
            {
                this.Handler.ChannelRead(this, message);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }

        }

        public IChannelHandlerContext FireExceptionCaught(Exception ex)
        {
            var next = FindContextInbound();
            if (next == null) return this;

            if (this.EventExecutor.InEventLoop)
            {
                ((ChannelHandlerContext)next).InvokeExceptionCaught(ex);
            }
            else
            {
                this.EventExecutor.Execute(() => ((ChannelHandlerContext)next).InvokeExceptionCaught(ex));
            }

            return this;
        }
        void InvokeExceptionCaught(Exception cause)
        {
            try
            {
                this.Handler.ExceptionCaught(this, cause);
            }
            catch (Exception)
            {
                //记录log
            }
        }

        public IChannelHandlerContext FireUserEventTriggered(object evt)
        {
            var next = FindContextInbound();
            if (next == null) return this;

            if (this.EventExecutor.InEventLoop)
            {
                ((ChannelHandlerContext)next).InvokeUserEventTriggered(evt);
            }
            else
            {
                this.EventExecutor.Execute(() => ((ChannelHandlerContext)next).InvokeUserEventTriggered(evt));
            }
            return this;
        }

        void InvokeUserEventTriggered(object message)
        {
            try
            {
                this.Handler.UserEventTriggered(this, message);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }

        }
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region 出站
        public Task DeregisterAsync()
        {
            var next = FindContextOutbound();
            if (next == null) return Task.CompletedTask;

            if (this.EventExecutor.InEventLoop)
            {
                return ((ChannelHandlerContext)next).InvokeDeregisterAsync();
            }
            else
            {
                return SafeExecuteOutboundAsync(this.EventExecutor, () => ((ChannelHandlerContext)next).InvokeDeregisterAsync());
            }
        }

         Task InvokeDeregisterAsync()
        {
            try
            {
                return  this.Handler.DeregisterAsync(this);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }
            return Task.CompletedTask;
        }

        public Task BindAsync(EndPoint localAddress)
        {
            var next = FindContextOutbound();
            if (next == null) return Task.CompletedTask;

            if (this.EventExecutor.InEventLoop)
            {
                return ((ChannelHandlerContext)next).InvokeBindAsync(localAddress);
            }
            else
            {
                return SafeExecuteOutboundAsync(this.EventExecutor, () => ((ChannelHandlerContext)next).InvokeBindAsync(localAddress));
            }
        }

         Task InvokeBindAsync(EndPoint localAddress)
        {
            try
            {
                return  this.Handler.BindAsync(this, localAddress);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }
            return Task.CompletedTask;
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            var next = FindContextOutbound();
            if (next == null) return Task.CompletedTask;

            if (this.EventExecutor.InEventLoop)
            {
                return ((ChannelHandlerContext)next).InvokeConnectAsync(remoteAddress);
            }
            else
            {
                return SafeExecuteOutboundAsync(this.EventExecutor, () => ((ChannelHandlerContext)next).InvokeConnectAsync(remoteAddress));
            }
        }

         Task InvokeConnectAsync(EndPoint remoteAddress)
        {
            try
            {
                return this.Handler.ConnectAsync(this, remoteAddress);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }

            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            var next = FindContextOutbound();
            if (next == null) return Task.CompletedTask;

            if (this.EventExecutor.InEventLoop)
            {
                return ((ChannelHandlerContext)next).InvokeDisconnectAsync();
            }
            else
            {
                return SafeExecuteOutboundAsync(this.EventExecutor, () => ((ChannelHandlerContext)next).InvokeDisconnectAsync());
            }
        }

         Task InvokeDisconnectAsync()
        {
            try
            {
                return this.Handler.DisconnectAsync(this);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }

            return Task.CompletedTask;

        }

        public Task CloseAsync()
        {
            var next = FindContextOutbound();
            if (next == null) return Task.CompletedTask;

            if (this.EventExecutor.InEventLoop)
            {
                return ((ChannelHandlerContext)next).InvokeCloseAsync();
            }
            else
            {
                return SafeExecuteOutboundAsync(this.EventExecutor, () => ((ChannelHandlerContext)next).InvokeCloseAsync());
            }
        }

         Task InvokeCloseAsync()
        {
            try
            {
                return  this.Handler.CloseAsync(this);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }
            return Task.CompletedTask;
        }

        public Task WriteAsync(object message)
        {
            var next = FindContextOutbound();
            if (next == null) return Task.CompletedTask;

            if (this.EventExecutor.InEventLoop)
            {
                return ((ChannelHandlerContext)next).InvokeWriteAsync(message);
            }
            else
            {
                return SafeExecuteOutboundAsync(this.EventExecutor, () => ((ChannelHandlerContext)next).InvokeWriteAsync(message));
            }
        }

         Task InvokeWriteAsync(object message)
        {
            try
            {
                return  this.Handler.WriteAsync(this, message);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }

            return Task.CompletedTask;

        }

        public IChannelHandlerContext Read()
        {
            var next = FindContextOutbound();
            if (next == null) return this;

            if (this.EventExecutor.InEventLoop)
            {
                ((ChannelHandlerContext)next).InvokeRead();
            }
            else
            {
                this.EventExecutor.Execute(() => ((ChannelHandlerContext)next).InvokeRead());
            }
            return this;
        }

        void InvokeRead()
        {
            try
            {
                this.Handler.Read(this);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }

        }

        #endregion

        #region private 

        void NotifyHandlerException(Exception cause)
        {
            this.InvokeExceptionCaught(cause);
        }
        IChannelHandlerContext FindContextInbound()
        {
            return this.Next;
        }

        IChannelHandlerContext FindContextOutbound()
        {
            return this.Prev;
        }

        static Task SafeExecuteOutboundAsync(IEventExecutor executor, Func<Task> function)
        {
            var promise = new TaskCompletionSource();
            try
            {
                //executor.Execute((p, func) => ((Func<Task>)func)().LinkOutcome((TaskCompletionSource)p), promise, function);
                executor.Execute(() => function().LinkOutcome(promise));
            }
            catch (Exception cause)
            {
                promise.TrySetException(cause);
            }
            return promise.Task;
        }

        protected static bool IsSkippable(Type handlerType, string methodName) => IsSkippable(handlerType, methodName, Type.EmptyTypes);

        protected static bool IsSkippable(Type handlerType, string methodName, params Type[] paramTypes)
        {
            var newParamTypes = new Type[paramTypes.Length + 1];
            newParamTypes[0] = typeof(IChannelHandlerContext);
            Array.Copy(paramTypes, 0, newParamTypes, 1, paramTypes.Length);
            return handlerType.GetMethod(methodName, newParamTypes).GetCustomAttribute<SkipAttribute>(false) != null;
        }

        #endregion

    }
}
