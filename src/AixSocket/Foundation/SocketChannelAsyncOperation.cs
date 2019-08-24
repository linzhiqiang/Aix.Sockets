using AixSocket.Channels;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace AixSocket.Foundation
{
    public class SocketChannelAsyncOperation : SocketAsyncEventArgs
    {
        public IChannel Channel { get; private set; }
        public SocketChannelAsyncOperation(IChannel channel)
        {
            Channel = channel;
        }
    }
}
