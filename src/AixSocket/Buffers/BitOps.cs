using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AixSocket.Buffers
{
    public static class BitOps
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RightUShift(this int value, int bits) => unchecked((int)((uint)value >> bits));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RightUShift(this long value, int bits) => unchecked((long)((ulong)value >> bits));
    }
}
