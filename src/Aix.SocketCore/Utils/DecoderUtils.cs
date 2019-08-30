using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Aix.SocketCore.Utils
{
     public class DecoderUtils
    {

        private static bool IsLittleEndian = BitConverter.IsLittleEndian;


        public static Int16 DecodeInt16(byte[] data)
        {
            Int16 netV = BitConverter.ToInt16(data, 0);
            return IPAddress.NetworkToHostOrder(netV);
        }

        public static UInt16 DecodeUInt16(byte[] data)
        {
            UInt16 netV = BitConverter.ToUInt16(data, 0);
            return IsLittleEndian ? Endian.SwapUInt16(netV) : netV;
        }

        public static int DecodeInt32(byte[] data)
        {
            int netV = BitConverter.ToInt32(data, 0);
            return IPAddress.NetworkToHostOrder(netV);
        }

        public static UInt32 DecodeUInt32(byte[] data)
        {
            UInt32 netV = BitConverter.ToUInt32(data, 0);
            return IsLittleEndian ? Endian.SwapUInt32(netV) : netV;
        }

        public static Int64 DecodeInt64(byte[] data)
        {
            Int64 netV = BitConverter.ToInt64(data, 0);
            return IPAddress.NetworkToHostOrder(netV);
        }

        public static UInt64 DecodeUInt64(byte[] data)
        {
            UInt64 netV = BitConverter.ToUInt64(data, 0);
            return IsLittleEndian ? Endian.SwapUInt64(netV) : netV;
        }


    }
}
