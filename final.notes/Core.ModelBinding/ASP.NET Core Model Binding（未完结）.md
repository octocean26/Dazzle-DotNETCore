# ASP.NET Core中的模型绑定

ASP.NET Core MVC 中的模型绑定指的是将 HTTP 请求中的数据映射到操作方法参数。 

当 MVC 收到 HTTP 请求时，它会将此请求路由到控制器的特定操作方法。然后将 HTTP 请求中的值绑定到该操作方法的参数。至于要运行哪个操作方法，取决于路由数据中的内容。

模型绑定的先后顺序如下：

1. Form Value：使用Post方式提交的值，包括JQuery Post请求。
2. Route Values：基于路由的路由值。
3. Query String：URL查询字符串部分。

> 如果操作方法的参数是一个属性为简单类型和复杂类型的类，如 Movie 类型，则 MVC 的模型绑定仍可对其进行妥善处理。 它使用反射和递归来遍历查找匹配项的复杂类型的属性。 模型绑定查找模式 parameter_name.property_name 以将值绑定到属性 。 如果未找到此窗体的匹配值，它将尝试仅使用属性名称进行绑定。
>
> 对于诸如 Collection 类型的类型，模型绑定将查找 parameter_name[index] 或仅 [index] 的匹配项。 
>
> 对于 Dictionary 类型，模型绑定的处理方法与之类似，即请求 parameter_name[key] 或仅 [key]（只要键是简单类型）。

若要实现模型绑定，该类必须具有要绑定的公共默认构造函数和公共可写属性。 发生模型绑定时，在使用公共默认构造函数对类进行实例化后才可设置属性。

如果绑定失败，MVC 不会引发错误。 接受用户输入的每个操作均应检查 ModelState.IsValid 属性。

注意：控制器的 `ModelState` 属性中的每个输入均为包含 `Errors` 属性的 `ModelStateEntry`。 很少需要自行查询此集合。 请改用 `ModelState.IsValid`。



## 通过特性自定义模型绑定行为

MVC 包含多种特性，可用于将其默认模型绑定行为定向到不同的源。 

- `[BindRequired]`：指定需要绑定的参数，如果无法发生绑定，此特性将添加模型状态错误。
- `[BindNever]`：指示模型绑定器从不绑定到此参数。
- `[FromHeader]`、`[FromQuery]`、`[FromRoute]`、`[FromForm]`：使用这些特性指定要应用的确切绑定源。
- `[FromServices]`：此特性使用依赖关系注入绑定服务中的参数。
- `[FromBody]`：使用配置的格式化程序绑定请求正文中的数据。 基于请求的内容类型，选择格式化程序。
- `[ModelBinder]`：用于替代默认模型绑定器、绑定源和名称。



## 全局配置模型绑定行为(不常用)

通过向 MvcOptions.ModelMetadataDetailsProviders 添加详细信息提供程序，可以全局配置系统行为的各个方面。 MVC 有一些内置的详细信息提供程序，可通过它们配置禁用模型绑定或验证某些类型等行为。

### 禁用对特定类型的所有模型的模型绑定

例如禁用对System.Version类型的所有模型的模型绑定，需要在Startup.ConfigureServices 中添加 ExcludeBindingMetadataProvider()方法的调用：

```c#
services.AddMvc().AddMvcOptions(options =>
    options.ModelMetadataDetailsProviders.Add(
        new ExcludeBindingMetadataProvider(typeof(System.Version))));
```

### 禁用对特定类型的属性的验证

例如，禁用对System.Guid类型的属性的验证，可以在Startup.ConfigureServices 中添加 SuppressChildValidationMetadataProvider()方法的调用：

```c#
services.AddMvc().AddMvcOptions(options =>
    options.ModelMetadataDetailsProviders.Add(
        new SuppressChildValidationMetadataProvider(typeof(System.Guid))));
```



## 绑定请求正文（FromBody）中的带格式数据

这里的“带格式数据”通常指的是JSON、XML 和许多其他格式的请求数据。

 使用 [FromBody] 特性绑定参数和请求数据时，MVC 会使用一组已配置的格式化程序，基于请求数据的内容类型对请求数据进行处理。 默认情况下，MVC 已经包括了用于处理 JSON 数据的 JsonInputFormatter 类（JsonInputFormatter 为默认格式化程序且基于 Json.NET）。

对于其他格式的数据，可以添加其他格式化程序。

除非有特性应用于 ASP.NET Core，否则它将基于 Content-Type 标头和参数类型来选择输入格式化程序。 

例如，如果想要使用 XML 或其他格式，则必须在 Startup.cs 文件中配置该格式：

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc()
        .AddXmlSerializerFormatters();
   }
```

 注：上述代码可能需要先使用 NuGet 获取对 Microsoft.AspNetCore.Mvc.Formatters.Xml 的引用，然后，将 Consumes 特性应用于控制器类或操作方法，以使用所需格式。



## 自定义模型绑定



