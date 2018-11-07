using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Securing_Sample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // 准备要绑定到用户身份的声明列表
            Claim[] claims = new Claim[] {
                new Claim(ClaimTypes.Name,"smallz"),
                new Claim("display_name","wy"),
                new Claim(ClaimTypes.Email,"wy@163.com"),
                new Claim("picture_url","/images/my.png"),
                new Claim("age","24"),
                new Claim(ClaimTypes.Role,"Manager"),
                new Claim(ClaimTypes.Role,"admin")

            };
              

            // 从声明中创建认证对象
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            //从identity创建主体对象
            var principal = new ClaimsPrincipal(identity);




            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            })
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Account/Login");
                    options.Cookie.Name = "CookieName_Octocean";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    //SlidingExpiration设置为true，以指示处理程序在处理超过到期窗口一半的请求时，重新发出具有新到期时间的新cookie。
                    options.SlidingExpiration = true;
                    //处理ForbidAsync时，处理程序将处理AccessDeniedPath属性用于重定向目标。
                    options.AccessDeniedPath = new PathString("/Account/Denied");

                });
            //.AddOpenIdConnect(options =>
            //{
            //    options.Authority = "http://localhost:6666";
            //    options.ClientId = "";
            //    options.ClientSecret = "";

            //});

            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvcWithDefaultRoute();

        }
    }
}
