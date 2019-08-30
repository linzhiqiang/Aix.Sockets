using Aix.SocketCore.Channels;
using Aix.SocketCore.EventLoop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.DefaultHandlers
{
    public enum IdleState
    {
        ReaderIdle,
        WriterIdle,
        AllIdle
    }
    public class IdleStateEvent
    {
        public IdleStateEvent(IdleState state, bool first)
        {
            this.State = state;
            this.First = first;
        }

        public IdleState State { get; private set; }

        public bool First { get; private set; }
    }
    public class IdleStateHandler : ChannelHandlerAdapter
    {
        bool Open = false;

        readonly TimeSpan _readerIdleTime;
        readonly TimeSpan _writerIdleTime;
        readonly TimeSpan _allIdleTime;

        DateTime _lastReadTime;
        bool _firstReaderIdleEvent = true;

        DateTime _lastWriteTime;
        bool _firstWriterIdleEvent = true;

        bool _firstAllIdleEvent = true;

        public IdleStateHandler(
           int readerIdleTimeSeconds, int writerIdleTimeSeconds, int allIdleTimeSeconds)
           : this(TimeSpan.FromSeconds(readerIdleTimeSeconds), TimeSpan.FromSeconds(writerIdleTimeSeconds),TimeSpan.FromSeconds(allIdleTimeSeconds))
        {
        }

        public IdleStateHandler(TimeSpan readerIdleTime, TimeSpan writerIdleTime, TimeSpan allIdleTime)
        {
            _readerIdleTime = readerIdleTime;
            _writerIdleTime = writerIdleTime;
            _allIdleTime = allIdleTime;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            //连接建立时开始执行任务
            Init(context);
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            //连接关闭时关闭定时
            Destroy(context);
            base.ChannelInactive(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            _lastReadTime = DateTime.Now;
            base.ChannelRead(context, message);
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            _lastWriteTime = DateTime.Now;
            return base.WriteAsync(context, message);
        }

        private void Init(IChannelHandlerContext context)
        {
            Open = true;
            if (_readerIdleTime > TimeSpan.Zero)
            {
                context.Channel.EventExecutor.Schedule(() =>
                {
                    ReadIdle(context);
                }, _readerIdleTime);
            }

            if (_writerIdleTime > TimeSpan.Zero)
            {
                context.Channel.EventExecutor.Schedule(() =>
                {
                    WriteIdle(context);
                }, _writerIdleTime);
            }

            if (_allIdleTime > TimeSpan.Zero)
            {
                context.Channel.EventExecutor.Schedule(() =>
                {
                    AllIdle(context);
                }, _allIdleTime);
            }
        }

        private void Destroy(IChannelHandlerContext context)
        {
            Open = false;
        }

        private void ReadIdle(IChannelHandlerContext context)
        {
            if (Open == false) return;
            var delay = _lastReadTime.Add(_readerIdleTime) - DateTime.Now;
            if (delay.Ticks <=0)
            {
                context.Channel.EventExecutor.Schedule(() => { ReadIdle(context); }, _readerIdleTime);
                //触发读空闲事件
                var first = _firstReaderIdleEvent;
                _firstReaderIdleEvent = false;
                context.FireUserEventTriggered(new IdleStateEvent(IdleState.ReaderIdle, first));
            }
           else
            {
                context.Channel.EventExecutor.Schedule(() => { ReadIdle(context); }, delay);
            }
        }

        private void WriteIdle(IChannelHandlerContext context)
        {
            if (Open == false) return;
            var delay = _lastWriteTime.Add(_writerIdleTime) - DateTime.Now;
            if (delay.Ticks <=0)
            {
                context.Channel.EventExecutor.Schedule(() => { WriteIdle(context); }, _writerIdleTime);
                //触发写空闲事件
                var first = _firstWriterIdleEvent;
                _firstWriterIdleEvent = false;
                context.FireUserEventTriggered(new IdleStateEvent(IdleState.WriterIdle, first));
            }
           else 
            {
                context.Channel.EventExecutor.Schedule(() => { WriteIdle(context); }, delay);
            }
        }

        private void AllIdle(IChannelHandlerContext context)
        {
            if (Open == false) return;

            var delay =   Max(_lastReadTime, _lastWriteTime).Add( _allIdleTime) - DateTime.Now;
            if (delay.Ticks <=0)
            {
                context.Channel.EventExecutor.Schedule(() => { AllIdle(context); }, _allIdleTime);
                //触发读和写空闲事件
                var first = _firstAllIdleEvent;
                _firstAllIdleEvent = false;
                context.FireUserEventTriggered(new IdleStateEvent(IdleState.AllIdle, first));
            }
          else
            {
                context.Channel.EventExecutor.Schedule(() => {AllIdle(context); }, delay);
            }
        }

        DateTime Max(DateTime date1, DateTime date2)
        {
            return date1 > date2 ? date1 : date2;
        }

    }
}
