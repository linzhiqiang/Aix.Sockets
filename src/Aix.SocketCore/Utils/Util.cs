using Aix.SocketCore.Channels;
using AixSocket.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.SocketCore.Utils
{
  public static  class Util
    {
        static readonly ILogger Logger = InternalLoggerFactory.GetLogger<IChannel>();


        public static void CloseSafe(this IChannel channel)
        {
            CompleteChannelCloseTaskSafely(channel, channel.CloseAsync());
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
                Logger.LogDebug("Failed to close channel " + channelObject + " cleanly.", ex);
            }
        }
    }
}
