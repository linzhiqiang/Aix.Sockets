using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using Aix.SocketCore.Codecs;
using Aix.SocketCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Common.Codecs
{
   

    /// <summary>
    /// 解析为Message
    /// </summary>
    public class MessageDecoder: MessageToMessageDecoder<IByteBuffer>
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer byteBuffer, List<object> output)
        {
            Message message = new Message();

            message.Reserved1 = byteBuffer.ReadByte();
            message.Reserved2 = byteBuffer.ReadByte();
            message.Reserved3 = byteBuffer.ReadByte();
            message.MessageType = (MessageType)byteBuffer.ReadByte();

            var bodyLength = byteBuffer.ReadInt();

            message.RequestId = byteBuffer.ReadInt();

            var routeLength = byteBuffer.ReadInt();
            if (routeLength > 0)
            {
                message.Route = System.Text.Encoding.UTF8.GetString(byteBuffer.ReadeBytes(byteBuffer.ReaderIndex, routeLength));
            }

            message.Data = byteBuffer.ReadeBytes();

            output.Add(message);
        }
    }
}
