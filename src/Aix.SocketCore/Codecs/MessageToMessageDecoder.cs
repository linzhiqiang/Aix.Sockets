using Aix.SocketCore.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Codecs
{
    public abstract class MessageToMessageDecoder<T> : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is T)
            {
                var cast = (T)message;
                var output = new List<object>();
                this.Decode(context, cast, output);
                foreach (var item in output)
                {
                    context.FireChannelRead(item);
                }
            }
            else
            {
                base.ChannelRead(context, message);
            }
        }

        protected internal abstract void Decode(IChannelHandlerContext context, T message, List<object> output);
    }
}
