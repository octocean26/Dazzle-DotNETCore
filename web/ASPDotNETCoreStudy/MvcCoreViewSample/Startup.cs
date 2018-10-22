using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MvcCoreViewSample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc()
                ////自定义位置格式
                //.AddRazorOptions(options =>
                //{
                //    //清除当前的视图位置格式列表。 此时列表包含默认的视图位置格式。
                //    options.ViewLocationFormats.Clear();

                //    // {0} - Action Name
                //    // {1} - Controller Name
                //    // {2} - Area Name
                //    options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
                //    options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
                //    options.ViewLocationFormats.Add("/Views/Shared/Layouts/{0}.cshtml");
                //    options.ViewLocationFormats.Add("/Views/Shared/PartialViews/{0}.cshtml");

                //    options.ViewLocationExpanders.Add(new MultiTenantViewLocationExpander());
                //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        //{
        //    if (env.IsDevelopment())
        //    {
        //        app.UseDeveloperExceptionPage();
        //    }

        //   // app.UseMvcWithDefaultRoute();

        //    app.Run(async (context) =>
        //    {
        //        await context.Response.WriteAsync("<h1>Hello World!</h1><script>alert('ASP.NET Core!');</script>");
        //    });
        //}

        public void Configure(IApplicationBuilder app)
        {

            app.Run(async (context) =>
            {
                string str = "<h1>Hello World!</h1><script>alert('ASP.NET Core!');</script>";
                await context.Response.WriteAsync(str);
            });
        }
    }
}
