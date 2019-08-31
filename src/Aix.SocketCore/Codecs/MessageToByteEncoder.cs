using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.Codecs
{
    public abstract class MessageToByteEncoder<T> : ChannelHandlerAdapter
    {

        public override async Task WriteAsync(IChannelHandlerContext context, object message)
        {
            if (message is T)
            {
                var cast = (T)message;
                IByteBuffer byteBuffer = new ByteBuffer(256, int.MaxValue);
                this.Encode(context, cast, byteBuffer);

                if (byteBuffer.IsReadable())
                {
                    await context.WriteAsync(byteBuffer.ReadeBytes());
                }
            }
            else
            {
                await base.WriteAsync(context, message);
            }
        }

        protected abstract void Encode(IChannelHandlerContext context, T message, IByteBuffer output);
    }
}
