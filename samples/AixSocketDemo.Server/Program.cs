using AixSocket.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace AixSocketDemo.Server
{
    //Microsoft.Extensions.Hosting
    //Microsoft.Extensions.Configuration.EnvironmentVariables
    //Microsoft.Extensions.Configuration.Json
    //Microsoft.Extensions.Logging.Console

    class Program
    {
        static void Main(string[] args)
        {
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
                     factory.SetMinimumLevel(LogLevel.Trace);

                 })
                 .ConfigureServices(Startup.ConfigureServices);

            host.RunConsoleAsync().Wait();
            Console.WriteLine("服务已退出");
        }
        
    }
}
