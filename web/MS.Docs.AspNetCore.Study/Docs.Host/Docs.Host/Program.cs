using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Docs.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new HostBuilder()
               .UseServiceProviderFactory<ServiceContainer>(new ServiceContainerFactory())
               .ConfigureContainer<ServiceContainer>((hostContext, ServiceContainer) => {

               });

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
