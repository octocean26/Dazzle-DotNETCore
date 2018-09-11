# ASP.NET Core MVC引导



## 注册MVC服务

MVC应用程序模型的核心是MvcRouteHandler服务，它是负责解析MVC路由的URL，调用所选控制器方法以及处理操作结果的引擎。要将MVC路由处理程序服务添加到ASP.NET主机，只需向启动类的ConfigureServices方法添加一行代码即可。

```c#
public void ConfigureServices(IServiceCollection services)
{
	services.AddMvc();
}
```

AddMvc方法有两个重载。 无参数方法接受MVC服务的所有默认设置。 第二个重载，如下所示，允许您选择临时选项。

```c#
services.AddMvc(options =>
{
    options.ModelBinderProviders.Add(new SmartDateBinderProvider());
    options.SslPort = 345;
});
```

选项通过MvcOptions类的实例指定。 该类是可以在MVC框架中更改的配置参数的容器。 例如，上面的代码片段添加了一个新的模型绑定器，它将特定字符串解析为有效日期，并指定在使用RequireHttpsAttribute修饰控制器类时要使用的SSL端口。 完整的可配置选项列表可在此处找到：https://docs.microsoft.com/zh-cn/dotnet/api/microsoft.aspnetcore.mvc.mvcoptions

#### 启用了其他附加服务

当使用AddMvc()方法注册服务后，在该方法下，许多其他服务也一同被初始化并添加到管道中。

下面是使用AddMvc()方法后，一同被启用的MVC服务介绍：

- MVC Core：MVC应用程序模型的核心服务集，包括路由和控制器。
- API Explorer：服务负责收集和公开有关控制器和动作的信息，以便动态发现功能和帮助页面。
- Authorization：认证和授权背后的服务
- Default Framework Parts：将输入标记助手和URL解析助手添加到应用程序部件列表的服务
- Formatter Mappings：设置默认媒体类型映射的服务
- Views：处理操作的服务结果为HTML视图
- Razor Engine：将Razor视图和页面引擎注册到MVC系统中
- Tag Helpers：服务引用标签助手的框架部分
- Data Annotations：服务以引用关于数据注释的框架部分
- JSON Formatters：处理操作的服务结果为JSON流
- CORS：服务以引用关于跨源资源共享（CORS）的框架部分

注意：在上述服务中，某些服务仅在您公开Web API时才有用。 这些服务是API Explorer，Formatter Mappings和CORS。

如果您有内存限制 - 例如，您在云中托管应用程序 - 您可能希望应用程序只引用框架的裸机。 上述中的服务列表可以缩短; 缩短程度主要取决于您在应用程序中需要具备的实际功能。 以下代码足以在没有更高级功能的情况下提供纯HTML视图，例如表单验证和标记帮助程序的数据注释。

```c#
public void ConfigureServices(IServiceCollection services)
{
    var builder = services.AddMvcCore();
    builder.AddViews();
    builder.AddRazorViewEngine();
}
```

但是，上面的代码不足以返回格式化的JSON数据。 要添加该功能，您只需添加：

```c#
builder.AddJsonFormatters();
```



## 激活MVC服务

在启动类的Configure方法中，调用UseMvc方法来配置ASP.NET Core管道以支持MVC应用程序模型。此时，除了传统路由之外，MVC应用程序模型周围的所有内容都已完全设置。路由是应用程序可以识别和处理的URL模板。路由最终映射到一对控制器和操作名称。可以根据需要添加任意数量的路由，这些路由几乎可以采用您喜欢的任何形式。内部MVC服务负责请求路由;启用MVC Core服务时会自动注册。



## 启用常规路由

为了可以使用，您的应用程序应提供规则来选择它想要处理的URL。但是，并非所有可行的URL都必须明确列出; 一个或多个带占位符的URL模板将完成这项工作。 存在默认路由规则，有时称为传统路由。 通常，默认路由足以满足整个应用程序的需要。

#### 添加默认路由

如果您对路由没有任何特殊顾虑，最简单的方法是仅使用默认路由。

```c#
public void Configure(IApplicationBuilder app)
{
    app.UseMvcWithDefaultRoute();
}
```

UseMvcWithDefaultRoute方法背后的实际代码如下所示：

```c#
public void Configure(IApplicationBuilder app)
{
    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

路由参数 - 特别是名为controller和action的路由参数 - 在传入请求的整体处理中起着关键作用，因为它们以某种方式指向实际产生响应的代码。任何成功映射到路由的请求都将通过在控制器类上执行方法来处理。名为controller的路由参数标识控制器类，名为action的路由参数标识要调用的方法。

#### 没有配置路由时

可以在没有参数的情况下调用UseMvc方法。发生这种情况时，ASP.NET MVC应用程序功能齐全，但没有可以处理的已配置路由。

```c#
public void Configure(IApplicationBuilder app)
{
    app.UseMvc();
}
```

上面的代码完全等同于下面的代码段：

```c#
app.UseMvc(routes => { });
```



























