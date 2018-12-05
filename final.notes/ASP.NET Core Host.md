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

