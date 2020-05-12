using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Common.Codecs
{
    //1个byte，取值如下。
    //01: 客户端到服务器的握手请求以及服务器到客户端的握手响应
    //02: 客户端到服务器的握手ack
    //03: 心跳包
    //04: 认证服务器主动断开连接通知
    //05: 认证响应
    //11: 数据包的request(需要返回值的)
    //12: 数据包的request(不需要返回值的)
    //13: 数据包的request(对应11的返回值)
    //44: 数据包的推送(服务端向客户端的推送)

    /// <summary>
    /// 通信消息类型
    /// </summary>
    public enum MessageType : byte
    {
        Handshake = 1,
        HandshakeAck = 2,
        Heartbeat = 3,
        Auth = 4,//认证
        AuthRes = 5,//认证响应

        Request = 11, //客户端请求 需要服务端响应的
        Notify = 12,//客户端请求，不需要服务端响应
        Response = 13,//服务端响应的
        Push = 14// 服务端主动推送消息给客户端的
    }
}
