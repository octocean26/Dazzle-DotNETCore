using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace DI_Sample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IFlagRepository, FlagRepository>();
            services.AddMvc();
            
            services.AddTransient<IFlagRepository>(provider =>
            {
                //创建要返回的实际类型的实例
                //基于当前登录用户的身份
                IHttpContextAccessor context = provider.GetRequiredService<IHttpContextAccessor>();
                return new FlagRepositoryForUser(context.HttpContext.User);
                 

            });
        }



        //public IServiceProvider ConfigureServices(IServiceCollection services)

        //{
 
        //    services.AddTransient<IFlagRepository, FlagRepository>();



        //    // Create the container of the external DI library 

        //    // Using StructureMap here.

        //    var structureMapContainer = new Container();



        //    // Add your own services using the native API of the DI library

        //    // ...



        //    // Add services already registered with the ASP.NET Core DI system

        //    structureMapContainer.Populate(services);



        //    // Return the implementation of IServiceProvider using internally

        //    // the external library to resolve dependencies

        //    return structureMapContainer.GetInstance<IServiceProvider>();

        //}



        //public void ConfigureServices(IServiceCollection services)

        //{

        //    services.AddTransient<IFlagRepository>(provider =>

        //    {

        //        // Create the instance of the actual type to return 

        //        // based on the identity of the currently logged user. 

        //        var context = provider.GetRequiredService<IHttpContextAccessor>();

        //        return new FlagRepositoryForUser(context.HttpContext.User);

        //    });

        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
