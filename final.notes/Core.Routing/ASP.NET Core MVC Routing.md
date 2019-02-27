# ASP.NET Core MVC 路由

**本文适用范围**

本文所介绍的内容只是基于ASP.NET Core的MVC框架的应用，和ASP.NET Core中的Razor Pages路由有一定的差异。

**本文中的术语说明**

- 路由：本文中的路由特指的是ASP.NET Core MVC中的路由，也可以理解为控制器路由。
- 路由模板：类似于"{controller=Home}/{action=Index}/{id?}"形式的字符串，对于需要指定模板（template）参数的方法，会使用路由模板。
- 路由参数/路由参数名称：即路由模板中的“controller”、“action”、“id”等，除了系统保留的路由参数名称，也可以自定义路由参数名称（如上述中的“id”）。
- 路由名称：即路由模板中的“default”，通常作为方法的name参数进行传入，具体见本文介绍。
- 路由集合/路由表：
- 



## MVC路由

ASP.NET Core MVC使用路由中间件来匹配传入请求的 URL 并将它们映射到操作。可以在启动代码或属性中定义路由，用来描述如何将URL路径与控制器的操作方法相匹配。

控制器操作方法既支持传统路由，也支持属性路由，同时支持这两种形式的混合路由。



## 设置路由中间件

在Startup类中的ConfigureServices()方法中，添加MVC服务，并在Configure()方法中设置路由中间件，代码如下：

```c#
app.UseMvc(routes =>
{
   routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
});
```

上述代码中，routes.MapRoute()方法用于创建单个路由（default路由），"{controller=Home}/{action=Index}/{id?}"被称为路由模板，可以匹配诸如 /Products/Details/5 之类的 URL 路径，并通过对路径进行标记来提取路由值 { controller = Products, action = Details, id = 5 }。 MVC 将尝试查找名为 ProductsController 的控制器并运行 Details 操作方法。

除了使用上述代码设置路由中间件外，还可以直接使用UseMvcWithDefaultRoute()方法替换上述代码，如下所示：

```c#
app.UseMvcWithDefaultRoute();
```

UseMvc方法 和 UseMvcWithDefaultRoute方法都可以向中间件管道添加 RouterMiddleware 的实例。MVC 不直接与中间件交互，而是使用路由来处理请求，MVC 通过 MvcRouteHandler 实例连接到路由。

UseMvc()方法无论怎样配置，都和UseMvcWithDefaultRoute()一样，始终都支持属性路由。



## 传统路由

传统路由也被称为默认路由，它的定义如下：

```c#
routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
```

使用传统路由时，URL路径映射仅仅基于控制器和相关的操作名称，而不能基于命名空间、源文件位置或方法参数。



## 多个路由

通过添加对 MapRoute 的多次调用，可以在 UseMvc 内添加多个路由。 这样做可以定义多个约定，或添加专用于特定操作的传统路由，例如：

```c#
app.UseMvc(routes =>
{
   routes.MapRoute("blog", "blog/{*article}",
            defaults: new { controller = "Blog", action = "Article" });
   routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
});
```

对于第一个blog路由模板，由于controller和action不会在路由模板中作为路由参数显示，它们只能有默认值，因此该路由将始终映射到BlogController.Article操作方法。

路由集合中的路由会进行排序，并按添加顺序进行处理。 因此，在此示例中，将先尝试 blog 路由，再尝试default 路由。

### 路由名称

在上述代码中，routes.MapRoute()重载方法会传入一个name参数，即代码中的“blog”字符串和“default”字符串，这些都是路由名称。

路由名称为路由提供一个逻辑名称，以便使用命名路由来生成 URL。 路由排序会使 URL 生成复杂化，而这极大地简化了 URL 创建。路由名称必须在应用程序范围内是唯一的。

路由名称不影响请求的 URL 匹配或处理；它们仅用于 URL 生成。



## 属性路由——Route

属性路由使用一组Route属性将控制器操作方法直接映射到路由模板。 

```c#
public class HomeController : Controller
{
   [Route("")]
   [Route("Home")]
   [Route("Home/Index")]
   public IActionResult Index()
   {
      return View();
   }
   [Route("Home/About")]
   public IActionResult About()
   {
      return View();
   }
   [Route("Home/Contact")]
   public IActionResult Contact()
   {
      return View();
   }
}
```

与传统路由相比，属性路由需要精确控制应用于每项操作方法的路由模板。使用属性路由时，控制器名称和操作方法名称对于匹配的选择没有影响。



## 属性路由——Http[Verb]（Http谓词属性）

常见的Http谓词属性有HttpPost、HttpGet等：

```c#
[HttpGet("/products")]
public IActionResult ListProducts()
{
   // ...
}

[HttpPost("/products")]
public IActionResult CreateProduct(...)
{
   // ...
}
```

上述代码中，对于诸如 /products 之类的 URL 路径，当 Http 谓词为 GET 时将执行 ProductsApi.ListProducts 操作，当 Http 谓词为 POST 时将执行 ProductsApi.CreateProduct。 

**提示**：与[Route(...)]相比，应该优先使用HTTP谓词属性路由，尤其是对于REST API应用，更是如此。

下述示例演示了如何在路由模板中定义路由参数id，并在操作方法中，获取id参数的值：

```c#
public class ProductsApiController : Controller
{
   [HttpGet("/products/{id}", Name = "Products_List")]
   public IActionResult GetProduct(int id) { ... }
}
```

### HTTP谓词属性路由中的路由名称

```c#
[HttpGet("/products/{id}", Name = "Products_List")]
```

第二个参数Name即为路由名称，这里的路由名称为”Products_List“，和上文介绍的路由名称一样，这里的路由名称也不影响路由的URL匹配行为，仅用于生成URL，路由名称必须在应用程序范围内唯一。

### 

## 合并路由

















