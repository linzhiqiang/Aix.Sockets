using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Common.Codecs
{
    public class MessageDecoderOld : ChannelHandlerAdapter
    {
        MessageFrame MessageFrame = new MessageFrame();
        //public override void ChannelRead(IChannelHandlerContext context, object message)
        //{
        //    byte[] data = message as byte[];
        //    if (data != null)
        //    {
        //      var list =   MessageFrame.AddAndGetMessage(data,0, data.Length);
        //        foreach (var item in list)
        //        {
        //            context.FireChannelRead(item);
        //        }
        //    }
        //    else
        //    {
        //        context.FireChannelRead(message);
        //        //base.ChannelRead(context, message);
        //    }

        //}

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            IByteBuffer byteBuf = message as IByteBuffer;
            if (byteBuf != null)
            {
                var list = MessageFrame.AddAndGetMessage(byteBuf);
                foreach (var item in list)
                {
                    context.FireChannelRead(item);
                }
            }
            else
            {
                context.FireChannelRead(message);
                //base.ChannelRead(context, message);
            }

        }

    }
}
