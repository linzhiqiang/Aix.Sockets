using AixSocket.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocket
{
    public static class Extensions
    {
        public static ILoggerFactory AddConsoleLogger(this ILoggerFactory factory)
        {
            factory.AddProvider(new ConsoleLoggerProvider());

            return factory;
        }

        //public static ILoggingBuilder AddConsoleLogger(this ILoggingBuilder factory)
        //{
        //    factory.AddProvider(new ConsoleLoggerProvider());
        //    return factory;
        //}

    }
}
