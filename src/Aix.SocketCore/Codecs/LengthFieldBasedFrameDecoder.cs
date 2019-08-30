using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Codecs
{
    public class LengthFieldBasedFrameDecoder : ChannelHandlerAdapter
    {
        readonly int maxFrameLength;
        readonly int lengthFieldOffset;
        readonly int lengthFieldLength;
        readonly int lengthFieldEndOffset;
        readonly int lengthAdjustment;
        readonly int initialBytesToStrip;

        public LengthFieldBasedFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip)
        {
            if (maxFrameLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrameLength), "maxFrameLength must be a positive integer: " + maxFrameLength);
            }
            if (lengthFieldOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthFieldOffset), "lengthFieldOffset must be a non-negative integer: " + lengthFieldOffset);
            }
            if (initialBytesToStrip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialBytesToStrip), "initialBytesToStrip must be a non-negative integer: " + initialBytesToStrip);
            }
            if (lengthFieldOffset > maxFrameLength - lengthFieldLength)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrameLength), "maxFrameLength (" + maxFrameLength + ") " +
                    "must be equal to or greater than " +
                    "lengthFieldOffset (" + lengthFieldOffset + ") + " +
                    "lengthFieldLength (" + lengthFieldLength + ").");
            }

            this.maxFrameLength = maxFrameLength;
            this.lengthFieldOffset = lengthFieldOffset;
            this.lengthFieldLength = lengthFieldLength;
            this.lengthAdjustment = lengthAdjustment;
            this.lengthFieldEndOffset = lengthFieldOffset + lengthFieldLength;
            this.initialBytesToStrip = initialBytesToStrip;
        }
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            IByteBuffer byteBuf = message as IByteBuffer;
            if (byteBuf == null)
            {
                context.FireChannelRead(message);
                //base.ChannelRead(context, message);
                return;
            }
            //智能解码


        }
    }
}
