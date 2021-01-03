using Aix.SocketCore.Channels;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Aix.SocketCore.Foundation
{
    public class SocketChannelAsyncOperation : SocketAsyncEventArgs
    {
        public IChannel Channel { get; private set; }
        public SocketChannelAsyncOperation(IChannel channel)
        {
            Channel = channel;
        }

        public void Validate()
        {
            SocketError socketError = this.SocketError;
            if (socketError != SocketError.Success)
            {
                throw new SocketException((int)socketError);
            }
        }
    }
}
