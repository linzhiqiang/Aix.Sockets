using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Buffers
{
    public interface IByteBuffer
    {
        #region 基本
       byte[] Array { get; }

        int Capacity { get; }

        int MaxCapacity { get; }



        int ReaderIndex { get; }

        int WriterIndex { get; }

        IByteBuffer SetWriterIndex(int writerIndex);

        IByteBuffer SetReaderIndex(int readerIndex);

        IByteBuffer SetIndex(int readerIndex, int writerIndex);

        //ArraySegment<byte> GetIoBuffer();

        int ReadableBytes { get; }

        int WritableBytes { get; }

        int MaxWritableBytes { get; }

        bool IsReadable();
        bool IsReadable(int size);

        bool IsWritable();
        bool IsWritable(int size);

        IByteBuffer Clear();

        IByteBuffer DiscardReadBytes();
        IByteBuffer DiscardSomeReadBytes();

        IByteBuffer EnsureWritable(int minWritableBytes);
        //int EnsureWritable(int minWritableBytes, bool force);

        IByteBuffer AdjustCapacity(int newCapacity);
        #endregion

        #region get

        byte GetByte(int index);

        bool GetBoolean(int index);

        /// <summary>
        /// 按照大端读取 网络字节序
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        short GetShort(int index);

        /// <summary>
        /// 按照小段读取
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        short GetShortLE(int index);

        ushort GetUnsignedShort(int index);

        ushort GetUnsignedShortLE(int index);

        int GetUnsignedMedium(int index);


        int GetUnsignedMediumLE(int index);
        int GetMedium(int index);

        int GetMediumLE(int index);
        int GetInt(int index);


        int GetIntLE(int index);

        uint GetUnsignedInt(int index);

        uint GetUnsignedIntLE(int index);

        long GetLong(int index);


        long GetLongLE(int index);

        char GetChar(int index);


        float GetFloat(int index);

        float GetFloatLE(int index);

        double GetDouble(int index);

        double GetDoubleLE(int index);



        #endregion

        #region set

        IByteBuffer SetByte(int index, int value);

        /// <summary>
        /// 按照大端写入 符合网络字节序
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IByteBuffer SetShort(int index, int value);

        IByteBuffer SetShortLE(int index, int value);

        IByteBuffer SetMedium(int index, int value);

        IByteBuffer SetMediumLE(int index, int value);

        IByteBuffer SetInt(int index, int value);

        IByteBuffer SetIntLE(int index, int value);
        IByteBuffer SetLong(int index, long value);

        IByteBuffer SetLongLE(int index, long value);

        IByteBuffer SetFloat(int index, float value);
        IByteBuffer SetFloatLE(int index, float value);

        IByteBuffer SetDouble(int index, double value);

        IByteBuffer SetDoubleLE(int index, double value);

        IByteBuffer SetBytes(int index, byte[] src);

        IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length);


        #endregion

        #region read

        byte ReadByte();

        byte[] ReadeBytes();
        byte[] ReadeBytes(int offset, int legnth);

        bool ReadBoolean();

        short ReadShort();

        short ReadShortLE();

        ushort ReadUnsignedShort();

        ushort ReadUnsignedShortLE();
        int ReadMedium();

        int ReadMediumLE();

        int ReadUnsignedMedium();

        int ReadUnsignedMediumLE();

        int ReadInt();

        int ReadIntLE();

        uint ReadUnsignedInt();

        uint ReadUnsignedIntLE();

        long ReadLong();

        long ReadLongLE();

        char ReadChar();

        float ReadFloat();

        float ReadFloatLE();

        double ReadDouble();
        double ReadDoubleLE();

        IByteBuffer SkipBytes(int length);
        #endregion

        #region writer

        IByteBuffer WriteByte(int value);

        IByteBuffer WriteBoolean(bool value);

        IByteBuffer WriteShort(int value);

        IByteBuffer WriteShortLE(int value);

        IByteBuffer WriteUnsignedShort(ushort value);

        IByteBuffer WriteUnsignedShortLE(ushort value);

        IByteBuffer WriteMedium(int value);

        IByteBuffer WriteMediumLE(int value);

        IByteBuffer WriteInt(int value);

        IByteBuffer WriteIntLE(int value);

        IByteBuffer WriteLong(long value);

        IByteBuffer WriteLongLE(long value);
        IByteBuffer WriteChar(char value);
        IByteBuffer WriteFloat(float value);

        IByteBuffer WriteFloatLE(float value);

        IByteBuffer WriteDouble(double value);

        IByteBuffer WriteDoubleLE(double value);

        IByteBuffer WriteBytes(byte[] src, int srcIndex, int length);

        IByteBuffer WriteBytes(byte[] src);
        #endregion

        /// <summary>
        /// 返回只读buffer
        /// </summary>
        /// <returns></returns>
        IByteBuffer Slice();

        /// <summary>
        /// 返回只读buffer
        /// </summary>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IByteBuffer Slice(int index, int length);

    }
}
