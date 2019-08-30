using Aix.SocketCore.Config;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Aix.SocketCore.Channels
{
    public interface IChannelPipeline
    {
        IChannel Channel { get; }

        IChannelPipeline AddFirst(string name, IChannelHandler handler);
        IChannelPipeline AddLast(string name, IChannelHandler handler);
        IChannelPipeline AddBefore(string baseName, string name, IChannelHandler handler);
        IChannelPipeline AddAfter(string baseName, string name, IChannelHandler handler);

        IChannelPipeline Remove(IChannelHandler handler);
        IChannelHandler Remove(string name);
        T Remove<T>() where T : class, IChannelHandler;


        #region 入站
        IChannelPipeline FireChannelActive();

        IChannelPipeline FireChannelInactive();

        IChannelPipeline FireChannelRead(object message);

        IChannelPipeline FireExceptionCaught(Exception ex);

        IChannelPipeline FireUserEventTriggered(object evt);

        #endregion

        #region 出站

        Task DeregisterAsync();

        Task BindAsync(EndPoint localAddress);

        Task ConnectAsync(EndPoint remoteAddress);

        Task DisconnectAsync();

        Task CloseAsync();

        Task WriteAsync(object message);

        IChannelPipeline Read();

        #endregion

    }

    public class DefaultChannelPipeline : IChannelPipeline
    {
        public IChannel Channel { get; }

        IChannelHandlerContext Head = null;
        IChannelHandlerContext Tail = null;
        public DefaultChannelPipeline(IChannel channel)
        {
            Channel = channel;
            this.Head = new ChannelHandlerContext(this, "Head", new HeadHandler());
            this.Tail = new ChannelHandlerContext(this, "Tail", new TailHandler());

            this.Head.Next = this.Tail;
            this.Tail.Prev = this.Head;
        }

        #region 基本功能
        public IChannelPipeline AddFirst(string name, IChannelHandler handler)
        {
            var newNode = new ChannelHandlerContext(this, name, handler);
            var next = this.Head.Next;

            newNode.Prev = this.Head;
            newNode.Next = next;

            next.Prev = newNode;
            this.Head.Next = newNode;

            return this;
        }

        public IChannelPipeline AddLast(string name, IChannelHandler handler)
        {
            var newNode = new ChannelHandlerContext(this, name, handler);
            var prev = this.Tail.Prev;

            newNode.Prev = prev;
            newNode.Next = this.Tail;

            prev.Next = newNode;
            this.Tail.Prev = newNode;

            return this;
        }

        public IChannelPipeline AddBefore(string baseName, string name, IChannelHandler handler)
        {
            var current = GetContextByName(baseName);
            if (current == null) throw new Exception($"Name={baseName}的handler不存在");

            var newNode = new ChannelHandlerContext(this, name, handler);

            var prev = current.Prev;

            newNode.Prev = prev;
            newNode.Next = current;

            current.Prev = newNode;
            if (prev != null) prev.Next = newNode;

            return this;
        }
        public IChannelPipeline AddAfter(string baseName, string name, IChannelHandler handler)
        {
            var current = GetContextByName(baseName);
            if (current == null) throw new Exception($"Name={baseName}的handler不存在");

            var newNode = new ChannelHandlerContext(this, name, handler);

            var next = current.Next;

            newNode.Prev = current;
            newNode.Next = next;

            current.Next = newNode;
            if (next != null) next.Prev = newNode;

            return this;
        }

        public IChannelPipeline Remove(IChannelHandler handler)
        {
            Remove0(GetContextByHandler(handler));
            return this;
        }



        public IChannelHandler Remove(string name)
        {
            var context = GetContextByName(name);
            Remove0(context);
            return context?.Handler;
        }
        public T Remove<T>() where T : class, IChannelHandler
        {
            var context = GetContextByHandlerType<T>();
            Remove0(context);
            return context != null ? (T)context.Handler : default(T);
        }

        #endregion

        #region 入站

        public IChannelPipeline FireChannelActive()
        {
            this.Head.FireChannelActive();
            var autoRead = ConfigContainer.Instance.AutoRead;
            if (autoRead)
            {
                this.Channel.Read();
            }
            return this;
        }

        public IChannelPipeline FireChannelInactive()
        {
            this.Head.FireChannelInactive();
            return this;
        }

        public IChannelPipeline FireChannelRead(object message)
        {
            this.Head.FireChannelRead(message);
            return this;
        }

        public IChannelPipeline FireExceptionCaught(Exception ex)
        {
            this.Head.FireExceptionCaught(ex);
            return this;
        }

        public IChannelPipeline FireUserEventTriggered(object evt)
        {
            this.Head.FireUserEventTriggered(evt);
            return this;
        }
        #endregion

        #region 出站

        public Task DeregisterAsync()
        {
            return this.Tail.DeregisterAsync();
        }

        public Task BindAsync(EndPoint localAddress)
        {
            return this.Tail.BindAsync(localAddress);
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            return this.Tail.ConnectAsync(remoteAddress);
        }

        public Task DisconnectAsync()
        {
            return this.Tail.DisconnectAsync();
        }

        public Task CloseAsync()
        {
            return this.Tail.CloseAsync();
        }

        public Task WriteAsync(object message)
        {
            return this.Tail.WriteAsync(message);
        }

        public IChannelPipeline Read()
        {
            this.Tail.Read();
            return this;
        }

        #endregion

        #region      private 

        private void Remove0(IChannelHandlerContext context)
        {
            if (context == null) return;

            var prev = context.Prev;
            var next = context.Next;

            if (prev != null)
            {
                prev.Next = next;
            }

            if (next != null)
            {
                next.Prev = prev;
            }
        }

        private IChannelHandlerContext GetContextByName(string name)
        {
            var current = this.Head;
            while (current != null)
            {
                if (current.Name == name)
                {
                    break;
                }
                current = current.Next;
            }

            return current;
        }

        private IChannelHandlerContext GetContextByHandler(IChannelHandler handler)
        {
            var current = this.Head;
            while (current != null)
            {
                if (current.Handler == handler)
                {
                    return current;
                }
                current = current.Next;
            }

            return null;
        }

        private IChannelHandlerContext GetContextByHandlerType<T>() where T : class, IChannelHandler
        {
            var current = this.Head;
            while (current != null)
            {
                if (current.Handler is T)
                {
                    return current;
                }
                current = current.Next;
            }

            return null;
        }
        #endregion
    }

}
