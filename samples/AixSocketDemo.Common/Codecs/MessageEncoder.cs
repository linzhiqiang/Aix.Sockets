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
        protected override void Encode(IChannelHandlerContext context, Message message, IByteBuffer output)
        {
            output.WriteByte(message.Reserved1);
            output.WriteByte(message.Reserved2);
            output.WriteByte(message.Reserved3);
            output.WriteByte((byte)message.MessageType);

            var bodyLength = 0;
            output.WriteInt(bodyLength);
            output.WriteInt(message.RequestId);

            var routeDatas = System.Text.Encoding.UTF8.GetBytes(message.Route ?? string.Empty);
            output.WriteInt(routeDatas.Length);
            if (routeDatas.Length > 0)
            {
                output.WriteBytes(routeDatas);
            }

            if (message.Data != null && message.Data.Length > 0)
            {
                output.WriteBytes(message.Data);
            }

            output.SetInt(4, output.ReadableBytes - 8);

        }
    }
}
