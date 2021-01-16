using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using Aix.SocketCore.Codecs;
using Aix.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AixSocketDemo.Common.Codecs
{


    public class MessageEncoder : MessageToByteEncoder<Message>
    {
        private static byte[] EmptyBytes = new byte[0];
        private static int FixHeaderLength = 8;
        protected override void Encode(IChannelHandlerContext context, Message message, IByteBuffer output)
        {
            //这里都是大端读写
            //前四个字节
            output.WriteByte(message.Reserved1);
            output.WriteByte(message.Reserved2);
            output.WriteByte(message.Reserved3);
            output.WriteByte((byte)message.MessageType);

            //length字段  先填充0，最后再填充真实的
            var bodyLength = 0;
            output.WriteInt(bodyLength);

            if (message.MessageType == MessageType.Heartbeat)
            {
                return;
            }

            //RequestId
            output.WriteInt(message.RequestId);

            //Route
            var routeDatas = message.Route != null ? Encoding.UTF8.GetBytes(message.Route) : EmptyBytes;
            output.WriteInt(routeDatas.Length);
            if (routeDatas.Length > 0)
            {
                output.WriteBytes(routeDatas);
            }

            //Data
            if (message.Data != null && message.Data.Length > 0)
            {
                output.WriteBytes(message.Data);
            }

            output.SetInt(4, output.ReadableBytes - FixHeaderLength);

        }
    }
}
