using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Buffers
{
    //实现组合CompositeByteBuffer  增加空间，就是新增一个buff，实现零拷贝
    public class ReadOnlyCompositeByteBuffer
    {
        //get
        //read
        private List<ComponentEntry> Components = new List<ComponentEntry>();

        class ComponentEntry
        {
            public readonly IByteBuffer Buffer;
            public readonly int Length;
            public int Offset;
            public int EndOffset;

            public ComponentEntry(IByteBuffer buffer)
            {
                this.Buffer = buffer;
                this.Length = buffer.ReadableBytes;
            }



        }

        ReadOnlyCompositeByteBuffer AddComponent(IByteBuffer buffer)
        {

            int readableBytes = buffer.ReadableBytes;
            var c = new ComponentEntry(buffer.Slice());
            this.Components.Add(c);

            int count = this.Components.Count;
            if (count == 1) //第一个
            {
                c.Offset = 0;
                c.EndOffset = readableBytes;
            }
            else
            {
                ComponentEntry prev = this.Components[count - 2];
                c.Offset = prev.EndOffset;
                c.EndOffset = c.Offset + readableBytes;
            }

            return this;
        }

        /***********************get*****************/


    }

}
