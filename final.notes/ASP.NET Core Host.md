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





 
