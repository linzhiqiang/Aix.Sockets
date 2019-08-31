using Aix.SocketCore.Channels;
using Aix.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AixSocketDemo.Common.Codecs
{
    public class MessageEncoderOld : ChannelHandlerAdapter
    {
        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            Message msg = message as Message;
            if (msg != null)
            {
                var data = Encode(msg);
                return context.WriteAsync(data);
            }
            else
            {
                return context.WriteAsync(message);
            }

            //return base.WriteAsync(context, message);
        }

        public static byte[] Encode(Message msg)
        {
            var first4Bytes = new byte[4];
            first4Bytes[0] = msg.Reserved1;
            first4Bytes[1] = msg.Reserved2;
            first4Bytes[2] = msg.Reserved3;
            first4Bytes[3] = (byte)msg.MessageType;

            var bodyLength = 0;//最后再赋值

            var requestId = msg.RequestId;

            var routeDatas = System.Text.Encoding.UTF8.GetBytes(msg.Route ?? string.Empty);

            //8(固定header长度)+4(requestId长度)+4(routeDatas长度)+routeDatas.Length+msg.Data.Length
            bodyLength = 4 + 4 + routeDatas.Length + msg.Data.Length;

            var datas = new byte[bodyLength + 8];

            int offset = 0;
            Write(first4Bytes, datas, ref offset);

            Write(EncoderUtils.EncodeInt32(bodyLength), datas, ref offset);
            Write(EncoderUtils.EncodeInt32(msg.RequestId), datas, ref offset);
            Write(EncoderUtils.EncodeInt32(routeDatas.Length), datas, ref offset);
            Write(routeDatas, datas, ref offset);
            Write(msg.Data, datas, ref offset);

            return datas;
        }

        private static void Write(byte[] source, byte[] dest, ref int destOffset)
        {
            System.Array.Copy(source, 0, dest, destOffset, source.Length);
            destOffset += source.Length;
        }
    }
}
