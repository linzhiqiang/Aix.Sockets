using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AixSocket.Logging
{
    public class InternalLoggerFactory
    {
        static ILoggerFactory defaultFactory;
        public static ILoggerFactory DefaultFactory
        {
            get
            {
                ILoggerFactory factory = Volatile.Read(ref defaultFactory);
                if (factory == null)
                {
                    factory = new LoggerFactory();
                    ILoggerFactory current = Interlocked.CompareExchange(ref defaultFactory, factory, null);
                    if (current != null)
                    {
                        return current;
                    }
                }
                return factory;
            }
            set
            {
                Volatile.Write(ref defaultFactory, value);
            }
        }

        public static ILogger GetLogger<T>() => GetLogger(typeof(T));

        public static ILogger GetLogger(Type type) => GetLogger(type.FullName);


        public static ILogger GetLogger(string name) => defaultFactory.CreateLogger(name);
    }
   
}
