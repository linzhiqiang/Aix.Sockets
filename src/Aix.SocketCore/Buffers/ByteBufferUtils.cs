using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Buffers
{
    public static class ByteBufferUtils
    {
        public static unsafe float Int32BitsToSingle(int value)
        {
            return *(float*)(&value);
        }

        public static unsafe int SingleToInt32Bits(float value)
        {
            return *(int*)(&value);
        }
    }
}
