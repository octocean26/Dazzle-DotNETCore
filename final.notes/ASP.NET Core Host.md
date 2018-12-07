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













​                                                   
