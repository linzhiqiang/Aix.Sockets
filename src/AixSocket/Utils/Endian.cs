using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocket.Utils
{
    public class Endian
    {

        public static UInt16 SwapUInt16(UInt16 v)
        {
            return (UInt16)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }
        public static UInt32 SwapUInt32(UInt32 v)
        {
            return (UInt32)((v & 0x000000FFU) << 24 | (v & 0x0000FF00U) << 8 |
                    (v & 0x00FF0000U) >> 8 | (v & 0xFF000000U) >> 24);
        }

        public static UInt64 SwapUInt64(UInt64 v)
        {
            return (UInt64)((v & 0x00000000000000FFUL) << 56 | (v & 0x000000000000FF00UL) << 40 |
                        (v & 0x0000000000FF0000UL) << 24 | (v & 0x00000000FF000000UL) << 8 |
                        (v & 0x000000FF00000000UL) >> 8 | (v & 0x0000FF0000000000UL) >> 24 |
                        (v & 0x00FF000000000000UL) >> 40 | (v & 0xFF00000000000000UL) >> 56);
        }

    }
}
