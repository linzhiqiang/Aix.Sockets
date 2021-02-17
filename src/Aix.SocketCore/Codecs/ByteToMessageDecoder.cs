using Aix.SocketCore.Buffers;
using Aix.SocketCore.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Codecs
{
    public abstract class ByteToMessageDecoder : ChannelHandlerAdapter
    {
        private IByteBuffer cumulationBuff = null; //这里可以优化为组合buff 就完全实现了零拷贝了
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
                cumulationBuff.WriteBytes(byteBuf.ReadeBytes()); //如果这里是组合buff，可以提高cpu性能了
            }

            var output = new List<object>();
            this.Decode(context, cumulationBuff, output);
            foreach (var item in output)
            {
                context.FireChannelRead(item); //把数据交给下一个handler
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
