# ASP.NET Core依赖注入

依赖注入（Dependency Injection，简称DI）：DI是一种促进类之间松散耦合的设计原则。

## 依赖注入初始

首先使用一个示例来说明什么是依赖注入。

假如存在以下类：

```c#
public class Flag
{
}
//返回Flag对象
public class FlagRepository
{
    public Flag GetFlag(string country)
    {
        return new Flag();
    }
}
```

上述两个类没有特殊含义，只是为了说明依赖注入的演变过程。

下面定义一个新类，在未使用依赖注入之前，代码如下：

```c#
public class FlagService
{
    private FlagRepository _repository;

    public FlagService()
    {
        _repository = new FlagRepository();
    }

    public Flag GetFlagForCountry(string country)
    {
        return _repository.GetFlag(country);
    }
}
```

在该类中，FlagService类依赖于FlagRepository类，并且给定了两个类完成的任务，紧密的关系是不可避免的。DI原则有助于保持FlagService及其依赖关系之间的松散关系。 DI的核心思想是使FlagService仅依赖于FlagRepository提供的函数的抽象。 考虑到DI，可以按如下方式重写类：

```c#
public interface IFlagRepository
{
    Flag GetFlag(string country);
}

public class FlagRepository : IFlagRepository
{
    public Flag GetFlag(string country)
    {
       return new Flag();
    }
}

public class FlagService
{
    private IFlagRepository _repository;

    public FlagService(IFlagRepository repository)
    {
        _repository = repository;
    }

    public Flag GetFlagForCountry(string country)
    {
        return _repository.GetFlag(country);
    }
}
```

现在，任何实现IFlagRepository的类都可以安全地使用FlagService实例。 通过使用DI，我们将FlagService和FlagRepository之间的紧密依赖关系转换为FlagService与从外部导入所需服务的抽象之间的松散关系。 创建存储库抽象实例的责任已从服务类中移除。 这意味着其他一些代码现在负责引用一个接口（一个抽象）并返回一个具体类型（一个类）的可用实例。 每次需要时都可以手动编写此代码。

```c#
var repository = new FlagRepository();
var service = new FlagService(repository);
```

当然，你还可以对此语句进行封装，使其可以在其他代码层被调用，该代码层检查服务的构造函数并解析其所有依赖项。类似的调用语句如下：

```c#
var service = DependencyInjectionSubsystem.Resolve(FlagService);
```

ASP.NET Core带有自己的DI子系统，因此任何类（包括控制器）都可以在构造函数（或成员）中声明所有必需的依赖项; 系统将确保创建和传递有效实例。



## ASP.NET Core中的依赖注入

要使用DI系统，您需要注册 系统必须能够为您实例化的类型。 ASP.NET Core DI系统已经知道某些类型，例如IHostingEnvironment和ILoggerFactory，但它需要了解特定于应用程序的类型。

#### 使用DI系统注册类型

在ConfigureServices()方法中，添加要注册新类型的代码。

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IFlagRepository, FlagRepository>();
}
```

每次请求IFlagRepository接口之类的抽象时，方法AddTransient都会指示DI系统为FlagRepository类型提供一个全新的实例。有了这一行，任何实例化由ASP.NET Core管理的类都可以简单地声明IFlagRepository类型的参数，以便系统提供一个新的实例。例如，在控制器中利用DI得到实例对象，而不是显示的实例化：

```
 public class HomeController : Controller
 {
     private IFlagRepository _flagRepository;

     public HomeController(IFlagRepository flagRepository)
     {
         _flagRepository = flagRepository;
     }
}
```

#### 基于运行时条件解析类型

有时您希望在DI系统中注册抽象类型，但只有在验证某些运行时条件（例如，附加的cookie，HTTP标头或查询字符串参数）之后，您才需要确定具体类型。 

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IFlagRepository>(provider =>
    {
    	//创建要返回的实际类型的实例
		//基于当前登录用户的身份
        var context = provider.GetRequiredService<IHttpContextAccessor>();
        return new FlagRepositoryForUser(context.HttpContext.User);
    });
}
```

#### 按需解析类型

在某些情况下，您需要创建具有自己的依赖项的类型的实例，可以使用DI系统帮你解决这类问题。通常，DI系统以一个称为容器的根对象为中心，该容器遍历依赖树并解析抽象类型。在ASP.NET Core系统中，容器由IServiceProvider接口表示。要解析FlagService的实例，您有两个选择：使用经典的new运算符并提供IFlagRepository实现依赖项的有效实例或利用IServiceProvider，如下所示：

```
var flagService = provider.GetService<FlagService>();
```

#### 控制对象的生命周期

使用DI系统注册类型有三种不同的方法，返回实例的生命周期各不相同。

###### AddTransient

每个调用者都会收到一个新创建的指定类型的实例。

###### AddSingleton

所有请求都会收到应用程序启动后第一次创建的指定类型的相同实例。 如果由于某种原因没有可用的缓存实例，则会重新创建它。 该方法还具有重载功能，允许您将实例传递给缓存并按需返回。

###### AddScoped

在给定请求的上下文中对DI系统的每次调用都接收在请求处理开始时创建的相同实例。 此选项类似于单例，但它的作用域为请求生存期。

下面的代码显示了如何注册用户创建的实例作为单例：

```c#
public void ConfigureServices(IServiceCollection services)
{
   services.AddSingleton<ICountryRepository>(new CountryRepository());
}
```

每个抽象类型都可以映射到多个具体类型。 发生这种情况时，系统使用最后注册的具体类型来解决依赖关系。 如果找不到具体类型，则返回null。 如果找到具体类型，但无法实例化，则抛出异常。



## 与外部DI库集成

ASP.NET Core中自带的DI框架，对于基本任务，它完成的很好，能够满足ASP.NET Core 平台的需求，但是与其他流行的DI框架相比，并不存在竞争优势。它与其他流行的DI框架最大的不同是注入点。

#### 注入点

一般来说，依赖关系可以以三种不同的方式注入到类中：作为构造函数，公共方法或公共属性中的附加参数。 但是，ASP.NET Core中的DI实现一直保持简单，并不完全支持其他流行的DI框架之类的高级用例，包括Microsoft的Unity，AutoFac，Ninject，Structure-Map等等。因此在ASP.NET Core中，依赖注入只能通过构造函数进行。但是，在完全启用的MVC上下文中使用DI时，可以使用FromServices属性将类的公共属性或方法参数标记为注入点。 缺点是FromServices属性属于ASP.NET模型绑定层，从技术上讲它不是DI系统的一部分。 因此，只有在启用ASP.NET MVC引擎且仅在控制器类的范围内时，才能使用FromServices。

#### 使用外部DI框架

如果您发现ASP.NET Core DI基础结构太简单，无法满足您的需求，或者您有针对不同DI框架编写的大型代码库，那么您可以将ASP.NET Core系统配置为切换到使用外部DI框架。 但是，要实现这一点，外部框架需要支持ASP.NET Core并提供连接到ASP.NET Core基础结构的桥梁。支持ASP.NET Core意味着提供与.NET Core框架兼容的类库以及IServiceProvider接口的自定义实现。作为此支持工作的一部分，外部DI框架还必须能够导入本机或以编程方式向ASP.NET Core DI系统注册的服务集合。

TODO：代码补充

请务必注意，您可以在ConfigureServices中注册外部DI框架。 但是，在这样做时，必须将启动类中方法的返回类型从void更改为IServiceProvider。 最后，请记住，只有少数DI框架已经移植到.NET Core。 在少数人中，有Autofac和StructureMap。 您可以通过Autofac.Extensions.DependencyInjection NuGet包获取Autofac for .NET Core。



## 访问Web服务器上的文件

在ASP.NET Core中，在您明确启用它之前，没有任何功能可用。 启用功能意味着将适当的NuGet包添加到项目中，向DI系统注册服务，以及在启动类中配置服务。 即使对于必须注册的MVC引擎，该规则也没有例外。 同样，您必须注册一项服务，以保证访问位于Web根文件夹下的静态文件。

#### 启用静态文件服务

要启用静态文件（如HTML页面，图像，JavaScript文件或CSS文件）的检索，需要将以下行添加到startup类的Configure方法中：

 ```
app.UseStaticFiles();
 ```

启用静态文件服务不会让您的用户浏览指定目录的内容。 要启用目录浏览，您还需要以下内容：

```c#
public void ConfigureServices(IServiceCollection services) 
{
   services.AddDirectoryBrowsing();
}

public void Configure(IApplicationBuilder app) 
{
   app.UseStaticFiles(); 
   app.UseDirectoryBrowser(); 
}
```

使用上面的代码，将为Web根目录下的所有目录启用目录浏览。 您还可以将浏览限制为几个目录。

```c#
public void Configure(IApplicationBuilder app) 
{
   app.UseDirectoryBrowser(new DirectoryBrowserOptions()
   {
       FileProvider = new PhysicalFileProvider(
           Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", "pics"))
   });
}
```

中间件添加了一个目录配置，只能浏览wwwroot / pics文件夹。 如果您还想启用其他目录的浏览，只需复制UseDirectoryBrowser调用，将路径更改为所需目录。

请注意，静态文件和目录浏览是独立设置。 您可以同时启用，不启用，也可以只启用其中一个。 但实际上，您希望在任何Web应用程序中至少启用静态文件。
注意：启用目录浏览不是建议使用的功能，因为它可能会导致用户潜入您的文件并可能了解您的网站的秘密。

#### 启用多个Web根

有时，您希望能够从wwwroot以及其他目录提供静态文件。 这绝对可以在ASP.NET Core中实现，所需要的只是对UseStaticFiles的多次调用，如下所示。

```c#
public void Configure(IApplicationBuilder app)
{
    //从配置的Web根文件夹（即WWWROOT）启用服务文件
    app.UseStaticFiles();
    //启用位于站点根文件夹下的\Assets中的服务文件
    app.UseStaticFiles(new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), @"Assets")),
        	RequestPath = new PathString("/Public/Assets")
    });
}
```

该代码包含两个UseStaticFiles调用。 前者使应用程序默认只从配置的Web根文件夹-wwwroro中提供文件。 后者使应用程序还可以提供位于站点根目录下的Assets文件夹下的文件。 但是，在这种情况下，用于从Assets物理文件夹中检索文件的URL是什么？ 这正是StaticFileOptions类的RequestPath属性的作用。 要从Assets访问test.jpg，浏览器应调用以下URL：/public/assets/test.jpg。

注意：IIS有自己的HTTP模块来处理名为StaticFileModule的静态文件。 在IIS下托管ASP.NET Core应用程序时，ASP.NET核心模块会绕过默认的静态文件处理程序。 但是，如果IIS中的ASP.NET核心模块配置错误或丢失，则不会绕过StaticFileModule，并且文件将在您的控件之外提供。 为避免这种情况，作为一项额外措施，建议为ASP.NET Core应用程序禁用IIS的StaticFileModule。

#### 支持默认文件

默认Web文件是当用户导航到站点内的文件夹时自动提供的HTML页面。 默认页面通常名为index。*或default。*，允许的扩展名为.html和.htm。 除非您添加以下中间件，否则这些文件应放在wwroot中但忽略不计：

```c#
public void Configure(IApplicationBuilder app)
{
	app.UseDefaultFiles();
	app.UseStaticFiles();
}
```

请注意，必须在静态文件中间件之前启用默认文件中间件。 特别是，默认文件中间件将按以下顺序检查以下文件：default.htm，default.html，index.htm和index.html。 搜索在找到的第一个匹配时停止。

可以按照以下代码重新定义默认文件名列表：

```c#
var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("home.html");
options.DefaultFileNames.Add("home.htm");
app.UseDefaultFiles(options);
```

#### 添加自己的MIME类型

静态文件中间件可识别并提供400多种不同的文件类型。 但是，如果您的网站遗漏了MIME类型，您仍然可以添加它。

```c#
public void Configure(IApplicationBuilder app)
{
   // 设置自定义内容类型 - 将文件扩展名与MIME类型关联
   var provider = new FileExtensionContentTypeProvider();

   // 添加新映射或替换（如果已存在）
   provider.Mappings[".script"] = "text/javascript";

   // Remove JS files
   provider.Mappings.Remove(".js");

   app.UseStaticFiles(new StaticFileOptions()
   {
   	  ContentTypeProvider = provider
   });
}
```

对于传统的ASP.NET Web应用程序，添加缺少的MIME类型是您在IIS中执行的配置任务。 但是，在ASP.NET Core应用程序的上下文中，IIS（以及其他平台上的Web服务器）扮演反向代理的角色，只是将传入的请求转发到ASP.NET Core嵌入式Web服务器（Kestrel）和 从那里通过请求管道。 但是，必须以编程方式配置管道。