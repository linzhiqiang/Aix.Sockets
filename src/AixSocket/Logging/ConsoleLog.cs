using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocket.Logging
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLog();
        }

        public void Dispose()
        {

        }
    }

    public class ConsoleLog : ILogger
    {
        Disposable _DisposableInstance = new Disposable();
        public IDisposable BeginScope<TState>(TState state)
        {
            return _DisposableInstance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                var msg = formatter(state, exception);
                Console.WriteLine(msg);
            }
        }
    }

    class Disposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
