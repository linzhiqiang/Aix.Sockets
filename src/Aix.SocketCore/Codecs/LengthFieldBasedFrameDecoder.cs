using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Codecs
{
    public class LengthFieldBasedFrameDecoder : ByteToMessageDecoder
    {
        readonly int maxFrameLength;
        readonly int lengthFieldOffset;
        readonly int lengthFieldLength;
        readonly int lengthFieldEndOffset;
        readonly int lengthAdjustment;
        readonly int initialBytesToStrip;

        public LengthFieldBasedFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength)
            :this(maxFrameLength, lengthFieldOffset, lengthFieldLength,0,0)
        {
        }
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
        protected internal override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            object decoded = this.Decode(context, input);
            if (decoded != null)
            {
                output.Add(decoded);
            }
        }

        protected virtual object Decode(IChannelHandlerContext context, IByteBuffer input)
        {
            if(input.ReadableBytes < this.lengthFieldEndOffset)
            {
                return null;
            }
            long frameLength = this.GetUnadjustedFrameLength(input, input.ReaderIndex + this.lengthFieldOffset, this.lengthFieldLength);
            if (frameLength < 0)
            {
                input.SkipBytes(this.lengthFieldEndOffset);
                throw new Exception("negative pre-adjustment length field: " + frameLength);
            }

            //总长度  =长度域值+lengthAdjustment+lengthFieldEndOffset(前面头长度= lengthFieldOffset + lengthFieldLength)
            frameLength += this.lengthAdjustment + this.lengthFieldEndOffset;
            if (frameLength < this.lengthFieldEndOffset)
            {
                input.SkipBytes(this.lengthFieldEndOffset);
                throw new Exception("Adjusted frame length (" + frameLength + ") is less " +
                    "than lengthFieldEndOffset: " + this.lengthFieldEndOffset);
            }

            if (frameLength > this.maxFrameLength)
            {
                throw new Exception($"一个包长度超过最大值：{frameLength} >{maxFrameLength}");
            }

            if (input.ReadableBytes < frameLength)
            {
                return null;
            }
            if (this.initialBytesToStrip > frameLength)
            {
                input.SkipBytes((int)frameLength);
                throw new Exception("Adjusted frame length (" + frameLength + ") is less " +
                    "than initialBytesToStrip: " + this.initialBytesToStrip);
            }
            input.SkipBytes(this.initialBytesToStrip);

            int readerIndex = input.ReaderIndex;
            int actualFrameLength = (int)frameLength - this.initialBytesToStrip;
            IByteBuffer frame = this.ExtractFrame(context, input, readerIndex, actualFrameLength);
            input.SetReaderIndex(readerIndex + actualFrameLength);
            return frame;
        }

        protected virtual IByteBuffer ExtractFrame(IChannelHandlerContext context, IByteBuffer buffer, int index, int length)
        {
            IByteBuffer buff = buffer.Slice(index, length);
            return buff;
        }

        protected long GetUnadjustedFrameLength(IByteBuffer buffer, int offset, int length)
        {
            long frameLength;
            switch (length)
            {
                case 1:
                    frameLength = buffer.GetByte(offset);
                    break;
                case 2:
                    frameLength = BitConverter.IsLittleEndian ? buffer.GetUnsignedShort(offset) : buffer.GetUnsignedShortLE(offset);
                    break;
                case 3:
                    frameLength = BitConverter.IsLittleEndian ? buffer.GetUnsignedMedium(offset) : buffer.GetUnsignedMediumLE(offset);
                    break;
                case 4:
                    frameLength = BitConverter.IsLittleEndian ? buffer.GetInt(offset) : buffer.GetIntLE(offset);
                    break;
                case 8:
                    frameLength = BitConverter.IsLittleEndian ? buffer.GetLong(offset) : buffer.GetLongLE(offset);
                    break;
                default:
                    throw new Exception("unsupported lengthFieldLength: " + this.lengthFieldLength + " (expected: 1, 2, 3, 4, or 8)");
            }
            return frameLength;
        }
    }
}
