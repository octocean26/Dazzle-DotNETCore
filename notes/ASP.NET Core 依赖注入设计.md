# ASP.NET Core 依赖注入设计

## 依赖注入基础设施

DI是一种广泛用于使应用程序中的代码可用于服务的开发模式。每当代码组件（例如类）需要引用某些外部代码（例如服务）时，您有两个选择。

- 首先，在调用代码中创建服务组件的新实例。
- 其次，您希望收到其他人为您创建的有效服务实例。我们来看一个说明性的例子。

### 重构以隔离依赖关系

假设您有一个类作为外部功能的包装器，例如记录器。在下面的代码中，该类与该功能的特定实现紧密结合。

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

如果您移动类，它将仅在引用的组件及其所有依赖项也被移动时起作用。例如，如果记录器使用数据库，那么在使用示例业务类的任何地方都必须能够连接到数据库。

### 将应用程序代码与依赖项分离

面向对象设计的一个古老而明智的原则是，您应该始终编程接口而不是实现。应用于以前的代码，这个原则意味着我们将从记录器组件中提取一个接口，并将对它的引用注入到业务类中。

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

现在，通过构造函数注入了抽象到ILogger接口的记录器功能。从这里，两个主要事实下降。

- 首先，实例化记录器的负担已经移到了业务类之外。
- 其次，业务类现在可以透明地使用任何实现给定接口的今天和明天的类。

这是依赖注入的一种基本形式，有时也被称为穷人的依赖注入，只是为了强调它的最小但功能实现。

#### 介绍DI框架

设计用于接收外部依赖项的类将创建所有必需实例的负担转移到调用代码。但是，如果广泛使用DI模式，则在获取实例注入之前要编写的代码量可能很大。例如，业务类依赖于记录器，而记录器依赖于数据源提供程序。反过来，数据源提供程序可能具有另一个依赖项等等。

为了减轻类似情况的负担，您可以使用DI框架，该框架使用反射或更可能的动态编译代码来返回所需的实例，只需一行代码即可。 DI框架有时被称为控制反转（IoC）框架。

```c#
var logger = SomeFrameworkIoC.Resolve(typeof(ILogger));
```

DI框架基本上通过将抽象类型（通常是接口）映射到具体类型来工作。每当以编程方式请求出现已知的抽象类型时，框架就会创建并返回映射的具体类型的实例。请注意，DI框架的根对象通常称为容器。

#### 服务定位器模式

依赖注入不是以松散耦合方式调用外部依赖关系的唯一可能模式。另一种模式称为服务定位器。以下是检索上一个示例类以使用Service Locator的方法。

```c#
public class BusinessTask
{
   public void Perform()
   {
      ...

      var logger = ServiceLocator.GetService(typeof(ILogger));
      logger.Log("Done");
   }
}
```

ServiceLocator伪类表示一些能够为指定的抽象类型创建匹配实例的基础结构。 DI和服务定位器之间的主要区别在于DI需要相应地设计周围的代码;构造函数和其他方法的签名可能会更改。 Service Locator更保守，但它也导致代码不太可读，因为开发人员需要调查整个源代码以找出依赖关系。同时，当您在大型现有代码库中重构依赖关系时，Service Locator是一个理想的选择。

在ASP.NET Core中，服务定位器的角色由HTTP上下文中的RequestServices对象扮演。这是一些示例代码。

```c#
public void Perform()
{

    ...

    var logger = HttpContext.RequestServices.GetService<ILogger>();
    logger.Log("Done");
}
```

请注意，假定示例代码是控制器类的一部分;因此，HttpContext意味着是基类Controller类的属性。

### ASP.NET核心DI系统的一般性

ASP.NET Core附带了自己的DI框架，可以在应用程序启动时进行初始化。让我们来看看它最具特色的点。

#### 预定义的依赖关系

当容器可用于应用程序代码时，它已包含一些已配置的依赖项，如表中所示。

表7-1 ASP.NET Core DI系统中默认映射的抽象类型

| 抽象类型            | 描述                                          |
| ------------------- | --------------------------------------------- |
| IApplicationBuilder | 该类型提供了配置应用程序请求管道的机制        |
| ILoggerFactory      | 该类型提供了用于创建记录器组件的模式          |
| IHostingEnvironment | 该类型提供有关运行应用程序的Web托管环境的信息 |

在ASP.NET Core应用程序中，您可以将任何上述类型注入任何有效的代码注入点，而无需任何初步配置。 （更多关于注射点的信息。）但是，为了能够注射任何其他类型，您必须首先完成注册步骤。

#### 注册自定义依赖项

您可以使用ASP.NET Core DI系统以两种非独占方式之一注册类型。注册类型包括让系统知道如何将抽象类型解析为具体类型。可以动态地静态设置或确定映射。静态映射通常发生在启动类的ConfigureServices方法中。

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<ICustomerService, CustomerService>();
    }
}
```

您可以使用DI系统定义的AddXxx扩展方法之一来绑定类型。 DI的AddXxx扩展方法在IServiceCollection接口上定义。上面代码的净效果是，只要请求实现ICustomerService的类型的实例，系统就会返回CustomerService的实例。特别是，AddTransient方法确保每次都返回一个全新的CustomerService类型实例。但是，存在其他寿命选项。

抽象类型的静态解析有时是限制性的。实际上，如果您需要根据运行时条件将类型T解析为不同类型，该怎么办？这就是动态分辨率发挥作用的地方;动态分辨率允许您指示回调函数来解决依赖关系。

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<ICustomerService>(provider =>
    {
        if (SomeRuntimeConditionHolds())
           return new CustomerServiceMatchingRuntimeCondition();
        else 
           return new DefaultCustomerService();
    });
}
```

实际上，您需要传递一些运行时数据来评估条件。要从回调函数中检索HTTP上下文，请使用服务定位器API。

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

注意您必须调用IServiceCollection的AddXxx扩展方法之一，将任何类型添加到DI系统，以及将任何系统抽象类型绑定到不同的实现。

#### 依赖的生命周期

在ASP.NET Core中，有几种不同的方法可以向DI系统请求映射的具体类型的实例。表7-2列出了所有这些。

表7-2 DI创建的实例的生命周期选项

| 方法         | 描述                                                         |
| ------------ | ------------------------------------------------------------ |
| AddTransient | 调用者每次调用都会收到指定类型的新实例                       |
| AddSingleton | 调用者接收第一次创建的指定类型的相同实例。无论何种类型，每个应用程序都有自己的实例 |
| AddScoped    | 与AddSingleton相同，但它的作用域是当前请求                   |

请注意，通过简单地使用AddSingleton方法的备用重载，您还可以指示要为任何连续调用返回的特定实例。当您需要将返回的对象配置为某个状态时，此方法很有用。

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

在这种情况下，首先创建实例并在其中存储您希望的任何状态，然后将其传递给AddSingleton。

重要注意，注意在给定生命周期内注册的任何组件不能依赖于使用较短生命周期注册的其他组件。换句话说，您应该避免将注册了瞬态或作用域生命周期的组件注入单例。如果这样做，您可能会遇到应用程序不一致，因为对单例的依赖会使瞬态（或作用域）实例远远超出其预期生命周期。这可能不一定会导致应用程序中出现明显的错误，但是存在单个人正在处理错误对象（就应用程序而言）的风险。通常，只要链式对象的生命周期不同，就会出现问题。

#### 连接到外部DI框架

ASP.NET Core中的DI系统是根据ASP.NET的需求量身定制的，因此它可能无法提供您在另一个DI框架中熟悉的所有特性和功能。 ASP.NET Core的优点在于它允许您插入任何外部DI框架，前提是框架已移植到.NET Core并且存在连接器。以下代码显示了如何执行此操作。

```c#
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // Configure the ASP.NET Core native DI system
    services.AddTransient<ICustomerService, CustomerService>();

    ...


    // Import existing mappings in the external DI framework 
    var builder = new ContainerBuilder();  
    builder.Populate(services);
    var container = builder.Build();

    // Replace the service provider for the rest of the pipeline to use
    return container.Resolve<IServiceProvider>();
}
```

当您计划在应用程序中使用外部DI框架时，您应该做的第一件事是更改启动类中的ConfigureServices方法的签名。该方法必须返回IServiceProvider，而不是void。在上面的代码中，类ContainerBuilder是我们尝试插入的特定DI框架的连接器（例如，Autofac）。方法Populate导入Autofac内部的所有挂起类型映射，然后Autofac框架用于解析IServiceProvider上的根依赖。这是管道的所有其余部分将在内部使用以解决依赖关系的接口。

### DI容器的方面

在ASP.NET Core中，如果要求实例化尚未注册的类型，DI容器将返回null。如果已为同一抽象类型注册了多个具体类型，则DI容器将返回上次注册类型的实例。如果由于歧义或参数不兼容而无法解析构造函数，则DI容器会抛出异常。

如果要处理复杂的场景，您可以以编程方式检索为给定抽象类型注册的所有具体类型。该列表由IServiceProvider接口上定义的GetServices <TAbstract>方法返回。最后，一些流行的DI框架允许开发人员根据键或条件注册一个类型。 ASP.NET Core不支持此方案。如果该功能在您的应用程序中至关重要，您可能需要考虑为所涉及的类型创建专用工厂类。

### 在图层中注入数据和服务

一旦在DI系统中注册了服务，您只需在必要的位置请求实例即可。在ASP.NET Core中，您可以通过控制器和视图中的Configure方法和中间件类将服务注入管道。

#### 注入技术

将服务注入组件的主要方法是通过其构造函数。中间件类，控制器和视图始终通过DI系统实例化，随后，将自动解析签名中列出的任何其他参数。

除了构造函数注入之外，在控制器类中，您可以利用FromServices属性来获取实例，最后但并非最不重要的是，使用Service Locator接口。请注意，当您需要检查运行时条件以正确解析依赖关系时，将使用Service Locator接口。

#### 在管道中注入服务

您可以将服务注入ASP.NET Core应用程序的启动类。但是，此时，您只能继续构造函数注入，并且仅适用于表7-1中列出的类型。

```c#
// Constructor injection
public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
{
   // Initialize the application 
   ...
}
```

接下来，当您继续使用处理请求之前和之后的组件配置管道时，您可以通过中间件类的构造函数（如果使用任何内容）注入依赖项，或者可以使用服务定位器方法。

```c#
app.Use((context, next) =>
{
    var service = context.RequestServices.GetService<ICustomerService>();
    ...
    next();
    ...
});
```

#### 将服务注入控制器

在MVC应用程序模型内部，服务注入主要通过控制器类的构造函数发生。这是一个示例控制器。

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

此外，您可以覆盖模型绑定机制以将方法参数映射到成员。

```c#
public IActionResult Index(
       [FromServices] ICustomerService service)
{
   ...    
}
```

FromServices属性使DI系统创建并返回与ICustomerService接口关联的具体类型的实例。最后，在控制器方法的主体中，您始终可以引用HTTP上下文对象及其RequestServices对象来使用Service Locator API。

#### 将服务注入视图

如第5章“ASP.NET MVC视图”所示，可以在Razor视图中使用@inject指令强制DI系统返回指定类型的实例并将其绑定到给定属性。

```c#
@inject ICustomerService Service
```

上面一行的净效果是在Razor视图中提供了一个名为“Service”的属性，该属性已设置为ICustomerService类型的DI解析实例。分配的实例的生命周期取决于DI容器中ICustomerService类型的配置。