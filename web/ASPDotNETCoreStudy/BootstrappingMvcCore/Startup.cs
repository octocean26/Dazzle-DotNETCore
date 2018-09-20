using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

namespace BootstrappingMvcCore
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(new CultureAttribute());
            });

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //自定义路由
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "route-today",
                    template: "today/{offset}",
                    defaults: new { controller = "date", action = "day", offset = 0 });

                routes.MapRoute(
                    name: "route-yesterday",
                    template: "yesterday",
                    defaults: new { controller = "date", action = "day", offset = -1 });

                routes.MapRoute(
                    name: "route-tomorrow",
                    template: "tomorrow/{format:regex(A|B|C)}",
                    defaults: new { controller = "date", action = "day", offset = 1 });

                routes.MapRoute(
                    name: "route-day",
                    template: "date/day/{offset}",
                    defaults: new { controller = "date", action = "day", offset = 0 });
            });

            //常规路由
            app.UseMvcWithDefaultRoute();

            //路由约束
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "route-today",
                    template: "today/{offset}",
                    defaults: new { controller = "date", action = "day", offset = 0 },
                    constraints: new { offset = new IntRouteConstraint() });
            });

            //数据令牌
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "catch-all",
                    template: "{*url}",
                    defaults: new { controller = "home", action = "index" },
                    constraints: new { },
                    dataTokens: new { reason = "catch-all" });
            });

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});


            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "route-today",
            //        template: "today",
            //        defaults: new { controller = "date", action = "day", offset = 0 });

            //});




            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}