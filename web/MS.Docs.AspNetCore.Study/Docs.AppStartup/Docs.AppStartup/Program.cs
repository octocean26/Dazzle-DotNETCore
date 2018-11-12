using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Docs.AppStartup
{
    public class Program
    {
        public static void Main(string[] args)
        { 
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureServices(service => {
                service.AddTransient<IStartupFilter, RequestSetOptionsStartupFilter>();
            })
            .UseStartup<Startup>();


        #region 不使用Startup类
        //public static IHostingEnvironment HostingEnvironment { get; set; }
        //public static IConfiguration Configuration { get; set; }

        //public static void Main(string[] args)
        //{
        //    CreateWebHostBuilder(args).Build().Run();
        //}

        //public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        //{
        //    return WebHost.CreateDefaultBuilder(args)
        //    .ConfigureAppConfiguration((hostingContext, config) =>
        //    {
        //        HostingEnvironment = hostingContext.HostingEnvironment;
        //        Configuration = config.Build();
        //    })
        //    .ConfigureServices(services =>
        //    {
        //        services.AddMvc();
        //    })
        //    .Configure(app =>
        //    {
        //        ILoggerFactory loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
        //        ILogger<Program> logger = loggerFactory.CreateLogger<Program>();
        //        logger.LogInformation("Logged in Configure");

        //        if (HostingEnvironment.IsDevelopment())
        //        {
        //            app.UseDeveloperExceptionPage();
        //        }
        //        else
        //        {
        //            app.UseExceptionHandler("/Error");
        //        }

        //        //可以在此处访问Configuration
        //        app.UseMvcWithDefaultRoute();
        //        app.UseStaticFiles();
        //    });
        //}
        #endregion
    }
}
