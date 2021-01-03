using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.SocketCore.Utils
{
    public static class With
    {
        public static void NoException(Action action)
        {
            try
            {
                action();
            }
            catch
            {
            }
        }
    }
}
