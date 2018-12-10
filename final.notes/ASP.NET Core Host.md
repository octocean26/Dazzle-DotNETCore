# ASP.NET Core Host

Host也被称为托管或主机。Host负责应用程序启动和生存期管理，主要有以下两种：

- Web Host：Web主机，适用于托管Web应用。为托管 ASP.NET Core Web 应用，开发人员应使用基于 `IWebHostBuilder` 的 Web 主机。
- Generic Host：通用主机， 适用于托管非 Web 应用（例如，运行后台任务的应用）。 在未来的版本中，通用主机将适用于托管任何类型的应用，包括 Web 应用。 通用主机最终将取代 Web 主机。为托管非 Web 应用，开发人员应使用基于 `HostBuilder`的通用主机。



## Web Host

ASP.NET Core应用需要配置和启动Host，Host负责应用程序的启动和生存期管理，Host至少要配置服务器和请求处理管道。在ASP.NET Core中，使用ASP.NET Core Web主机 （IWebHostBuilder）托管Web应用。

### 设置Host

通常Program.cs中的Main方法在应用的入口点首先被执行，典型的Program.cs中的代码如下：

```c#
public static void Main(string[] args)
{
    CreateWebHostBuilder(args).Build().Run();
}

public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
```

它通过WebHost.CreateDefaultBuilder的调用开始设置Host，CreateDefaultBuilder将会执行以下任务：

###### 配置Web服务器

使用应用程序的Host配置提供程序将Kestrel服务器配置为web服务器。

###### 设置内容根目录

将内容根目录设置为Directory.GetCurrentDirectory返回的路径。

内容根确定主机搜索内容文件（如 MVC 视图文件）的位置。 应用从项目的根文件夹启动时，会将项目的根文件夹用作内容根。 这是 Visual Studio 和 dotnet new 模板中使用的默认值。

###### 加载Host配置

主要通过以下方式加载Host配置：

- 以ASPNETCORE_作为前缀的环境变量（例如，ASPNETCORE_ENVIRONMENT）。
- 命令行参数。

###### 加载应用配置

将按照以下顺序加载应用配置：

- appsettings.json。
- appsettings.{Environment}.json。
- 应用在使用入口程序集的 Development 环境中运行时的机密管理器（Secret Manager）。
- 环境变量。
- 命令行参数。

###### 配置控制台和调试输出的日志记录

日志记录包含 appsettings.json 或 appsettings.{Environment}.json 文件的日志记录配置部分中指定的日志筛选规则。

###### 在IIS后方运行时，启用IIS集成

当使用ASP.NET Core 模块时，可以配置基路径和被服务器侦听的端口。ASP.NET Core模块创建IIS与Kestrel之间的反向代理，还配置应用启动错误的捕获。

###### 设置作用域验证

如果应用环境为“开发”，则 CreateDefaultBuilder 将 ServiceProviderOptions.ValidateScopes 设为 true。

#### 其他方法

除了CreateDefaultBuilder定义的配置外，还可以使用ConfigureAppConfiguration、ConfigureLogging 以及 IWebHostBuilder 的其他方法和扩展方法重写和增强 CreateDefaultBuilder 定义的配置。

##### ConfigureAppConfiguration 

ConfigureAppConfiguration用于指定应用的其他IConfiguration。

下面的示例代码中，ConfigureAppConfiguration调用一个委托，向应用添加appsettings.xml文件中的配置，可以多次调用ConfigureAppConfiguration方法。

```c#
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) => {
        config.AddXmlFile("appsettings.xml", optional: true, reloadOnChange: true);
    })
    .UseStartup<Startup>();
```

##### ConfigureLogging

添加委托以配置提供的ILoggingBuilder，可以被多次调用。

下面的示例代码中，ConfigureLogging 调用添加委托，以将最小日志记录级别 (SetMinimumLevel) 配置为 LogLevel.Warning。 此设置重写了CreateDefaultBuilder在appsettings.Development.json和appsettings.Production.json中配置的设置，这两个配置项分别为 LogLevel.Debug 和 LogLevel.Error。

```c#
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => {
        logging.SetMinimumLevel(LogLevel.Warning);
    })
    .UseStartup<Startup>();
```

##### ConfigureKestrel

用于重写CreateDefaultBuilder中的Kestrel配置。

下面的示例调用ConfigureKestrel来重写CreateDefaultBuilder在配置Kestrel时，Limits.MaxRequestBodySize默认指定的30000000字节。

```c#
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
    .ConfigureKestrel((context,options)=> {
        options.Limits.MaxRequestBodySize = 20_000_000;
    })
    .UseStartup<Startup>();
```

### Host配置值

WebHostBuilder派生自IWebHostBuilder接口，WebHostBuilder依赖于以下几种形式设置Host配置值：

- 基于Host生成器配置，Host生成器配置会读取设置的环境变量，其中包括格式ASPNETCORE_{configurationKey} 的环境变量， 例如 ASPNETCORE_ENVIRONMENT。因此可以通过环境变量进行设置Host配置值。
- 使用UseContentRoot和UseConfiguration等扩展方法显式的设置Host配置值。
- 使用UseSetting方法，该方法需要指定要设置的Host配置值对应的配置键，该值来自于WebHostDefaults的成员变量，同时还要指定要设置的字符串值。

WebHostDefaults的静态成员变量如下，注意它们是只读的：

```c#
public static class WebHostDefaults
{
	public static readonly string ApplicationKey = "applicationName";
	public static readonly string StartupAssemblyKey = "startupAssembly";
	public static readonly string HostingStartupAssembliesKey = "hostingStartupAssemblies";
	public static readonly string HostingStartupExcludeAssembliesKey = "hostingStartupExcludeAssemblies";
	public static readonly string DetailedErrorsKey = "detailedErrors";
	public static readonly string EnvironmentKey = "environment";
	public static readonly string WebRootKey = "webroot";
	public static readonly string CaptureStartupErrorsKey = "captureStartupErrors";
	public static readonly string ServerUrlsKey = "urls";
	public static readonly string ContentRootKey = "contentRoot";
	public static readonly string PreferHostingUrlsKey = "preferHostingUrls";
	public static readonly string PreventHostingStartupKey = "preventHostingStartup";
	public static readonly string SuppressStatusMessagesKey = "suppressStatusMessages";
	public static readonly string ShutdownTimeoutKey = "shutdownTimeoutSeconds";
}
```

下面对常用的Host配置值进行讲述。在每个配置值中，列出的环境变量来自于”ASPNETCORE_配置键“的形式（习惯全大写），下述列出的设置形式只是对常用的形式进行了表述，并不仅仅局限于代码中指定的这种形式。可以结合上述的WebHostDefaults中的成员进行理解。

#### 应用程序名称

配置键：applicationName

环境变量：ASPNETCORE_APPLICATIONNAME

调用方法设置：调用UseSetting方法，传入WebHostDefaults.ApplicationKey，等同于applicationName。

设置说明：设置IHostingEnvironment.ApplicationName属性。在Host构造期间调用UseStartup或Configure时，会自动默认将该属性的值设置为，包含应用入口点的程序集的名称。可以使用下述方法，显式设置该属性值：

```c#
WebHost.CreateDefaultBuilder(args)
//设置IHostingEnvironment.ApplicationName属性值
.UseSetting(WebHostDefaults.ApplicationKey,"MyAppName")
```

#### 捕获启动错误

配置键：captureStartupErrors

环境变量：ASPNETCORE_CAPTURESTARTUPERRORS

调用方法设置：调用CaptureStartupErrors方法

设置说明：此设置控制启动错误的捕获。默认为false，除非应用使用 Kestrel 在 IIS 后方运行，其中默认值是 `true`。当 值为false 时，启动期间出错将会导致主机退出。 当 值为true 时，主机在启动期间捕获异常，但是会尝试启动服务器。

```c#
WebHost.CreateDefaultBuilder(args)
    .CaptureStartupErrors(true)
```

#### 内容根

配置键：contentRoot

环境变量：ASPNETCORE_CONTENTROOT

调用方法形式：调用UseContentRoot方法

设置说明：设置ASP.NET Core 内容文件的根路径，默认为应用程序集所在的文件夹。内容根也是Web根的基路径（webroot包含在内容根内），如果内容根不存在，主机将无法启动。

```c#
WebHost.CreateDefaultBuilder(args)
    .UseContentRoot("c:\\<content-root>")
```

#### 详细错误

配置键：detailedErrors

环境变量：ASPNETCORE_DETAILEDERRORS

调用方法形式：调用UseSetting方法

设置说明：用于设置是否应捕获详细错误，默认值为false。当环境设置为Development或启用时，将会捕获详细的异常。

```c#
WebHost.CreateDefaultBuilder(args)
    .UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
```

#### 环境

配置键：environment

环境变量：ASPNETCORE_ENVIRONMENT

调用方法方式：调用UseEnvironment方法

设置说明：用于设置应用的环境。环境可以设置为任何值，框架定义的值包括Development和Production，值不区分大小写，默认情况下，从ASPNETCORE_ENVIRONMENT 环境变量读取环境。 使用 Visual Studio 时，可能会在 launchSettings.json 文件中设置环境变量。 

```c#
WebHost.CreateDefaultBuilder(args)
    .UseEnvironment(EnvironmentName.Development)
```

#### 承载启动程序集

配置键：hostingStartupAssemblies

环境变量：ASPNETCORE_HOSTINGSTARTUPASSEMBLIES

调用方法方式：调用UseSetting方法

设置说明：设置应用的承载启动程序集，值为以分号分割的程序集字符串，对应的承载启动程序集将在应用启动时被加载。默认值为空字符串，虽然没有显式的指定，但是承载启动程序集会始终包含应用的程序集。提供承载启动程序集时，当应用在启动过程中生成其公用服务时，将加载它们添加到应用的程序集。

```
WebHost.CreateDefaultBuilder(args)
    .UseSetting(WebHostDefaults.HostingStartupAssembliesKey, "assembly1;assembly2")
```

#### HTTPS端口

配置键：https_port

环境变量：ASPNETCORE_HTTPS_PORT

调用方法方式：使用UseSetting方法

设置说明：该配置键未在WebHostDefaults中指定，实际使用时，是一个字符串具体值。它用于设置HTTPS重定向的端口，用于强制实施HTTPS。

```c#
WebHost.CreateDefaultBuilder(args)
    .UseSetting("https_port", "8080")
```

#### 承载启动排除程序集

配置键：hostingStartupExcludeAssemblies

环境变量：ASPNETCORE_HOSTINGSTARTUPEXCLUDEASSEMBLIES

调用方法方式：UseSetting方法

设置说明：值是以分号分隔的承载启动程序集字符串，这些指定的程序集将在启动时排除。

```c#
WebHost.CreateDefaultBuilder(args)
    .UseSetting(WebHostDefaults.HostingStartupExcludeAssembliesKey, "assembly1;assembly2")
```

#### 首选承载URL

配置键：preferHostingUrls

环境变量：ASPNETCORE_PREFERHOSTINGURLS

调用方法方式：调用PreferHostingUrls方法进行设置

设置说明：该设置指示Host是否应该侦听使用WebHostBuilder配置的URL，而不是使用IServer实现配置的URL。默认值为true。

```c#
WebHost.CreateDefaultBuilder(args)
    .PreferHostingUrls(false)
```

#### 阻止承载启动

配置值：preventHostingStartup

环境变量：ASPNETCORE_PREVENTHOSTINGSTARTUP

调用方法方式：使用UseSetting

设置说明：是否阻止承载启动程序集自动加载，包括应用的程序集所配置的承载启动程序集，默认值为false。

```c#
WebHost.CreateDefaultBuilder(args)
    .UseSetting(WebHostDefaults.PreventHostingStartupKey, "true")
```

#### 服务器URL

配置值：urls

环境变量：ASPNETCORE_URLS

调用方式：调用UseUrls方法进行设置

设置说明：设置服务器应响应的以分号分隔（;）的URL前缀列表，例如：http://localhost:123。设置了该值后，必须通过访问设置的URL才能请求服务。使用“`*`”指示服务器应针对请求侦听的使用特定端口和协议（例如 http://*:5000）的 IP 地址或主机名。 协议（http:// 或 https://）必须包含每个 URL。 不同的服务器支持的格式有所不同。

```c#
WebHost.CreateDefaultBuilder(args)
    .UseUrls("http://*:5000;http://localhost:5001;https://hostname:5002")
```

#### 关闭超时

配置值：shutdownTimeoutSeconds

环境变量：ASPNETCORE_SHUTDOWNTIMEOUTSECONDS

调用方式：调用方法UseShutdownTimeout进行设置

设置说明：设置等待Web Host关闭的时长。也可以使用UseSetting方法进行设置，例如：

```c#
WebHost.CreateDefaultBuilder(args)
.UseSetting(WebHostDefaults.ShutdownTimeoutKey, "10")
```

UseSetting方法接受int字符串值，而UseShutdownTimeout扩展方法接收TimeSpan，如下：

```c#
WebHost.CreateDefaultBuilder(args)
    .UseShutdownTimeout(TimeSpan.FromSeconds(10))
```

在超时时间段中，Host将会触发 IApplicationLifetime.ApplicationStopping，并尝试停止Host服务，对服务停止失败的任何错误进行日志记录。

如果在所有Host服务停止之前就达到了超时时间，则会在应用关闭时会终止剩余的所有活动的服务。 即使没有完成处理工作，服务也会停止。 如果停止服务需要额外的时间，那么就需要增加超时时间。

#### 启动程序集

配置值：startupAssembly

环境变量：ASPNETCORE_STARTUPASSEMBLY

调用方式：调用UseStartup方法

设置说明：用于设置要在应用中搜索Startup类的程序集，可以引用按名称（string）或类型（TStartup）的程序集。如果调用多个UseStartup方法，优先选择最后一个方法。

```
WebHost.CreateDefaultBuilder(args)
    .UseStartup("StartupAssemblyName")
```

或者：

```
WebHost.CreateDefaultBuilder(args)
    .UseStartup<TStartup>()
```

#### Web 根路径

配置值：webroot

环境变量：ASPNETCORE_WEBROOT

调用方式：调用UseWebRoot方法

设置说明：设置应用的静态资源的相对路径，如果未指定，默认值是“(Content Root)/wwwroot”（如果该路径存在）。 如果该路径不存在，则使用无操作文件提供程序。

```c#
WebHost.CreateDefaultBuilder(args)
    .UseWebRoot("public")
```

### 重写配置

UseConfiguration可用于配置Web Host，注意和上述中的ConfigureAppConfiguration之间的不同。

#### UseConfiguration和ConfigureAppConfiguration的区别

- 配置的对象不同：UseConfiguration针对Web Host进行配置（框架配置），而ConfigureAppConfiguration针对的是应用程序配置，换句话说，如果只是应用在应用程序上的配置，应该使用ConfigureAppConfiguration，ConfigureAppConfiguration可以多次被调用；如果是应用在框架上的Host配置，比如服务器URL、环境等配置，这些应该使用UseConfiguration。（一般来说如果名称上带有“App”的，都是基于应用程序的，相关的配置就需要使用ConfigureAppConfiguration方法，例如：appsettings.json。）
- UseConfiguration添加的配置会影响ConfigureAppConfiguration添加的配置，这是因为IWebHostBuilder配置会添加到应用配置中，而使用ConfigureAppConfiguration添加的配置，并不会影响IWebHostBuilder 配置，也就是说基于框架的配置，优先级别更高，不会被ConfigureAppConfiguration影响。

#### 使用UseConfiguration重写配置

在下面的示例中，先使用UseUrls指定Url后，再使用hostsettings.json重写UseUrls提供的配置，重写时调用了UseConfiguration方法，在重写配置的过程中，主机配置是根据需要在 hostsettings.json 文件中指定。 命令行参数可能会重写从 hostsettings.json 文件加载的任何配置。 生成的配置（在 config 中）用于通过 UseConfiguration 配置主机。

```c#
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("hostsettings.json", optional: true)
            .AddCommandLine(args)
            .Build();

        return WebHost.CreateDefaultBuilder(args)
            .UseUrls("http://*:5000")
            .UseConfiguration(config)
            .Configure(app =>
            {
                app.Run(context => 
                    context.Response.WriteAsync("Hello, World!"));
            });
    }
}
```

hostsettings.json：

```
{
    urls: "http://*:5005"
}
```

注意：UseConfiguration 只将所提供的 IConfiguration 中的键复制到主机生成器配置中。 因此，JSON、INI 和 XML 设置文件的设置 reloadOnChange: true 没有任何影响。

UseConfiguration 方法需要键来匹配 WebHostBuilder 键（例如 urls、environment）。若要指定在特定的 URL 上运行的主机，所需的值可以在执行 dotnet 运行时从命令提示符传入。 命令行参数重写 hostsettings.json 文件中的 urls 值，且服务器侦听端口 8080：

```
dotnet run --urls "http://*:8080"
```

### 管理主机

#### Run()

Run 方法启动 Web 应用并阻塞调用线程，直到关闭主机。

```
host.Run();
```

#### Start()

通过调用 Start 方法以非阻塞方式运行主机：

```c#
using (host)
{
    host.Start();
    Console.ReadLine();
}
```

如果将URL的列表传递给Start方法，那么将侦听该列表指定的URL：

```c#
var urls = new List<string>()
{
    "http://*:5000",
    "http://localhost:5001"
};

var host = new WebHostBuilder()
    .UseKestrel()
    .UseStartup<Startup>()
    .Start(urls.ToArray());

using (host)
{
    Console.ReadLine();
}
```

在使用WebHost.CreateDefaultBuilder方法时，应用通过该方法的预配置的默认值初始化并启动新的主机，这些方法在没有控制台输出的情况下启动服务器，并使用 WaitForShutdown 等待中断（Ctrl-C/SIGINT 或 SIGTERM）。

#### Start(RequestDelegate app)和Start(string url,RequestDelegate app)

这两个方法执行的相同的结果，不同的是Start(string url,RequestDelegate app)用于在指定的URL上进行响应，第二个参数RequestDelegate在两个方法中的用法相同：

```c#
using (var host = WebHost.Start("http://localhost:8080", app => app.Response.WriteAsync("Hello, World!")))
{
    Console.WriteLine("Use Ctrl-C to shutdown the host...");
    host.WaitForShutdown();
}
```

运行上述代码后，在浏览器中向http://localhost:5000 发出请求，接收响应“Hello World!” WaitForShutdown 受到阻止，直到发出中断（Ctrl-C/SIGINT 或 SIGTERM）。 应用显示 Console.WriteLine 消息并等待 keypress 退出。

#### `Start(Action<IRouteBuilder> routeBuilder)`和`Start(string url, Action<IRouteBuilder> routeBuilder)`

这两个方法执行的结果相同，使用 IRouteBuilder 的实例 (Microsoft.AspNetCore.Routing) 用于路由中间件，可以指定URL进行响应：

```c#
using (var host = WebHost.Start("http://localhost:8080", router => router
    .MapGet("hello/{name}", (req, res, data) => 
        res.WriteAsync($"Hello, {data.Values["name"]}!"))
    .MapGet("buenosdias/{name}", (req, res, data) => 
        res.WriteAsync($"Buenos dias, {data.Values["name"]}!"))
    .MapGet("throw/{message?}", (req, res, data) => 
        throw new Exception((string)data.Values["message"] ?? "Uh oh!"))
    .MapGet("{greeting}/{name}", (req, res, data) => 
        res.WriteAsync($"{data.Values["greeting"]}, {data.Values["name"]}!"))
    .MapGet("", (req, res, data) => res.WriteAsync("Hello, World!"))))
{
    Console.WriteLine("Use Ctrl-C to shut down the host...");
    host.WaitForShutdown();
}
```

WaitForShutdown 受到阻塞，直到发出中断（Ctrl-C/SIGINT 或 SIGTERM）。 应用显示 Console.WriteLine 消息并等待 keypress 退出。

#### `StartWith(Action<IApplicationBuilder> app)`和`StartWith(string url, Action<IApplicationBuilder> app)`

这两个方法执行的结果相同，都提供委托以配置 IApplicationBuilder，第二个方法提供了响应的URL：

```c#
using (var host = WebHost.StartWith("http://localhost:8080", app => 
    app.Use(next => 
    {
        return async context => 
        {
            await context.Response.WriteAsync("Hello World!");
        };
    })))
{
    Console.WriteLine("Use Ctrl-C to shut down the host...");
    host.WaitForShutdown();
}
```

备注：上述的这些方法都提供了URL参数版本，除此之外，带URL和不带URL的方法的其他参数的使用都相同。

### IHostingEnvironment 接口

IHostingEnvironment 接口提供有关应用的 Web Hosting环境的信息。 可以使用构造函数注入的方式获取 IHostingEnvironment，以使用其属性和扩展方法。

```c#
public class CustomFileReader
{
    private readonly IHostingEnvironment _env;

    public CustomFileReader(IHostingEnvironment env)
    {
        _env = env;
    }

    public string ReadFile(string filePath)
    {
        var fileProvider = _env.WebRootFileProvider;
        // Process the file here
    }
}
```

基于环境的Startup类和方法可以用于在启动时基于环境配置应用，或者，将IHostingEnvironment 注入到 Startup 构造函数用于 ConfigureServices：

```c#
public class Startup
{
    public Startup(IHostingEnvironment env)
    {
        HostingEnvironment = env;
    }

    public IHostingEnvironment HostingEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        if (HostingEnvironment.IsDevelopment())
        {
            // Development configuration
        }
        else
        {
            // Staging/Production configuration
        }

        var contentRootPath = HostingEnvironment.ContentRootPath;
    }
}
```

IHostingEnvironment 服务还可以直接注入到 Configure 方法以设置处理管道：

```c#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        // In Development, use the developer exception page
        app.UseDeveloperExceptionPage();
    }
    else
    {
        // In Staging/Production, route exceptions to /error
        app.UseExceptionHandler("/error");
    }

    var contentRootPath = env.ContentRootPath;
}
```

创建自定义中间件时可以将 IHostingEnvironment 注入 Invoke 方法：

```c#
public async Task Invoke(HttpContext context, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        // Configure middleware for Development
    }
    else
    {
        // Configure middleware for Staging/Production
    }

    var contentRootPath = env.ContentRootPath;
}
```

### IApplicationLifetime 接口

IApplicationLifetime允许启动后和关闭活动，该接口的三个属性是用于注册Action方法的取消标记。

| 取消标记            | 触发条件                                                     |
| ------------------- | ------------------------------------------------------------ |
| ApplicationStarted  | 主机已完全启动。                                             |
| ApplicationStopped  | 主机正在完成正常关闭。 应处理所有请求。 关闭受到阻止，直到完成此事件。 |
| ApplicationStopping | 主机正在执行正常关闭。 仍在处理请求。 关闭受到阻止，直到完成此事件。 |

```c#
public class Startup
{
    public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime)
    {
        appLifetime.ApplicationStarted.Register(OnStarted);
        appLifetime.ApplicationStopping.Register(OnStopping);
        appLifetime.ApplicationStopped.Register(OnStopped);

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            appLifetime.StopApplication();
            // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
            eventArgs.Cancel = true;
        };
    }

    private void OnStarted()
    {
        // Perform post-startup activities here
    }

    private void OnStopping()
    {
        // Perform on-stopping activities here
    }

    private void OnStopped()
    {
        // Perform post-stopped activities here
    }
}
```

StopApplication 请求应用终止。 以下类在调用类的 Shutdown 方法时使用 StopApplication 正常关闭应用：

```c#
public class MyClass
{
    private readonly IApplicationLifetime _appLifetime;

    public MyClass(IApplicationLifetime appLifetime)
    {
        _appLifetime = appLifetime;
    }

    public void Shutdown()
    {
        _appLifetime.StopApplication();
    }
}
```

### 作用域验证

