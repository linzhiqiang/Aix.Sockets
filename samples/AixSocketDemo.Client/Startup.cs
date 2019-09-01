using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AixSocketDemo.Client
{
   public class Startup
    {
        internal static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<StartHostService>();
        }
    }
}
