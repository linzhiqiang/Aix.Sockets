using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Codecs
{
    public abstract class ByteToMessageDecoder : ChannelHandlerAdapter
    {
        private IByteBuffer cumulationBuff = null;
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            IByteBuffer byteBuf = message as IByteBuffer;
            if (byteBuf == null)
            {
                context.FireChannelRead(message);
                return;
            }

            if (cumulationBuff == null)
            {
                cumulationBuff = byteBuf;
            }
            else
            {
                cumulationBuff.WriteBytes(byteBuf.ReadeBytes());
            }

            var output = new List<object>();
            this.Decode(context, cumulationBuff, output);
            foreach (var item in output)
            {
                context.FireChannelRead(item);
            }

            if (output.Count > 0)
            {
                if (cumulationBuff != null && !cumulationBuff.IsReadable())
                {
                    cumulationBuff = null;
                }
                else
                {
                    cumulationBuff.DiscardReadBytes();
                }

            }
        }
        protected internal abstract void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output);
    }
    
}
