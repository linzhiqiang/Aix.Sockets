using AixSocket.Buffers;
using AixSocket.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Common.Codecs
{
    public class MessageFrame
    {
        public const int HEADER_LENGTH = 16;

        private int Status = 0;//0 正在解析head，1 正在解析body
        private int bodyLength = 0;

        private IByteBuffer cumulationBuff = null;
        public List<Message> AddAndGetMessage1(byte[] data, int offset, int length)
        {
            List<Message> list = new List<Message>();

            if (cumulationBuff == null)
            {
                cumulationBuff = new ByteBuffer(256, 10 * 1024 * 1024);
            }
            cumulationBuff.WriteBytes(data, offset, length);
            bool isParse = true;

            while (isParse)
            {
                isParse = false;
                if (Status == 0 && cumulationBuff.ReadableBytes >= HEADER_LENGTH)//解析head
                {

                    bodyLength = cumulationBuff.GetInt(cumulationBuff.ReaderIndex + 4);
                    Status = 1;

                    //if (bodyLength > 最大值)
                    //{
                    //    throw new Exception("包长超过最大值");
                    //}
                }
                if (Status == 1 && cumulationBuff.ReadableBytes >= bodyLength)  //解析body
                {
                    byte[] frameData = cumulationBuff.ReadeBytes(cumulationBuff.ReaderIndex, bodyLength);
                    Status = 0;

                    list.Add(Decode(frameData));
                    isParse = true;

                }
            }//while

            if (list.Count > 0)
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

            return list;

        }

        public List<Message> AddAndGetMessage(IByteBuffer byteBuf)
        {
            List<Message> list = new List<Message>();

            if (cumulationBuff == null)
            {
                cumulationBuff = byteBuf;
            }
            else
            {
                cumulationBuff.WriteBytes(byteBuf.ReadeBytes());
            }
            
            bool isParse = true;

            while (isParse)
            {
                isParse = false;
                if (Status == 0 && cumulationBuff.ReadableBytes >= HEADER_LENGTH)//解析head
                {

                    bodyLength = cumulationBuff.GetInt(cumulationBuff.ReaderIndex + 4);
                    Status = 1;

                    if (bodyLength > cumulationBuff.MaxCapacity)
                    {
                        throw new Exception("包长超过最大值");
                    }
                }
                if (Status == 1 && cumulationBuff.ReadableBytes >= bodyLength)  //解析body
                {
                    byte[] frameData = cumulationBuff.ReadeBytes(cumulationBuff.ReaderIndex, bodyLength);
                    Status = 0;

                    list.Add(Decode(frameData));
                    isParse = true;

                }
            }//while

            if (list.Count > 0)
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

            return list;

        }

        public static Message Decode(byte[] data)
        {
            Message message = new Message();
            int offset = 0;
            message.Reserved1 = data[0];
            message.Reserved2 = data[1];
            message.Reserved3 = data[2];
            message.MessageType = (MessageType)data[3];
            offset += 4;

            var bodyLength = DecoderUtils.DecodeInt32(GetBytes(data, ref offset, 4));

            message.RequestId = DecoderUtils.DecodeInt32(GetBytes(data, ref offset, 4));

            var routeLength = DecoderUtils.DecodeInt32(GetBytes(data, ref offset, 4));
            if (routeLength > 0)
            {
                message.Route = System.Text.Encoding.UTF8.GetString(GetBytes(data, ref offset, routeLength));
            }
            message.Data = GetBytes(data, ref offset, data.Length - offset);

            return message;
        }

        private static byte[] GetBytes(byte[] source, ref int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(source, offset, result, 0, length);
            offset += length;
            return result;
        }
    }
}
