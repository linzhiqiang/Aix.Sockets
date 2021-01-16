using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Codecs
{
    public class LengthFieldBasedFrameDecoder : ByteToMessageDecoder
    {
        /// <summary>
        /// 整体最大长度
        /// </summary>
        readonly int maxFrameLength;

        /// <summary>
        /// 长度字段的偏移位置（离首位的距离）
        /// </summary>
        readonly int lengthFieldOffset;

        /// <summary>
        /// 长度字段的长度（占几位 如int占4位）
        /// </summary>
        readonly int lengthFieldLength;

        /// <summary>
        /// 长度字段的结尾位置的偏移量 = lengthFieldOffset + lengthFieldLength;
        /// </summary>
        readonly int lengthFieldEndOffset;

        /// <summary>
        /// 长度修正值 在总长被定义为包含包头长度时，修正信息长度 ，就是长度域值是否包含头部或其他长度，实现中认为长度域值是后面具体内容的长度，如果包含其他，这里请修正即可
        /// 总长度  =长度域值(长度字段的值)+lengthAdjustment+lengthFieldEndOffset(前面头长度= lengthFieldOffset + lengthFieldLength)
        /// 例如：1 长度域值是整包的长度，包含头部 2:   在长度字段和具体内容中间 再增加一些其他信息
        /// </summary>
        readonly int lengthAdjustment;

        /// <summary>
        /// 跳过的字节数，根据需要可以跳过固定的字节数，让上层直接接受某些内容或具体内容，比如我们把头部跳过，设为0时，上层接受的buff是整个包的内容
        /// </summary>
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
