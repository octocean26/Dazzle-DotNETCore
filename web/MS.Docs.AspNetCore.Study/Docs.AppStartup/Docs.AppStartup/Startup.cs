using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Docs.AppStartup
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IHostingEnvironment env, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _env = env;
            _config = configuration;
            _loggerFactory = loggerFactory;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            
            ILogger<Startup> logger = _loggerFactory.CreateLogger<Startup>();
            if (_env.IsDevelopment())
            {
                logger.LogInformation("当前是开发环境");
            }
            else
            {
                logger.LogInformation($"环境名称:{_env.EnvironmentName}");
            }
            //可以在此处直接访问_config


             
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            
            

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }

         
        
    }
}