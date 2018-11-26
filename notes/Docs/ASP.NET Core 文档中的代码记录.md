# ASP.NET Core 官方文档中的代码记录

本文存在的目的： 如果收藏的书签无法快速定位到具体内容，那么可以参考本文用于辅助查找代码，一切以官方文档为主，书签收藏的内容优先于本文描述的内容。



### 不使用CreateDefaultBuilder方法直接创建WebHostBuilder，并添加配置

```
var config = new ConfigurationBuilder()
    // Call additional providers here as needed.
    // Call AddCommandLine last to allow arguments to override other configuration.
    .AddCommandLine(args)
    .Build();

var host = new WebHostBuilder()
    .UseConfiguration(config)
    .UseKestrel()
    .UseStartup<Startup>();
```

来源：https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1#command-line-configuration-provider





