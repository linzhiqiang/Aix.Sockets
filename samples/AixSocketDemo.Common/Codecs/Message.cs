using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Common.Codecs
{
    /* 应用协议
0........8........16........24........32
1  |--预留1--|--预留2--|--预留3--|--type--| //预留,版本号,消息类型
2  |--------length--------------------------| //包长(总长度-8) 不包括header(8)
3  |--------RequestId---------------------| //请求号
4  |-------------route长度-----------------|
    |-- route body..............................................| //路由名称 route长度=4byte  route body = n byte
...
//以下是body
5|---------data..............................|//请求数据或响应数据
*/


    public class Message
    {
        /// <summary>
        /// 预留字段1
        /// </summary>
        public byte Reserved1 { get; set; }

        /// <summary>
        /// 预留字段2
        /// </summary>
        public byte Reserved2 { get; set; }

        /// <summary>
        /// 预留字段3
        /// </summary>
        public byte Reserved3 { get; set; }


        public MessageType MessageType { get; set; }

        public int RequestId { get; set; }

        public string Route { get; set; }


        private byte[] _data;
        public byte[] Data
        {
            get
            {
                if (_data == null) _data = new byte[0];
                return _data;
            }
            set { _data = value; }
        }
    }
}
