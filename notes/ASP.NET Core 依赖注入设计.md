# ASP.NET Core 依赖注入设计

本文主要对ASP.NET Core中的依赖注入框架进行介绍，以及实际设计使用中需要注意的事项进行说明。



## 依赖注入基础结构

DI是一种广泛使用的开发模式，用于使应用程序中的任何地方都可以使用服务。每当代码组件（例如类）需要引用某些外部代码（例如服务）时，您有两个选择。

- 首先，在调用代码中创建服务组件的新实例。
- 其次，你希望接收到其他人将为你创建的服务的有效实例。

下面将使用一个示例进行详细说明。

### 重构以隔离依赖关系

假设您有一个类，它充当外部功能(如日志记录器)的包装器。在下面的代码中，类与特性的特定实现紧密耦合。

```c#
public class BusinessTask
{
   public void Perform()
   {
      var logger = new Logger();

  	    ...

      logger.Log("Done");
   }
}
```

如果您移动类，那么只有当引用的组件及其所有依赖项也被移动时，它才会工作。例如，如果Logger使用数据库，那么在使用示例`BusinessTask`类的任何地方都必须能够连接到数据库。

#### 将应用程序代码与依赖项分离

面向对象设计的一个古老而明智的原则是，您应该始终按照接口编程，而不是按照实现编程。应用于前面的代码，这个原则意味着我们将从logger组件中提取一个接口，并将对它的引用注入到`BusinessTask`类中。

```c#
public class BusinessTask
{
   private ILogger _logger;
   public BusinessTask(ILogger logger)
   {
       _logger = logger;
   }

   public void Perform()
   {
      ...
      
      _logger.Log("Done");

   }
}
```

上述代码，提取到`ILogger`接口的记录器功能现在通过构造函数注入。

这是依赖注入的一种基本形式，有时也被称为“穷人的依赖注入”(poor man’s dependency injection)，只是为了强调它的最基本的、但却是功能性的实现。

#### DI框架介绍

用于接收外部依赖的类将创建所有必要实例的负担转移到调用代码。但是，如果广泛使用DI模式，那么在获得要注入的实例之前要编写的代码量可能非常大。例如，business类依赖于记录器，而记录器依赖于数据源提供程序。反过来，数据源提供程序可能具有另一个依赖项等等。

为了减轻类似情况的负担，您可以使用DI框架，它使用反射，或者更有可能使用动态编译的代码来返回所需的实例，只需一行代码即可。 DI框架有时被称为控制反转(IoC)框架。

```c#
var logger = SomeFrameworkIoC.Resolve(typeof(ILogger));
```

DI框架本质上是通过将抽象类型（通常是接口）映射到具体类型来工作的。当以编程方式请求已知的抽象类型时，框架就会创建并返回映射的具体类型的实例。注意，DI框架的根对象通常称为容器。

#### 服务定位器（Service Locator）模式

依赖注入并不是以松散耦合的方式调用外部依赖的唯一合理模式。另一种模式称为服务定位器。以下是重写上一个示例类以使用服务定位器的方法。

```c#
public class BusinessTask
{
   public void Perform()
   {
      ...
	  //获取引用
      var logger = ServiceLocator.GetService(typeof(ILogger));
      logger.Log("Done");
   }
}
```

`ServiceLocator`伪类表示一些能够为指定的抽象类型创建匹配实例的基础结构。 DI和服务定位器之间的主要区别在于，DI需要相应地设计周围的代码，构造函数和其他方法的签名可能会更改。 服务定位器更保守，但它也导致代码可读性较差，因为开发人员需要研究整个源代码以确定依赖关系。同时，当您在重构大型现有代码库中的依赖关系时，服务定位器是一个理想的选择。

在ASP.NET Core中，服务定位器的角色由HTTP上下文中的`RequestServices`对象扮演。下面是一些示例代码。

```c#
public void Perform()
{
    ...

    var logger = HttpContext.RequestServices.GetService<ILogger>();
    logger.Log("Done");
}
```

请注意，示例代码假设是控制器类的一部分，因此，`HttpContext`是基类`Controller`类的属性。



## ASP.NET Core DI系统的概述

ASP.NET Core自带的DI框架在应用程序启动时被初始化。

#### 预定义的依赖关系

当容器可用于应用程序代码时，它已包含一些已配置的依赖项，如表中所示。

ASP.NET Core DI系统中默认映射的抽象类型：

| 抽象类型            | 描述                                          |
| ------------------- | --------------------------------------------- |
| IApplicationBuilder | 该类型提供了配置应用程序请求管道的机制        |
| ILoggerFactory      | 该类型提供了用于创建记录器组件的模式          |
| IHostingEnvironment | 该类型提供有关运行应用程序的Web托管环境的信息 |

在ASP.NET Core应用程序中，您可以将上述任何一种类型注入任何有效的代码注入点，而无需进行任何初始配置。但是，为了能够注射任何其他类型，您必须首先完成注册步骤。

#### 注册自定义依赖关系

您可以使用ASP.NET Core DI系统以两种非独占方式之一注册类型。注册类型包括让系统知道如何将抽象类型解析为具体类型。映射可以静态设置或动态确定。静态映射通常发生在`Startup`类的`ConfigureServices`方法中。

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        //将具体类型CustomerService绑定到ICustomerService接口
        services.AddTransient<ICustomerService, CustomerService>();
    }
}
```

可以使用DI系统定义的`AddXxx`扩展方法之一来绑定类型。 DI的`AddXxx`扩展方法在`IServiceCollection`接口上定义。上面代码的最终效果是，每当请求实现`ICustomerService`的类型的实例时，系统就会返回`CustomerService`的实例。特别是，`AddTransient`方法确保每次都返回一个全新的`CustomerService`类型实例。然而，还有其他的生命周期选项。

抽象类型的静态解析有时是有限制的。实际上，如果您需要根据运行时条件将类型T解析为不同类型，此时，就需要使用动态解析。动态解析允许指定一个回调函数来解决依赖关系。

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<ICustomerService>(provider =>
    {
        //逻辑将放在这里，以决定如何解决ICustomerService。
        if (SomeRuntimeConditionHolds())
           return new CustomerServiceMatchingRuntimeCondition();
        else 
           return new DefaultCustomerService();
    });
}
```

实际上，您需要传递一些运行时数据来评估条件。要从回调函数中检索HTTP上下文，可以使用服务定位器API。

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<ICustomerService>(provider =>
    {
        var context = provider.GetRequiredService<IHttpContextAccessor>();
        if (SomeRuntimeConditionHolds(context.HttpContext.User))
           return new CustomerServiceMatchingRuntimeCondition();
        else ...    
    });
}
```

必须调用`IServiceCollection`的`AddXxx`扩展方法之一，将任何类型添加到DI系统，以及将任何系统抽象类型绑定到不同的实现。

#### 依赖的生命周期

在ASP.NET Core中，有几种不同的方法可以向DI系统请求映射的具体类型的实例。

创建实例的生命周期选项：

| 方法         | 描述                                                         |
| ------------ | ------------------------------------------------------------ |
| AddTransient | 调用者每次调用都会接收到指定类型的新实例                     |
| AddSingleton | 调用者接收到第一次创建的指定类型的相同实例。无论何种类型，每个应用程序都有自己的实例 |
| AddScoped    | 与AddSingleton相同，但它的作用域是当前请求                   |

注意，通过简单地使用`AddSingleton`方法的备用重载，您还可以为任何后续调用指示要返回的特定实例。当您需要将返回的对象配置为某个状态时，此方法很有用。

```c#
public void ConfigureServices(IServiceCollection services)
{
    // Singleton
    services.AddSingleton<ICustomerService, CustomerService>();
    
    // Custom instance
    var instance = new CustomerService();
    instance.SomeProperty = ...;
    services.AddSingleton<ICustomerService>(instance);            
}
```

在本例中，首先创建实例并将其存储在任意状态，然后将其传递给`AddSingleton`。

重要的是，注意：在给定生命周期内注册的任何组件都不能依赖于使用较短生命周期注册的其他组件。换句话说，应该避免向单例对象中注入一个使用临时的或作用域生命周期注册的组件。如果这样做，您可能会遇到应用程序不一致的情况，因为对单例的依赖会使临时（或作用域）实例的生命周期远远超出其预期生命周期。这可能不一定会导致应用程序中出现明显可见的错误，但是存在单例对象正在处理错误对象（就应用程序而言）的风险。通常，当链接对象的生命周期不一样时 ，就会出现问题。

#### 连接到外部DI框架

ASP.NET Core中的DI系统是根据ASP.NET的需求量身定制的，因此它可能无法提供您在另一个DI框架中熟悉的所有特性和功能。 ASP.NET Core的优点在于它允许您插入任何外部DI框架，前提是框架已移植到.NET Core并且存在一个连接器。下面的代码展示了如何做到这一点。

```c#
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // 配置ASP.NET Core本机DI系统
    services.AddTransient<ICustomerService, CustomerService>();

    ...

    // 在外部DI框架中导入现有的映射
    var builder = new ContainerBuilder();  
    builder.Populate(services);
    var container = builder.Build();

    // 替换管道其余部分的服务提供者
    return container.Resolve<IServiceProvider>();
}
```

当你计划在应用程序中使用外部DI框架时，首先要做的是更改`Startup`类中的`ConfigureServices`方法的签名。该方法必须返回`IServiceProvider`，而不是`void`。在上面的代码中，类`ContainerBuilder`是我们试图插入的特定DI框架的连接器（例如，Autofac）。方法`Populate`填充Autofac内部所有挂起的类型映射，然后使用Autofac框架解决对`IServiceProvider`的根依赖关系。这是管道中所有其它部分将在内部使用的接口，用于解决依赖的关系。

### DI容器

在ASP.NET Core中，如果要求实例化尚未注册的类型，DI容器将返回`null`。如果为同一抽象类型注册了多个具体类型，那么DI容器将返回上一次注册类型的实例。如果由于歧义不确定性或参数不兼容而无法解析构造函数，那么DI容器会抛出异常。

对于需要处理的复杂场景，可以通过编程方式检索为给定抽象类型注册的所有具体类型。列表由`IServiceProvider`接口上定义的`GetServices <TAbstract>`方法返回。最后，一些流行的DI框架允许开发人员根据键或条件注册类型。 ASP.NET Core不支持这种场景。如果这种需求在你的应用程序中至关重要，你可能需要考虑为所涉及的类型创建专用的工厂类。

### 在层中注入数据和服务

在DI系统中注册了一个服务之后，您需要做的就是在必要的位置请求一个实例。在ASP.NET Core中，可以在控制器和视图中通过`Configure`方法和中间件类将服务注入到管道。

#### 注入技术

将服务注入组件的主要方法是通过组件的构造函数。中间件类、控制器和视图总是通过DI系统实例化，随后，将自动解析签名中列出的任何附加参数。

除了构造函数注入之外，在控制器类中，还可以利用`FromServices`属性来获取实例，最后还可以使用Service Locator接口，一般当需要检查运行时条件以正确解析依赖关系时，可以使用Service Locator接口。

#### 在管道中注入服务

可以将服务注入ASP.NET Core应用程序的启动类中，只不过此时只能继续使用构造函数注入，并且仅适用于上述第一个表中列出的类型。

```c#
// Constructor injection
public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
{
   // Initialize the application 
   ...
}
```

接下来，当你继续使用处理请求之前和之后的组件配置管道时，您可以通过中间件类的构造函数注入依赖项，或者可以使用服务定位器方法。

```c#
app.Use((context, next) =>
{
    var service = context.RequestServices.GetService<ICustomerService>();
    ...
    next();
    ...
});
```

#### 向控制器注入服务

在MVC应用程序模型内部，服务注入主要通过控制器类的构造函数实现的。

下面是一个示例控制器：

```c#
public class CustomerController : Controller
{
    private readonly ICustomerService _service;
    // Service injection
    public CustomerController(ICustomerService service)
    {
        _service = service;
    }
    ...
}
```

此外，还可以覆盖模型绑定机制，将方法参数映射到成员。

```c#
public IActionResult Index(
       [FromServices] ICustomerService service)
{
   ...    
}
```

`FromServices`属性使DI系统创建并返回与`ICustomerService`接口关联的具体类型的实例。最后，在控制器方法的主体中，始终可以引用HTTP上下文对象及其`RequestServices`对象来使用服务定位器API（Service Locator API）。

#### 将服务注入视图

可以在Razor视图中使用`@inject`指令，强制DI系统返回指定类型的实例并将其绑定到给定属性。

```c#
@inject ICustomerService Service
```

上面一行的最终效果是，在Razor视图中提供了一个名为“`Service`”的属性，该属性已被设置为`ICustomerService`类型的DI解析实例。分配的实例的生命周期将取决于DI容器中`ICustomerService`类型的配置。