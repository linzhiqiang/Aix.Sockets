using Aix.SocketCore.Channels;
using Aix.SocketCore.Foundation;
using Aix.SocketCore.Utils;
using System;
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


            await DoConnectAsync(channel, remoteAddress);
            return channel;
        }

        private Task DoConnectAsync(IChannel channel, EndPoint remoteAddress)
        {
            var promise = new TaskCompletionSource();
            channel.EventExecutor.Execute(() => {
                try
                {
                    channel.ConnectAsync(remoteAddress).LinkOutcome(promise); 
                }
                catch (Exception ex)
                {
                    CompleteChannelCloseTaskSafely(channel, ((IChannelUnsafe)channel).UnsafeCloseAsync());
                    promise.TrySetException(ex);
                }
            });
            return promise.Task;
        }
        internal static async void CompleteChannelCloseTaskSafely(object channelObject, Task closeTask)
        {
            try
            {
                await closeTask;
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
               
            }
        }
    }
}
