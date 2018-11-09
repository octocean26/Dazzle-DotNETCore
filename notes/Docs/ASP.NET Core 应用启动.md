# ASP.NET Core 中的应用启动

启动类用于配置服务和应用程序的请求管道。



## Startup类

在ASP.NET Core中，按照约定将启动类命名Startup，Startup类必须包括Configure方法，该方法用于创建应用的请求处理管道。除了Configure方法外，Startup类也可以包括ConfigureServices方法，但不是必须的，ConfigureServices方法用于配置应用的服务。

Startup类最终在Program.cs中被引入。

当创建一个Empty的Web应用程序时，Startup类中默认包含的代码如下：

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.Run(async (context) =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
    }
}
```

上述代码中，并没有显式的指定Startup构造函数，如果你想要使用常见的一些服务，比如配置服务、日志记录服务等，可以在Startup构造函数中显式的指定，这些是Web主机提供给Startup类构造函数可用的服务，常见的有：

- IHostingEnvironment 以按环境配置服务。
- IConfiguration 以读取配置。
- ILoggerFactory 以在 Startup.ConfigureServices 中创建记录器。

除了这些服务之外，应用还可以通过ConfigureServices方法添加其他服务，然后，主机和应用服务都可以在Configure和整个应用中使用。

改写后的代码如下：

```c#
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

        app.Run(async (context) =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
    }
}
```



## ConfigureServices方法

ConfigureServices方法是可选的，它在Configure方法配置应用程序之前，由Web主机调用，用于设置配置选项。

该方法中，典型模式是先调用所有的services.Add{Service}方法，然后再调用所有的services.Configure{Service} 方法。虽然并不是所有的都遵循该模式，但是一般而言，都至少会调用这两个形式的方法，只是先后顺序不一样。例如，在使用VS2017创建ASP.NET Core MVC应用程序时，ConfigureServices方法包含的代码如下：

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<CookiePolicyOptions>(options =>
    {
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
}
```

ConfigureServices方法将服务添加到服务容器，使其能够在应用中（通过依赖关系注入解析）和Configure 方法中（通过IApplicationBuilder.ApplicationServices解析）可用。

注意：并不是所有的服务都会在ConfigureServices方法中配置，Web主机可能会在调用 Startup 方法之前配置某些服务。

ConfigureServices方法提供了IServiceCollection参数，IServiceCollection提供了Add[Service] 扩展方法，可以支持大量设置的功能，常见的如实体框架、身份标识和MVC等：

```c#
public void ConfigureServices(IServiceCollection services)
{
    // 添加实体框架服务.
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
    // 添加身份服务
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
    // 添加MVC的支持
    services.AddMvc();

    // 添加其他的应用程序服务
    services.AddTransient<IEmailSender, AuthMessageSender>();
    services.AddTransient<ISmsSender, AuthMessageSender>();
}
```



## Configure方法

Configure方法用于指定应用程序如何响应HTTP请求。请求管道是通过向IApplicationBuilder实例添加中间件组件来配置的。Configure方法可以使用IApplicationBuilder，但它不在服务容器中注册。托管负责创建IApplicationBuilder，并将其直接传递给Configure方法。

下面是创建MVC应用程序默认生成的代码：

```c#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCookiePolicy();

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

每个Use扩展方法都向请求管道添加一个中间件组件。例如，UseMvc扩展方法将路由中间件添加到请求管道中，并将MVC配置为默认处理程序。


请求管道中的每个中间件组件负责调用管道中的下一个组件，或者在适当的情况下使链短路。如果在中间件链中没有发生短路，那么每个中间件都有第二次机会在将请求发送到客户端之前处理该请求。

其他服务，如IHostingEnvironment和ILoggerFactory，也可以在方法签名中指定。如果指定了这些服务，一旦可用，就会被注入。



## 不使用Startup类

可以使用ConfigureServices和Configure便利方法来代替指定启动类。 多次调用 ConfigureServices 将追加到上一个。 多次调用 Configure 将使用上一个方法调用（注：这里描述不够准确，需要实际验证，强烈建议只调用一次Configure方法）。

```c#
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
        public static IHostingEnvironment HostingEnvironment { get; set; }
        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                HostingEnvironment = hostingContext.HostingEnvironment;
                Configuration = config.Build();
            })
            .ConfigureServices(services =>
            {
                services.AddMvc();
            })
            .Configure(app =>
            {
                ILoggerFactory loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
                ILogger<Program> logger = loggerFactory.CreateLogger<Program>();
                logger.LogInformation("Logged in Configure");

                if (HostingEnvironment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                }

                //可以在此处访问Configuration
                app.UseMvcWithDefaultRoute();
                app.UseStaticFiles();
            });
        }
    }
}
```





















