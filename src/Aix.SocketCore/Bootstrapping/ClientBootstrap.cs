using Aix.SocketCore.Channels;
using System.Net;
using System.Threading.Tasks;

namespace Aix.SocketCore.Bootstrapping
{
    public class ClientBootstrap : AbstractBootstrap<ClientBootstrap>
    {
        public async Task<IChannel> ConnectAsync(EndPoint remoteAddress)
        {
            var channel = this._channelFactory();

            //初始化handler
            _workerHandler(channel);

            //注册事件循环
            await ((IChannelUnsafe)channel).UnsafeRegisterAsync(_workerGroup.GetNext());

            await channel.ConnectAsync(remoteAddress);
            return channel;
        }
    }
}
