using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Aix.SocketCore.Utils;
using Aix.SocketCore.Foundation;

namespace AixSocketDemo.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test();
            //Console.Read();

            var host = new HostBuilder()
                   .ConfigureHostConfiguration(builder =>
                   {
                       builder.AddEnvironmentVariables(prefix: "Demo_");
                   })
                    .ConfigureAppConfiguration((hostContext, config) =>
                    {
                        config.AddJsonFile("config/appsettings.json", optional: true);
                        config.AddJsonFile($"config/appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);// 覆盖前面的相同内容

                  })
                   .ConfigureLogging((context, factory) =>
                   {
                       factory.AddConsole();
                   })
                   .ConfigureServices(Startup.ConfigureServices);

            host.RunConsoleAsync().Wait();
            Console.WriteLine("服务已退出");
        }

        private static void Test()
        {
            Task.Run(async()=> {
                try
                {
                    await SafeExecuteOutboundAsync(taskMethod);
                }
                catch (Exception ex)
                { 
                
                }
            });
        }

        private static Task taskMethod()
        {
            return Task.Run(()=> {
                throw new Exception("error");
                Console.WriteLine("async taskMethod");
            });
        }

        static Task SafeExecuteOutboundAsync(Func<Task> function)
        {
            var promise = new TaskCompletionSource();
            try
            {
                //executor.Execute((p, func) => ((Func<Task>)func)().LinkOutcome((TaskCompletionSource)p), promise, function);
                Task.Run(async()=> {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    function().LinkOutcome(promise);
                });
            }
            catch (Exception cause)
            {
                promise.TrySetException(cause);
            }
            return promise.Task;
        }
    }
}
