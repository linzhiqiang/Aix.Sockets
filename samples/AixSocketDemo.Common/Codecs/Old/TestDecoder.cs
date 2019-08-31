using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using Aix.SocketCore.Codecs;
using Aix.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Common.Codecs
{
    public class TestDecoder : ByteToMessageDecoder
    {
        public const int HEADER_LENGTH = 8;

        private int Status = 0;//0 正在解析head，1 正在解析body
        private int bodyLength = 0;

        protected  override void Decode(IChannelHandlerContext context, IByteBuffer cumulationBuff, List<object> output)
        {
            bool isParse = true;

            while (isParse)
            {
                isParse = false;
                if (Status == 0 && cumulationBuff.ReadableBytes >= HEADER_LENGTH)//解析head
                {

                    bodyLength = cumulationBuff.GetInt(cumulationBuff.ReaderIndex + 4);
                    Status = 1;

                    if (bodyLength > cumulationBuff.MaxCapacity)
                    {
                        throw new Exception("包长超过最大值");
                    }
                }
                if (Status == 1 && cumulationBuff.ReadableBytes >= bodyLength + HEADER_LENGTH)  //解析body
                {
                    byte[] frameData = cumulationBuff.ReadeBytes(cumulationBuff.ReaderIndex, bodyLength + HEADER_LENGTH);
                    Status = 0;

                    output.Add(Decode(frameData));
                    isParse = true;

                }
            }//while
        }

        private static Message Decode(byte[] data)
        {
            Message message = new Message();
            int offset = 0;
            message.Reserved1 = data[0];
            message.Reserved2 = data[1];
            message.Reserved3 = data[2];
            message.MessageType = (MessageType)data[3];
            offset += 4;

            var bodyLength = DecoderUtils.DecodeInt32(GetBytes(data, ref offset, 4));

            message.RequestId = DecoderUtils.DecodeInt32(GetBytes(data, ref offset, 4));

            var routeLength = DecoderUtils.DecodeInt32(GetBytes(data, ref offset, 4));
            if (routeLength > 0)
            {
                message.Route = System.Text.Encoding.UTF8.GetString(GetBytes(data, ref offset, routeLength));
            }
            message.Data = GetBytes(data, ref offset, data.Length - offset);

            return message;
        }

        private static byte[] GetBytes(byte[] source, ref int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(source, offset, result, 0, length);
            offset += length;
            return result;
        }

    }
}
