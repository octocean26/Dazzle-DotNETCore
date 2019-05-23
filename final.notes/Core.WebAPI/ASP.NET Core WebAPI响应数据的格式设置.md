# ASP.NET Core WebAPI响应数据的格式设置

原文参考链接：<https://docs.microsoft.com/zh-cn/aspnet/core/web-api/advanced/formatting?view=aspnetcore-2.2>

当Web API控制器操作返回结果时，可以对结果进行格式设置。



## 特定于格式的操作结果

这类操作结果的类型特定于特殊格式，例如JsonResult和ContentResult。

> 返回非 IActionResult 类型对象的操作结果将使用相应的 IOutputFormatter 实现来进行序列化。
>
> 对于有多个返回类型或选项的重要操作（例如基于所执行操作的结果的不同 HTTP 状态代码），请首选 IActionResult 作为返回类型。



## 内容协商（Content Negotiation）

当客户端指定 [Accept 标头](https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html)时，会发生内容协商（简称 conneg）。

ASP.NET Core MVC 使用的默认格式是 JSON。 

内容协商由 ObjectResult 实现，并且还内置于从帮助程序方法（全部基于 ObjectResult）返回的特定于状态代码的操作结果中。 还可以返回一个模型类型（已定义为数据传输类型的类），框架将自动将其打包在 ObjectResult 中。

例如，下述代码中使用了OK()和NotFound()帮助程序方法：

```c#
[HttpGet("Search")]
public IActionResult Search(string namelike)
{
    if (_context.TodoItems.Any(a => a.Name == namelike))
    {
        return Ok();
    }
    return NotFound(namelike);
}
```

这两个方法都将返回JSON格式的响应。

 默认情况下，ASP.NET Core MVC 仅支持 JSON。所以，即使指定另一种格式，返回的结果仍然是 JSON 格式。（可以通过配置格式化程序改变这一行为，但是大多数情况下，不需要进行格式化配置）

即使控制器操作返回的是一个普通的类对象（例如Student），在这种情况下，ASP.NET Core MVC 将自动创建打包对象的 ObjectResult。 客户端将获取设有格式的序列化对象（默认为 JSON 格式，可以配置 XML 或其他格式）。 如果返回的对象为 null，那么框架将返回 204 No Content 响应。

例如：

```c#
public Student Get(string stuNo)
{
    return GetByStudent(stuNo);
}
```

上述代码中，如果获取到有效的Student，将会返回“200 正常”响应，如果获取的是null，将会返回“204 无内容”响应。

注意：该示例更加说明了不同的HTTP状态码特定于不同的场景，实际中应该结合相应的场景，使用特定于此场景的具有语义的HTTP状态码对应的返回IActionResult的方法。

### 内容协商过程

> 内容协商仅在 `Accept` 标头出现在请求中时发生。 请求包含 accept 标头时，框架会以最佳顺序枚举 accept 标头中的媒体类型，并且尝试查找可以生成一种由 accept 标头指定格式的响应的格式化程序。 如果未找到可以满足客户端请求的格式化程序，框架将尝试找到第一个可以生成响应的格式化程序（除非开发人员配置 `MvcOptions` 上的选项以返回“406 不可接受”）。 如果请求指定 XML，但是未配置 XML 格式化程序，那么将使用 JSON 格式化程序。 一般来说，如果没有配置可以提供所请求格式的格式化程序，那么使用第一个可以设置对象格式的格式化程序。 如果不提供任何标头，则将使用第一个可以处理要返回的对象的格式化程序来序列化响应。 在此情况下，没有任何协商发生 - 服务器确定将使用的格式。
>
> 说明：
>
> 如果 Accept 标头包含 `*/*`，则将忽略该标头，除非 `RespectBrowserAcceptHeader` 在 `MvcOptions` 上设置为 true。

### 浏览器和内容协商

>不同于传统 API 客户端，Web 浏览器倾向于提供包括各种格式（含通配符）的 `Accept` 标头。 默认情况下，当框架检测到请求来自浏览器时，它将忽略 `Accept` 标头转而以应用程序的配置默认格式（JSON，除非有其他配置）返回内容。 这在使用不同浏览器使用 API 时提供更一致的体验。
>
>如果首选应用程序服从浏览器 accept 标头，则可以将此配置为 MVC 配置的一部分，方法是在 Startup.cs 中以 `ConfigureServices`方法将 `RespectBrowserAcceptHeader` 设置为 `true`。
>
>```c#
>services.AddMvc(options =>
>{
>    options.RespectBrowserAcceptHeader = true; // 默认为false
>});
>```



## 配置格式化程序

如果应用程序需要支持默认 JSON 格式以外的其他格式，那么可以添加 NuGet 包并配置 MVC 来支持它们。

 输入和输出的格式化程序不同：输入格式化程序由模型绑定使用；输出格式化程序用来设置响应格式。 

### 添加 XML 格式支持

若要添加对 XML 格式的支持，请安装 Microsoft.AspNetCore.Mvc.Formatters.Xml NuGet 包。

将 XmlSerializerFormatters 添加到 Startup.cs 中 MVC 的配置：

```c#
services.AddMvc()
    .AddXmlSerializerFormatters();
```

或者，可以仅添加输出格式化程序：

```c#
services.AddMvc(options =>
{
    options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
});
```

这两个方法将使用 System.Xml.Serialization.XmlSerializer 来序列化结果。 如果愿意，可以通过添加相关联的格式化程序使用 System.Runtime.Serialization.DataContractSerializer：

```c#
services.AddMvc(options =>
{
    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
});
```

### 强制执行特定格式

如果需要限制特定操作的响应格式，那么可以应用 [Produces] 筛选器。 [Produces] 筛选器指定特定操作（或控制器）的响应格式。 如同大多筛选器，这可以在操作层面、控制器层面或全局范围内应用。

```c#
[Produces("application/json")]
public class AuthorsController
```

[Produces] 筛选器将强制 AuthorsController 中的所有操作返回 JSON 格式的响应，即使已经为应用程序配置其他格式化程序且客户端提供了请求其他可用格式的 Accept 标头也是如此。 

### 特例格式化程序

一些特例是使用内置格式化程序实现的。 默认情况下，`string` 返回类型的格式将设为 text/plain（如果通过 `Accept` 标头请求则为 text/html）。可以通过删除 TextOutputFormatter 删除此行为。 有模型对象返回类型的操作将在返回 `null` 时返回“204 无内容”响应。 可以通过删除 `HttpNoContentOutputFormatter` 删除此行为。（具体见下述示例）

在 Startup.cs 中通过 Configure 方法删除格式化程序，以下代码删除 TextOutputFormatter 和 HttpNoContentOutputFormatter：

```c#
services.AddMvc(options =>
{
    options.OutputFormatters.RemoveType<TextOutputFormatter>();
    options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
});
```

没有 `TextOutputFormatter` 时，`string` 返回类型将返回“406 不可接受”等内容。 请注意，如果 XML 格式化程序存在，当删除 `TextOutputFormatter` 时，它将设置 `string` 返回类型的格式。

没有 `HttpNoContentOutputFormatter`，null 对象将使用配置的格式化程序来进行格式设置。 例如，JSON 格式化程序仅返回正文为 `null` 的响应，而 XML 格式化程序将返回属性设为 `xsi:nil="true"` 的空 XML 元素。



## 响应格式URL文件扩展名映射

客户端可要求特定格式作为 URL 一部分，例如在查询字符串中或在路径的一部分中，或者通过使用特定格式的文件扩展名（例如 .xml 或 .json）。 请求路径的映射必须在 API 使用的路由中指定。 例如:

```c#
[FormatFilter]
public class ProductsController
{
    [Route("[controller]/[action]/{id}.{format?}")]
    public Product GetById(int id)
```

此路由将允许所所请求格式指定为可选文件扩展名。 `[FormatFilter]` 属性检查 `RouteData` 中格式值是否存在，并在响应创建时将响应格式映射到相应格式化程序。

| 路由                       | 格式化程序                |
| :------------------------- | :------------------------ |
| `/products/GetById/5`      | 默认输出格式化程序        |
| `/products/GetById/5.json` | JSON 格式化程序（如配置） |
| `/products/GetById/5.xml`  | XML 格式化程序（如配置）  |

