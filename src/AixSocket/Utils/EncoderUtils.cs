using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AixSocket.Utils
{
    public class EncoderUtils
    {
        private static bool IsLittleEndian = BitConverter.IsLittleEndian;

        public static byte[] EncodeInt32(int v)
        {
            int netV = IPAddress.HostToNetworkOrder(v);
            return BitConverter.GetBytes(netV);
        }

        public static byte[] EncodeUInt32(uint v)
        {
            return BitConverter.GetBytes(IsLittleEndian ? Endian.SwapUInt32(v) : v);
        }

        public static byte[] EncodeInt16(Int16 v)
        {
            Int16 netV = IPAddress.HostToNetworkOrder(v);
            return BitConverter.GetBytes(netV);
        }

        public static byte[] EncodeUInt16(ushort v)
        {
            return BitConverter.GetBytes(IsLittleEndian ? Endian.SwapUInt16(v) : v);
        }

        public static byte[] EncodeInt64(Int64 v)
        {
            Int64 netV = IPAddress.HostToNetworkOrder(v);
            return BitConverter.GetBytes(netV);
        }

        public static byte[] EncodeUInt64(UInt64 v)
        {
            return BitConverter.GetBytes(IsLittleEndian ? Endian.SwapUInt64(v) : v);
        }

    }
}
