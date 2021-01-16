using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Buffers
{
    /// <summary>
    /// 默认都是大端读写 LE是小端读写
    /// </summary>
  public class ByteBuffer : IByteBuffer
    {
        const int CalculateThreshold = 1048576 * 4; // 4 MiB page
        byte[] _array;

        public ByteBuffer(int initialCapacity, int maxCapacit)
            : this(new byte[initialCapacity], 0, 0, maxCapacit)
        {

        }
        public ByteBuffer(byte[] initialArray, int readerIndex, int writerIndex,
            int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            SetArray(initialArray);
            SetIndex(readerIndex, writerIndex);
        }
        ////////////////////////////

        #region 基本方法

        public byte[] Array { get { return this._array; } }

        public int Capacity { get { return this._array.Length; } }
        public int MaxCapacity { get; }

        //////////////////////////

        public int ReaderIndex { get; protected set; }

        public int WriterIndex { get; protected set; }

        public IByteBuffer SetReaderIndex(int readerIndex)
        {
            if (readerIndex < 0 || readerIndex > WriterIndex)
                throw new IndexOutOfRangeException(
                    string.Format("ReaderIndex: {0} (expected: 0 <= readerIndex <= writerIndex({1})", readerIndex,
                        WriterIndex));
            ReaderIndex = readerIndex;
            return this;
        }
        public IByteBuffer SetWriterIndex(int writerIndex)
        {
            if (writerIndex < ReaderIndex || writerIndex > Capacity)
                throw new IndexOutOfRangeException(
                    string.Format("WriterIndex: {0} (expected: 0 <= readerIndex({1}) <= writerIndex <= capacity ({2})",
                        writerIndex, ReaderIndex, Capacity));

            WriterIndex = writerIndex;
            return this;
        }

        public IByteBuffer SetIndex(int readerIndex, int writerIndex)
        {
            if (readerIndex < 0 || readerIndex > writerIndex || writerIndex > Capacity)
                throw new IndexOutOfRangeException(
                    string.Format(
                        "ReaderIndex: {0}, WriterIndex: {1} (expected: 0 <= readerIndex <= writerIndex <= capacity ({2})",
                        readerIndex, writerIndex, Capacity));

            ReaderIndex = readerIndex;
            WriterIndex = writerIndex;
            return this;
        }

        ////////////////////////////////////

        public int ReadableBytes { get { return this.WriterIndex - this.ReaderIndex; } }

        public int WritableBytes { get { return this.Capacity - this.WriterIndex; } }

        public int MaxWritableBytes { get { return this.MaxCapacity - this.WriterIndex; } }

        public bool IsReadable()
        {
            return this.WriterIndex > this.ReaderIndex;
        }
        public bool IsReadable(int size)
        {
            return this.WriterIndex - this.ReaderIndex >= size;
        }

        public bool IsWritable()
        {
            return this.Capacity > this.WriterIndex;
        }
        public bool IsWritable(int size)
        {
            return this.Capacity - this.WriterIndex >= size;
        }
        /////////////////////////
        public IByteBuffer Clear()
        {
            this.ReaderIndex = this.WriterIndex = 0;
            return this;
        }

        public IByteBuffer DiscardReadBytes()
        {
            if (this.ReaderIndex == 0)
            {
                return this;
            }

            if (this.ReaderIndex != this.WriterIndex)
            {
                this.SetBytes(0, this.Array, this.ReaderIndex, this.WriterIndex - this.ReaderIndex);
                this.WriterIndex -= this.ReaderIndex;
                //this.AdjustMarkers(this.readerIndex);
                this.ReaderIndex = 0;
            }
            else
            {
                // this.AdjustMarkers(this.readerIndex);
                this.WriterIndex = this.ReaderIndex = 0;
            }

            return this;
        }
        public IByteBuffer DiscardSomeReadBytes()
        {
            if (this.ReaderIndex == 0)
            {
                return this;
            }

            if (this.ReaderIndex == this.WriterIndex)
            {
                //this.AdjustMarkers(this.ReaderIndex);
                this.WriterIndex = this.ReaderIndex = 0;
                return this;
            }

            if (this.ReaderIndex >= this.Capacity.RightUShift(1))
            {
                this.SetBytes(0, this.Array, this.ReaderIndex, this.WriterIndex - this.ReaderIndex);
                this.WriterIndex -= this.ReaderIndex;
                //this.AdjustMarkers(this.readerIndex);
                this.ReaderIndex = 0;
            }

            return this;
        }

        public IByteBuffer EnsureWritable(int minWritableBytes)
        {
            if (minWritableBytes <= this.WritableBytes)
            {
                return this;
            }

            if (minWritableBytes > this.MaxCapacity - this.WriterIndex)
            {
                throw new IndexOutOfRangeException($"writerIndex({this.WriterIndex}) + minWritableBytes({minWritableBytes}) exceeds maxCapacity({this.MaxCapacity}): {this}");
            }

            // Normalize the current capacity to the power of 2.
            int newCapacity = CalculateNewCapacity(this.WriterIndex + minWritableBytes, this.MaxCapacity);

            // Adjust to the new capacity.
            this.AdjustCapacity(newCapacity);
            return this;
        }


        public IByteBuffer AdjustCapacity(int newCapacity)
        {
            this.CheckNewCapacity(newCapacity);

            int oldCapacity = this.Array.Length;
            byte[] oldArray = this.Array;
            if (newCapacity > oldCapacity)
            {
                byte[] newArray = new byte[newCapacity];
                System.Array.Copy(this.Array, 0, newArray, 0, this.Array.Length);

                this.SetArray(newArray);
            }
            else if (newCapacity < oldCapacity)
            {
                byte[] newArray = new byte[newCapacity];
                int readerIndex = this.ReaderIndex;
                if (readerIndex < newCapacity)
                {
                    int writerIndex = this.WriterIndex;
                    if (writerIndex > newCapacity)
                    {
                        this.SetWriterIndex(writerIndex = newCapacity);
                    }

                    System.Array.Copy(this.Array, readerIndex, newArray, 0, writerIndex - readerIndex);
                }
                else
                {
                    this.SetIndex(newCapacity, newCapacity);
                }

                this.SetArray(newArray);
            }
            return this;
        }

        #endregion

        #region get

        public byte GetByte(int index)
        {
            this.CheckIndex(index);
            return HeapByteBufferUtils.GetByte(this.Array, index);
        }

        public bool GetBoolean(int index)
        {
            return this.GetByte(index) != 0;
        }

        public short GetShort(int index)
        {
            this.CheckIndex(index, 2);
            return HeapByteBufferUtils.GetShort(this.Array, index);
        }

        public short GetShortLE(int index)
        {
            this.CheckIndex(index, 2);
            return HeapByteBufferUtils.GetShortLE(this.Array, index);
        }

        public ushort GetUnsignedShort(int index)
        {
            unchecked
            {
                return (ushort)this.GetShort(index);
            }
        }

        public ushort GetUnsignedShortLE(int index)
        {
            unchecked
            {
                return (ushort)this.GetShortLE(index);
            }
        }

        public int GetUnsignedMedium(int index)
        {
            this.CheckIndex(index, 3);
            return HeapByteBufferUtils.GetUnsignedMedium(this.Array, index);
        }


        public int GetUnsignedMediumLE(int index)
        {
            this.CheckIndex(index, 3);
            return HeapByteBufferUtils.GetUnsignedMediumLE(this.Array, index);
        }

        public int GetMedium(int index)
        {
            uint value = (uint)this.GetUnsignedMedium(index);
            if ((value & 0x800000) != 0)
            {
                value |= 0xff000000;
            }

            return (int)value;
        }

        public int GetMediumLE(int index)
        {
            uint value = (uint)this.GetUnsignedMediumLE(index);
            if ((value & 0x800000) != 0)
            {
                value |= 0xff000000;
            }

            return (int)value;
        }

        public int GetInt(int index)
        {
            this.CheckIndex(index, 4);
            return HeapByteBufferUtils.GetInt(this.Array, index);
        }


        public int GetIntLE(int index)
        {
            this.CheckIndex(index, 4);
            return HeapByteBufferUtils.GetIntLE(this.Array, index);
        }

        public uint GetUnsignedInt(int index)
        {
            unchecked
            {
                return (uint)(this.GetInt(index));
            }
        }

        public uint GetUnsignedIntLE(int index)
        {
            unchecked
            {
                return (uint)this.GetIntLE(index);
            }
        }

        public long GetLong(int index)
        {
            this.CheckIndex(index, 8);
            return HeapByteBufferUtils.GetLong(this.Array, index);
        }


        public long GetLongLE(int index)
        {
            this.CheckIndex(index, 8);
            return HeapByteBufferUtils.GetLongLE(this.Array, index);
        }

        public char GetChar(int index)
        {
            return Convert.ToChar(this.GetShort(index));
        }

        public float GetFloat(int index) => ByteBufferUtils.Int32BitsToSingle(this.GetInt(index));

        public float GetFloatLE(int index) => ByteBufferUtils.Int32BitsToSingle(this.GetIntLE(index));

        public double GetDouble(int index)
        {
            return BitConverter.Int64BitsToDouble(this.GetLong(index));
        }

        public double GetDoubleLE(int index)
        {
            return BitConverter.Int64BitsToDouble(this.GetLongLE(index));
        }


        #endregion

        #region set

        public IByteBuffer SetByte(int index, int value)
        {
            HeapByteBufferUtils.SetByte(this.Array, index, value);
            return this;
        }

        public IByteBuffer SetShort(int index, int value)
        {
            HeapByteBufferUtils.SetShort(this.Array, index, value);
            return this;
        }

        public IByteBuffer SetShortLE(int index, int value)
        {
            HeapByteBufferUtils.SetShortLE(this.Array, index, value);
            return this;
        }

        public IByteBuffer SetMedium(int index, int value)
        {
            HeapByteBufferUtils.SetMedium(this.Array, index, value);
            return this;
        }

        public IByteBuffer SetMediumLE(int index, int value)
        {
            HeapByteBufferUtils.SetMediumLE(this.Array, index, value);
            return this;
        }

        public IByteBuffer SetInt(int index, int value)
        {
            HeapByteBufferUtils.SetInt(this.Array, index, value);
            return this;
        }

        public IByteBuffer SetIntLE(int index, int value)
        {
            HeapByteBufferUtils.SetIntLE(this.Array, index, value);
            return this;
        }

        public IByteBuffer SetLong(int index, long value)
        {
            HeapByteBufferUtils.SetLong(this.Array, index, value);
            return this;
        }

        public IByteBuffer SetLongLE(int index, long value)
        {
            HeapByteBufferUtils.SetLongLE(this.Array, index, value);
            return this;
        }

        public IByteBuffer SetFloat(int index, float value)
        {
            this.SetInt(index, ByteBufferUtils.SingleToInt32Bits(value));
            return this;
        }

        public IByteBuffer SetFloatLE(int index, float value)
        {
            return this.SetIntLE(index, ByteBufferUtils.SingleToInt32Bits(value));
        }

        public IByteBuffer SetDouble(int index, double value)
        {
            this.SetLong(index, BitConverter.DoubleToInt64Bits(value));
            return this;
        }

        public IByteBuffer SetDoubleLE(int index, double value)
        {
            return this.SetLongLE(index, BitConverter.DoubleToInt64Bits(value));
        }

        public IByteBuffer SetBytes(int index, byte[] src)
        {
            SetBytes(index, src, 0, src.Length);
            return this;
        }

        public IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Length);
            System.Array.Copy(src, srcIndex, this._array, index, length);
            return this;
        }


        #endregion

        #region read

        public byte ReadByte()
        {
            this.CheckReadableBytes0(1);
            int i = this.ReaderIndex;
            byte b = this.GetByte(i);
            this.ReaderIndex = i + 1;
            return b;
        }

        public byte[] ReadeBytes()
        {
            return ReadeBytes(this.ReaderIndex, this.ReadableBytes);
        }
        public byte[] ReadeBytes(int offset, int legnth)
        {
            this.CheckReadableBytes0(legnth);
            byte[] newArray = new byte[legnth];

            System.Array.Copy(this.Array, offset, newArray, 0, legnth);
            this.ReaderIndex += legnth;
            return newArray;
        }

        public bool ReadBoolean()
        {
            return this.ReadByte() != 0;
        }

        public short ReadShort()
        {
            this.CheckReadableBytes0(2);
            short v = this.GetShort(this.ReaderIndex);
            this.ReaderIndex += 2;
            return v;
        }

        public short ReadShortLE()
        {
            this.CheckReadableBytes0(2);
            short v = this.GetShortLE(this.ReaderIndex);
            this.ReaderIndex += 2;
            return v;
        }

        public ushort ReadUnsignedShort()
        {
            unchecked
            {
                return (ushort)(this.ReadShort());
            }
        }

        public ushort ReadUnsignedShortLE()
        {
            unchecked
            {
                return (ushort)this.ReadShortLE();
            }
        }

        public int ReadMedium()
        {
            uint value = (uint)this.ReadUnsignedMedium();
            if ((value & 0x800000) != 0)
            {
                value |= 0xff000000;
            }

            return (int)value;
        }

        public int ReadMediumLE()
        {
            uint value = (uint)this.ReadUnsignedMediumLE();
            if ((value & 0x800000) != 0)
            {
                value |= 0xff000000;
            }

            return (int)value;
        }

        public int ReadUnsignedMedium()
        {
            this.CheckReadableBytes0(3);
            int v = this.GetUnsignedMedium(this.ReaderIndex);
            this.ReaderIndex += 3;
            return v;
        }

        public int ReadUnsignedMediumLE()
        {
            this.CheckReadableBytes0(3);
            int v = this.GetUnsignedMediumLE(this.ReaderIndex);
            this.ReaderIndex += 3;
            return v;
        }

        public int ReadInt()
        {
            this.CheckReadableBytes0(4);
            int v = this.GetInt(this.ReaderIndex);
            this.ReaderIndex += 4;
            return v;
        }

        public int ReadIntLE()
        {
            this.CheckReadableBytes0(4);
            int v = this.GetIntLE(this.ReaderIndex);
            this.ReaderIndex += 4;
            return v;
        }

        public uint ReadUnsignedInt()
        {
            unchecked
            {
                return (uint)(this.ReadInt());
            }
        }

        public uint ReadUnsignedIntLE()
        {
            unchecked
            {
                return (uint)this.ReadIntLE();
            }
        }

        public long ReadLong()
        {
            this.CheckReadableBytes0(8);
            long v = this.GetLong(this.ReaderIndex);
            this.ReaderIndex += 8;
            return v;
        }

        public long ReadLongLE()
        {
            this.CheckReadableBytes0(8);
            long v = this.GetLongLE(this.ReaderIndex);
            this.ReaderIndex += 8;
            return v;
        }

        public char ReadChar()
        {
            return (char)this.ReadShort();
        }

        public float ReadFloat() => ByteBufferUtils.Int32BitsToSingle(this.ReadInt());

        public float ReadFloatLE() => ByteBufferUtils.Int32BitsToSingle(this.ReadIntLE());

        public double ReadDouble() => BitConverter.Int64BitsToDouble(this.ReadLong());

        public double ReadDoubleLE() => BitConverter.Int64BitsToDouble(this.ReadLongLE());

        public IByteBuffer SkipBytes(int length)
        {
            this.CheckReadableBytes(length);
            this.ReaderIndex += length;
            return this;
        }
        #endregion

        #region writer

        public IByteBuffer WriteByte(int value)
        {
            this.EnsureWritable(1);
            this.SetByte(this.WriterIndex++, value);
            return this;
        }

        public IByteBuffer WriteBoolean(bool value)
        {
            this.WriteByte(value ? 1 : 0);
            return this;
        }

        public IByteBuffer WriteShort(int value)
        {
            this.EnsureWritable(2);
            this.SetShort(this.WriterIndex, value);
            this.WriterIndex += 2;
            return this;
        }

        public IByteBuffer WriteShortLE(int value)
        {
            this.EnsureWritable(2);
            this.SetShortLE(this.WriterIndex, value);
            this.WriterIndex += 2;
            return this;
        }

        public IByteBuffer WriteUnsignedShort(ushort value)
        {
            unchecked
            {
                return this.WriteShort((short)value);
            }
        }

        public IByteBuffer WriteUnsignedShortLE(ushort value)
        {
            unchecked
            {
                return this.WriteShortLE((short)value);
            }
        }

        public IByteBuffer WriteMedium(int value)
        {
            this.EnsureWritable(3);
            this.SetMedium(this.WriterIndex, value);
            this.WriterIndex += 3;
            return this;
        }

        public IByteBuffer WriteMediumLE(int value)
        {
            this.EnsureWritable(3);
            this.SetMediumLE(this.WriterIndex, value);
            this.WriterIndex += 3;
            return this;
        }

        public IByteBuffer WriteInt(int value)
        {
            this.EnsureWritable(4);
            this.SetInt(this.WriterIndex, value);
            this.WriterIndex += 4;
            return this;
        }

        public IByteBuffer WriteIntLE(int value)
        {
            this.EnsureWritable(4);
            this.SetIntLE(this.WriterIndex, value);
            this.WriterIndex += 4;
            return this;
        }

        public IByteBuffer WriteLong(long value)
        {
            this.EnsureWritable(8);
            this.SetLong(this.WriterIndex, value);
            this.WriterIndex += 8;
            return this;
        }

        public IByteBuffer WriteLongLE(long value)
        {
            this.EnsureWritable(8);
            this.SetLongLE(this.WriterIndex, value);
            this.WriterIndex += 8;
            return this;
        }

        public IByteBuffer WriteChar(char value)
        {
            this.WriteShort(value);
            return this;
        }
        public IByteBuffer WriteFloat(float value)
        {
            this.WriteInt(ByteBufferUtils.SingleToInt32Bits(value));
            return this;
        }

        public IByteBuffer WriteFloatLE(float value) => this.WriteIntLE(ByteBufferUtils.SingleToInt32Bits(value));

        public IByteBuffer WriteDouble(double value)
        {
            this.WriteLong(BitConverter.DoubleToInt64Bits(value));
            return this;
        }

        public IByteBuffer WriteDoubleLE(double value) => this.WriteLongLE(BitConverter.DoubleToInt64Bits(value));

        public IByteBuffer WriteBytes(byte[] src, int srcIndex, int length)
        {
            this.EnsureWritable(length);
            this.SetBytes(this.WriterIndex, src, srcIndex, length);
            this.WriterIndex += length;
            return this;
        }

        public IByteBuffer WriteBytes(byte[] src)
        {
            this.WriteBytes(src, 0, src.Length);
            return this;
        }

        #endregion

        /// <summary>
        /// 返回只读buffer
        /// </summary>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public IByteBuffer Slice(int index, int length)
        {
            return new ByteBuffer(this.Array, index, index + length, this.MaxCapacity);
        }

        #region private

        protected void SetArray(byte[] initialBuffer)
        {
            _array = initialBuffer;
        }

        protected void CheckSrcIndex(int index, int length, int srcIndex, int srcCapacity)
        {
            CheckIndex(index, length);
            if (srcIndex < 0 || srcIndex > srcCapacity - length)
            {
                throw new IndexOutOfRangeException(string.Format(
                    "srcIndex: {0}, length: {1} (expected: range(0, {2}))", srcIndex, length, srcCapacity));
            }
        }

        protected void CheckIndex(int index, int fieldLength)
        {
            if (fieldLength < 0)
            {
                throw new IndexOutOfRangeException(string.Format("length: {0} (expected: >= 0)", fieldLength));
            }

            if (index < 0 || index > Capacity - fieldLength)
            {
                throw new IndexOutOfRangeException(string.Format("index: {0}, length: {1} (expected: range(0, {2})",
                    index, fieldLength, Capacity));
            }
        }
        protected void CheckIndex(int index)
        {
            if (index < 0 || index >= Capacity)
            {
                throw new IndexOutOfRangeException(string.Format("index: {0} (expected: range(0, {1})", index, Capacity));
            }
        }

        protected void CheckNewCapacity(int newCapacity)
        {
            if (newCapacity < 0 || newCapacity > this.MaxCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(newCapacity), $"newCapacity: {newCapacity} (expected: 0-{this.MaxCapacity})");
            }
        }
        protected void CheckReadableBytes(int minimumReadableBytes)
        {
            if (minimumReadableBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumReadableBytes), $"minimumReadableBytes: {minimumReadableBytes} (expected: >= 0)");
            }

            this.CheckReadableBytes0(minimumReadableBytes);
        }
        protected void CheckReadableBytes0(int minimumReadableBytes)
        {
            if (this.ReaderIndex > this.WriterIndex - minimumReadableBytes)
            {
                throw new IndexOutOfRangeException($"readerIndex({this.ReaderIndex}) + length({minimumReadableBytes}) exceeds writerIndex({this.WriterIndex}): {this}");
            }
        }

        public int CalculateNewCapacity(int minNewCapacity, int maxCapacity)
        {
            if (minNewCapacity < 0)
            {
                throw new ArgumentOutOfRangeException($"minNewCapacity: {minNewCapacity} (expected: 0+)");
            }
            if (minNewCapacity > maxCapacity)
            {
                throw new ArgumentOutOfRangeException($"minNewCapacity: {minNewCapacity} (expected: not greater than maxCapacity({maxCapacity})");
            }

            const int Threshold = CalculateThreshold; // 4 MiB page
            if (minNewCapacity == CalculateThreshold)
            {
                return Threshold;
            }

            int newCapacity;
            // If over threshold, do not double but just increase by threshold.
            if (minNewCapacity > Threshold)
            {
                newCapacity = minNewCapacity / Threshold * Threshold;
                if (newCapacity > maxCapacity - Threshold)
                {
                    newCapacity = maxCapacity;
                }
                else
                {
                    newCapacity += Threshold;
                }

                return newCapacity;
            }

            // Not over threshold. Double up to 4 MiB, starting from 64.
            newCapacity = 64;
            while (newCapacity < minNewCapacity)
            {
                newCapacity <<= 1;
            }

            return Math.Min(newCapacity, maxCapacity);
        }

        #endregion
    }
}
