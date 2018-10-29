# ASP.NET Core应用程序安全性

web应用程序的安全性有很多方面。首先，在web场景中，安全性与确保交换数据的机密性有关。其次，它涉及到避免篡改数据，从而确保信息在端到端传递时保持完整性。web安全的另一个方面是防止在运行的应用程序中注入恶意代码。最后，安全性涉及到构建只有经过身份验证和授权的用户才能访问的应用程序(以及应用程序的各个部分)。

本文将主要讨论如何在ASP.NET Core中实现用户身份验证，并探索新的基于策略的API来处理用户授权。



## 网络安全的基础结构

HTTP协议在设计时并没有考虑到安全性，但后来对其进行了安全修补。显然，HTTP没有加密，这意味着第三方仍然可以拦截和收集正在两个连接的系统之间传递的数据。

### HTTPS协议

HTTPS是HTTP协议的安全形式。通过在网站上使用它，浏览器和网站之间的所有通信都是加密的。任何进出HTTPS页面的信息都会以确保完全机密性的方式自动加密。加密基于安全证书的内容。发送数据的方式取决于web服务器上启用的安全协议，例如传输层安全(TLS)及其前身安全套接字层(SSL)。

强烈建议在web服务器配置中禁用SSL 2.0和SSL 3.0，应该只启用TLS 1.x。

### 处理安全证书

在谈论HTTPS和证书时，通常使用SSL证书表达式。

HTTPS web服务器的配置决定了要使用的安全协议，证书只包含一对私有/公共加密密钥，并且绑定了域名和所有者的身份。

### 对HTTPS应用加密

当您的浏览器请求位于HTTPS连接上的网页时，网站最初会通过返回配置的HTTPS证书做出反应。证书包含安排安全对话所需的公钥。接下来，浏览器和网站将根据配置的协议（通常为TLS）的规则完成握手。如果浏览器信任该证书，那么它将生成一个对称的公钥/私钥，并与服务器共享公钥。



## ASP.NET Core中的认证

与旧版本的ASP.NET相比，用户身份验证是ASP.NET Core中变化最大的部分之一。

### 基于Cookie的身份验证

在ASP.NET Core中，用户身份验证涉及使用cookie来跟踪用户的身份。尝试访问私有页面的任何用户都将被重定向到登录页面，除非他们携带有效的身份验证cookie。然后登录页面在客户端收集凭据并在服务器上验证它们。如果一切正常，就会释放一个cookie。 Cookie会随着该用户通过同一浏览器发出的任何后续请求一起传播，直至其过期。

在ASP.NET Core中，与传统的ASP.NET相比，有两个主要变化：

- 首先，不再有web.config文件，这意味着以不同方式指定和检索登录路径、cookie名称和到期的配置。
- 其次，IPrincipal对象(用于建模用户身份的对象)是基于声明而不是纯用户名的。

#### 启用身份验证中间件

要在全新的ASP.NET Core应用程序中启用cookie身份验证，您需要引用Microsoft.AspNetCore.Authentication.Cookies包。但是，与同一ASP.NET Core框架的早期版本相比，ASP.NET Core 2.0中输入到应用程序的实际代码是不同的。

身份验证中间件作为服务公开，它必须在Startup类的ConfigureServices方法中进行配置。

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = new PathString("/Account/Login");
            options.Cookie.Name = "YourAppCookieName";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
            options.AccessDeniedPath = new PathString("/Account/Denied");
            ...
        });
}
```

AddAuthentication扩展方法获取一个字符串作为参数，指示要使用的身份验证方案。如果您计划支持单一身份验证方案，您将使用此路由。稍后，我们将看到如何稍微调整此代码以支持多个方案和处理程序。必须使用AddAuthentication返回的对象来调用表示身份验证处理程序的另一个方法。在上面的示例中，AddCookie方法指示框架通过配置的cookie登录和验证用户。每个身份验证处理程序（cookie，bearer等）都有自己的一组配置属性。

相反，在Configure方法中，您只需声明您打算使用已配置的身份验证服务，而无需指定任何其他选项。

```c#
public void Configure(IApplicationBuilder app)
{
     app.UseAuthentication();
     ...
}
```

代码片段中有一些名称和概念值得进一步解释 - 最值得注意的是身份验证方案。

#### Cookie身份验证选项

存储在web.config文件的<authentication>部分中的经典ASP.NET MVC应用程序的大部分信息现在都在代码中配置为中间件选项。上面的代码段列出了您可能想要选择的一些最常见的选项。表8-1提供了有关每个选项的更多详细信息。

8-1 Cookie身份验证选项

| 选项               | 说明                                                         |
| ------------------ | ------------------------------------------------------------ |
| AccessDeniedPath   | 指示如果当前标识没有查看所请求资源的权限，将重定向经过身份验证的用户的路径。该选项设置用户必须重定向到的URL，而不是接收纯HTTP 403状态代码。 |
| Cookie             | CookieBuilder类型的容器对象，包含正在创建的身份验证cookie的属性。 |
| ExpireTimeSpan     | 设置身份验证cookie的到期时间。时间必须是绝对的还是相对的，取决于SlidingExpiration属性的值。 |
| LoginPath          | 指示将匿名用户重定向以使用自己的凭据登录的路径。             |
| ReturnUrlParameter | 指示在匿名用户的情况下，用于传递最初请求的URL的参数的名称，该URL导致重定向到登录页面。 |
| SlidingExpiration  | 指示ExpireTimeSpan值是作为绝对时间还是相对时间。在后一种情况下，该值被视为间隔，如果超过间隔的一半，中间件将重新发出cookie。 |

请注意，LoginPath和AccessDeniedPath等路径属性的值不是字符串。实际上，LoginPath和AccessDeniedPath的类型为PathString。在.NET Core中，PathString类型与普通String类型不同，因为它在构建请求URL时提供了正确的转义。实质上，它是一种更具特定于URL的字符串类型。

ASP.NET Core中用户身份验证工作流程的总体设计确实提供了前所未有的灵活性。它的每个方面都可以随意定制。作为示例，让我们看看如何控制基于每个请求使用的身份验证工作流。

### 处理多种身份验证方案

有趣的是，在过去的ASP.NET版本中，身份验证挑战是自动的，您几乎无能为力。自动身份验证质询意味着一旦检测到当前用户缺少正确的身份信息，系统就会自动为配置的登录页面提供服务。在ASP.NET Core 1.x中，默认情况下，身份验证质询是自动的，但它会受到您的更改。在ASP.NET Core 2.0中，关闭自动质询的设置再次被删除。

但是，在ASP.NET Core中，您可以注册多个不同的身份验证处理程序，并通过算法或通过配置确定每个请求必须使用哪个处理程序。

### 启用多个身份验证处理程序

在ASP.NET Core中，您可以选择多种身份验证处理程序，例如基于cookie的身份验证，承载身份验证，通过社交网络或身份服务器进行身份验证，以及您可以想到和实现的其他任何内容。要注册多个身份验证处理程序，您只需在ASP.NET Core 2.0 Startup类的ConfigureServices方法中逐个列出所有部分。

每个配置的身份验证处理程序都由名称标识。该名称只是您在应用程序中用于引用处理程序的常规和任意字符串。处理程序的名称称为身份验证方案。可以将身份验证方案指定为魔术字符串，如Cookie或Bearer。但是，对于常见情况，存在一些预定义常量来限制在代码中使用时的拼写错误。如果您使用魔术字符串，请注意字符串被视为区分大小写。

```c#
// Authentication scheme set to "Cookies"
services.AddAuthentication(options =>
{
     options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

     options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

     options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;

})
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/Account/Login");
        options.Cookie.Name = "YourAppCookieName";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = new PathString("/Account/Denied");

    })

    .AddOpenIdConnect(options =>
    {
        options.Authority = "http://localhost:6000";
        options.ClientId = "...";
        options.ClientSecret = "...";
        ...
    });
```

您只需在调用AddAuthentication之后连接处理程序定义。同时，当注册多个处理程序时，您必须指明默认挑战，身份验证和登录方案。换句话说，当用户被要求在登录时证明她的身份时，您指示在对呈现的令牌进行身份验证时使用哪个处理程序。在每个处理程序中，您可以覆盖登录方案以满足您的目的。

#### 应用身份验证中间件

与传统的ASP.NET MVC一样，ASP.NET Core使用Authorize属性来修饰受身份验证的控制器类或操作方法。

```c#
[Authorize]
public class CustomerController : Controller
{
    // All action methods in this controller will 
    // be subject to authentication except those explicitly 
    // decorated with the AllowAnonymous attribute.
    ...
}
```

正如代码段中所指出的，您还可以使用AllowAnonymous属性将特定操作方法标记为匿名，因此不受身份验证的限制。

因此，操作方法上存在Authorize属性会限制其仅对经过身份验证的用户使用。但是，如果有多个身份验证中间件可用，应该应用哪个？ ASP.NET Core在Authorize属性上提供了一个新属性，允许您根据请求选择身份验证方案。

```c#
[Authorize(ActiveAuthenticationSchemes = "Bearer")]
public class ApiController : Controller
{
    // Your API action methods here 
    ...
}
```

此代码段的净效果是示例ApiController类的所有公共端点都受到承载令牌验证的用户身份的约束。

### 建模用户身份

必须以某种独特的方式描述登录到ASP.NET Core应用程序的任何用户。在Web的早期阶段 - 首次设计ASP.NET框架时 - 唯一的用户名足以唯一地标识已登录的用户。实际上，在旧版本的ASP.NET中，用户名是保存在身份验证cookie中的所有内容，用于模拟用户的身份。

有关用户的双重信息值得指出。几乎所有应用程序都有某种用户存储，其中保存了有关用户的所有详细信息。这种商店中的数据项具有主键和许多其他描述性字段。当该用户登录应用程序时，将创建一个身份验证cookie，并复制一些特定于用户的信息。至少，您必须在cookie中保存标识用户的唯一值，因为它出现在应用程序的后端。但是，身份验证cookie还可以包含与安全环境严格相关的其他信息。

总之，您通常在域中有一个实体，表示用户的持久层和一组名称/值对，它们提供从身份验证cookie读取的直接用户信息。这些名称/值对以声明的名称命名。

#### 索赔介绍

在ASP.NET Core中，声明是存储在身份验证cookie中的内容。作为开发人员，您可以在身份验证cookie中存储的所有内容都是声明，即名称/值对。与过去相比，您可以添加更多信息到cookie并直接从那里读取，而无需从数据库中获取更多数据。

您使用声明来建模用户身份。 ASP.NET Core正式化了一长串预定义声明，即预定义的键名，旨在存储某些众所周知的信息。欢迎您定义其他声明。在一天结束时，定义索赔取决于您和您的申请。

在ASP.NET Core Framework中，您可以找到围绕以下布局设计的Claim类。

```c#
public class Claim
{
    public string Type { get; }
    public string Value { get; }
    public string Issuer { get; }
    public string OriginalIssuer { get; }
    public IDictionary<string, string> Properties { get; }

    // More properties
}
```

声明具有一个属性，用于标识有关用户的声明类型。例如，声明类型是用户在给定应用程序中的角色。声明还具有字符串值。例如，Role声明的值可能是“admin”。声明的描述由原始发行者的名称完成，如果声明通过中间发行人转发，则还包括实际发行人的名称。最后，声明还可以包含附加属性的字典以补充该值。所有属性都是只读的，构造函数是推送值的唯一方法。索赔是一个不可变的实体。

#### 在代码中使用声明

一旦用户提供了有效凭证（或者更一般地，一旦用户已经绑定到已知身份），要解决的问题是持久存储关于所识别身份的关键信息。如前所述，在旧版本的ASP.NET中，这仅限于存储用户名。由于使用声明，它在ASP.NET Core中更具表现力。

要准备要存储在身份验证cookie中的用户数据，通常按以下步骤操作：

```c#
// Prepare the list of claims to bind to the user's identity
var claims = new Claim[] {
    new Claim(ClaimTypes.Name, "123456789"),
    new Claim("display_name", "Sample User"),
    new Claim(ClaimTypes.Email, "sampleuser@yourapp.com"),
    new Claim("picture_url", "\images\sampleuser.jpg"),
    new Claim("age", "24"),
    new Claim("status", "Gold"),
    new Claim(ClaimTypes.Role, "Manager"),
    new Claim(ClaimTypes.Role, "Supervisor")
};

// Create the identity object from claims
var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

// Create the principal object from identity
var principal = new ClaimsPrincipal(identity);
```

您可以从声明创建标识对象类型ClaimsIdentity，并从标识对象创建主体对象类型ClaimsPrincipal。创建标识时，还要指明选择的身份验证方案（意味着您指定了如何处理声明）。在代码片段中，CookieAuthenticationDefaults.AuthenticationScheme的传递值（Cookie的字符串值）表示声明将存储在身份验证Cookie中。

上面的代码片段中有几点需要注意。

- 首先，声明类型是纯字符串值，但是对于常见类型（如角色，名称，电子邮件）存在许多预定义常量。您可以使用自己的字符串或预定义的字符串作为常量从ClaimTypes类中公开。
- 其次，您可以在同一个声明列表中拥有多个角色。

#### 索赔假设

所有索赔都是平等的，但有些索赔比其他索赔更平等。名称和角色是两个享有ASP.NET核心基础结构（合理）特殊处理的声明。我们考虑以下代码：

```c#
var claims = new Claim[]
{
     new Claim("PublicName", userName),
     new Claim(ClaimTypes.Role, userRole),
     // More claims here
};
```

声明列表有两个元素 - 一个名为PublicName，另一个名为Role（通过常量ClaimTypes.Roles）。如您所见，没有名为Name的声明存在。当然，这不是错误，因为索赔清单完全取决于您。但是，至少拥有名称和角色是相当普遍的。 ASP.NET Core Framework为ClaimsIdentity类提供了一个超出声明列表的附加构造函数，并且身份验证方案还允许您通过名称指示给定列表中包含身份名称和角色的声明。

```c#
var identity = new ClaimsIdentity(claims,
      CookieAuthenticationDefaults.AuthenticationScheme,
      "PublicName",
      ClaimTypes.Role);
```

此代码的净效果是，名为Role的声明将是角色声明，正如人们所期望的那样。无论提供的声明列表是否包含名称声明，PublicName都是您应该用作用户名称的声明。

名称和角色在声明列表中指出，因为将使用这两条信息 - 主要是为了与旧的ASP.NET代码向后兼容 - 以支持IPrincipal接口的功能，例如IsInRole和Identity.Name。索赔列表中指定的角色将通过ClaimsPrincipal类中的IsInRole实现自动兑现。同样，用户名默认为使用“名称”状态指定的声明的值。

总之，Name和Role声明具有默认名称，但您可以随意覆盖这些名称。覆盖发生在ClaimsIdentity类的一个重载构造函数中。

### 登录并注销

拥有主要对象是登录用户的必要条件。标记用户的实际方法创建了身份验证cookie，由身份验证名称下的HTTP上下文对象公开。

```c#
// Gets the principal object
var principal = new ClaimsPrincipal(identity);

// Signs the user in (and creates the authentication cookie)
await HttpContext.SignInAsync(
          CookieAuthenticationDefaults.AuthenticationScheme,
          principal);
```

确切地说，只有在身份验证方案设置为cookie时，才会在登录过程中创建cookie。登录过程中发生的确切操作顺序取决于所选身份验证方案的处理程序。

Authentication对象是AuthenticationManager类的实例。该类有两个更有趣的方法：SignOutAsync和AuthenticateAsync。顾名思义，前一种方法会撤销身份验证cookie并将用户从应用程序中签名。

```c#
await HttpContext.SignOutAsync(
          CookieAuthenticationDefaults.AuthenticationScheme);
```

调用方法时，必须指明要从中注销的身份验证方案。相反，AuthenticateAsync方法只验证cookie并检查用户是否经过身份验证。此外，在这种情况下，验证cookie的尝试基于所选的身份验证方案。

#### 阅读索赔内容

ASP.NET核心身份验证是熟悉的世界和半个未知空间 - 特别是对于那些来自多年经典ASP.NET编程的人来说。在经典ASP.NET中，一旦系统处理了身份验证cookie，就可以轻松访问用户名，这是默认情况下唯一可用的信息。如果有关用户的更多信息必须可用，您可以创建自己的声明并将其内容序列化到cookie中，从而创建自己的主体对象。最近，经典ASP.NET中添加了对声明的支持。使用声明是在ASP.NET Core中工作的唯一方法。当您创建自己的委托人时，您自己负责阅读声明的内容。

您通过HttpContext.User属性以编程方式访问的ClaimsPrincipal实例具有用于查询特定声明的编程接口。这是一个从Razor视图中获取的示例。

```c#
@if(User.Identity.IsAuthenticated)
{
    var pictureClaim = User.FindFirst("picture_url");
    if (pictureClaim != null)
    {
            var picture = pictureClaim.Value;
            <img src="@picture" alt="" />
    }
}
```

渲染页面时，您可能希望显示已登录用户的头像.假设此信息可用作声明，上面的代码显示了查询声明的LINQ友好代码。 FindFirst方法仅返回可能具有相同名称的多个声明中的第一个。如果你想要全部使用它们，那么你可以使用FindAll方法。要阅读声明的实际值，请展开“值”属性。

注意一旦验证了登录页面凭据，您就会遇到想要在cookie中保留所有声明的问题。请注意，您存储在cookie中的信息越多，您几乎可以免费获得的用户信息就越多。有时，您可以在cookie中存储用户密钥，一旦登录开始，您就可以使用密钥从数据库中检索匹配的记录。这样更昂贵但确保用户信息始终是最新的，并且它允许更新，而无需在创建cookie时将用户注销并再次登录。应从您确定的位置读取索赔的实际内容。例如，声明内容可以来自数据库，云或Active Directory。

### 外部认证

外部身份验证是指使用外部且配置正确的服务对访问您网站的用户进行身份验证。一般而言，外部认证是双赢的。外部身份验证适用于不必为每个要注册的网站创建一个帐户的最终用户。此外，外部身份验证对于不必添加关键样板代码并存储和检查她设置的每个网站的用户凭据的开发人员都有好处。不仅任何网站都可以作为外部认证服务器。外部认证服务器需要特定功能的可用性，但几乎任何当前的社交网络都可以充当外部认证服务。

#### 添加对外部认证服务的支持

ASP.NET Core从头开始通过身份提供商支持外部身份验证。大多数情况下，您所做的就是为作业安装适当的NuGet包。例如，如果您希望允许您的用户使用他们的Twitter凭据进行身份验证，那么您在项目中执行的第一件事就是引入Microsoft.AspNetCore.Authentication.Twitter包并安装相关的处理程序：

```c#
services.AddAuthentication(TwitterDefaults.AuthenticationScheme)
  .AddTwitter(options =>
  {
      options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.ConsumerKey = "...";
      options.ConsumerSecret = "...";
  });
```

SignInScheme属性是将用于保留生成的标识的身份验证处理程序的标识符。在此示例中，将使用身份验证cookie。要查看上述中间件的效果，请添加控制器方法以触发基于Twitter的身份验证。以下是一个例子。

```c#
public async Task TwitterAuth()
{
   var props = new AuthenticationProperties
   {
      RedirectUri = "/"  // Where to go after authenticating
   };
   await HttpContext.ChallengeAsync(TwitterDefaults.AuthenticationScheme, props);
}
```

Twitter处理程序的内部知道要联系哪个URL以传递应用程序的标识（使用者密钥和密钥）并启用用户的验证。如果一切顺利，将向用户显示熟悉的Twitter身份验证页面。如果用户已在本地设备上通过Twitter进行身份验证，则仅要求她确认可以代表用户授予给定应用程序在Twitter上运行的权限。

图8-1显示了Twitter的确认页面，其中显示了示例应用程序何时尝试对用户进行身份验证。

![denglupage](assets/denglupage.jpg)

图8-1作为Twitter用户，您现在正在授权应用程序代表您执行操作

接下来，一旦Twitter成功验证了用户，SignInScheme属性就会指示应用程序执行下一步操作。如果您想要外部提供商（Twitter，在示例中）返回的声明中的cookie，则可以接受“Cookies”值。如果您想通过中间表单查看和完成信息，那么您必须通过引入临时登录方案来打破这一过程。我马上回到这个更复杂的场景。现在，让我们完成一个简单场景中发生的事情。

RedirectUri选项指示验证成功完成后的去向。在这种仅依赖于身份验证服务提供的声明列表的简单方案中，您无法控制您了解登录系统的每个用户的数据。各种社交网络默认返回的声明列表不是同质的。例如，如果用户通过Facebook连接，您可能拥有该用户的电子邮件地址。但是，如果用户通过Twitter或Google连接，您可能没有电子邮件地址。如果你只支持一个社交网络，这不是什么大问题，但是如果你支持其中许多社交网络 - 并且这个数字可能会随着时间的推移而增长 - 那么你必须设置一个中间页面来规范化信息并要求用户输入所有的声明你目前缺乏。

图8-2显示了在访问需要登录的受保护资源时在客户端浏览器，Web应用程序和外部身份验证服务之间设置的工作流。

![8_2](assets/8_2.jpg)

图8-2通过外部服务提供访问受保护资源和身份验证时的完整工作流程

此图显示了代表浏览器，Web应用程序和身份验证服务的三个框。在顶部，一个粗体箭头将“浏览器”框与“Web App”框连接。在底部，另一个箭头将“Web App”框与“浏览器”框连接。其他灰色箭头表示该框的各个步骤。通过外部服务验证用户的过程。

#### 要求完成信息

要在外部服务对用户进行身份验证后收集其他信息，您需要稍微调整一下服务配置。实质上，您将另一个处理程序添加到列表中，如下所示。

```
services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/Account/Login");
        options.Cookie.Name = "YourAppCookieName";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = new PathString("/Account/Denied");
        })
    .AddTwitter(options =>
    {
        options.SignInScheme = "TEMP";
        options.ConsumerKey = "...";
        options.ConsumerSecret = "...";
    }) 
    .AddCookie("TEMP");
```

当外部Twitter提供程序返回时，使用TEMP方案创建临时cookie。通过在挑战用户的控制器方法中适当地设置重定向路径，您有机会检查Twitter返回的主体并进一步编辑它：

```c#
public async Task TwitterAuthEx()
{
   var props = new AuthenticationProperties
   {
       RedirectUri = "/account/external"  
   };
   await HttpContext.ChallengeAsync(TwitterDefaults.AuthenticationScheme, props);
}
```

Twitter（或您使用的任何服务）现在将重定向到帐户控制器上的External方法，以完成您自己的工作流程。当回调外部方法时，一切都取决于你。您可能希望显示HTML表单以收集其他信息。在构建此表单时，您可能希望使用给定主体的声明列表。

```c#
public async Task<IActionResult> External()
{
    var principal = await HttpContext.AuthenticateAsync("TEMP");

    // Access the claims on the principal and prepare an HTML 
    // form that prompts only for the missing information
    ...

    return View();
}
```

然后向用户呈现表格并填写;您的表单代码验证数据并回发。在控制器方法的主体中，您保存完成表单的内容，您需要在离开之前执行几个关键步骤。您检索如上所示的主体，然后您登录到cookie方案并退出临时TEMP方案。这是代码：

```c#
await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
await HttpContext.SignOutAsync("TEMP");
```

此时 - 仅在此时 - 创建身份验证cookie。

注释在前面的示例代码中，TEMP以及CookieAuthenticationDefaults。 AuthenticationScheme只是内部标识符;只要它们在整个应用程序中保持一致，它们就可以重命名。

#### 外部认证问题

外部身份验证（例如通过Facebook或Twitter）有时对用户来说很酷，但并非总是如此。像往常一样，这是一个权衡问题。因此，让我们列出在应用程序中使用它时必须面对的一些挑战。

首先，用户必须登录您选择的社交网络或身份服务器。他们可能会也可能不喜欢使用现有凭据的想法。一般而言，社交认证应始终作为选项提供，除非应用程序本身与社交网络或社交网络紧密集成，以证明依赖于唯一的外部认证。始终考虑用户可能没有您支持的社交网络帐户。

从开发角度来看，外部身份验证意味着在每个应用程序中重复配置身份验证的工作。通常情况下，您必须处理用户注册并填写所有必填字段，这意味着就您的帐户管理而言，需要做很多工作。最后，您必须在本地用户存储中的帐户与外部帐户之间建立链接。

最后，外部身份验证并不是一种节省时间的方法。如果应用程序本身的性质合理，它应该被视为您为应用程序的用户提供的功能。



## 通过ASP.NET IDENTITY认证用户

到目前为止，您已经了解了ASP.NET Core中用户身份验证的基础知识。然而，整个功能范围在于用户身份验证。它通常以会员制的名义进行。会员制不仅仅是管理用户认证和身份数据的过程;它还涉及用户管理，密码哈希，验证和重置，角色及其管理以及更高级的功能，如双因素身份验证（2FA）。

构建自定义成员资格系统并不是一项艰巨的任务，但它可能是一项重复性的任务，而且它是您每次都需要重新创建的经典轮，以及您构建的每个应用程序。同时，成员资格系统并不容易抽象成可以在多个应用程序中以最小开销重用的东西。多年来已经进行了许多尝试，而微软本身就是其中的一部分。我个人对会员系统的看法是，如果您要编写和维护同一复杂度的多个系统，您可能希望花一些时间并使用自己的可扩展性点构建自己的系统。在其他情况下，选择是在两个极端之间 - 本章前面讨论的普通用户身份验证或ASP.NET身份。

### ASP.NET身份的一般性

ASP.NET Identity是一个成熟的，全面的大型框架，它为成员资格系统提供了一个抽象层。如果只需要通过从简单数据库表中读取的普通凭证来验证用户，那就太过分了。但与此同时，ASP.NET Identity旨在将存储与安全层分离。因此，最终，它提供了一个丰富的API，其中包含大量的可扩展性点，您可以根据上下文调整内容，同时还包括通常只需要配置的API。

配置ASP.NET标识意味着指示存储层的详细信息（关系和面向对象）以及最能代表用户的标识模型的详细信息。图8-3说明了ASP.NET标识的体系结构。

![8_3](assets/8_3.jpg)

#### 用户管理器

用户管理器是中央控制台，您可以从中执行ASP.NET Identity支持的所有操作。如上所述，这包括用于查询现有用户，创建新用户以及更新或删除用户的API。用户管理器还提供了支持密码管理，外部登录，角色管理以及更高级功能的方法，例如用户锁定，2FA，需要时通过电子邮件发送以及密码强度验证。

在代码中，您可以通过UserManager <TUser>类的服务调用上述函数。泛型类型是指提供的用户实体的抽象。换句话说，通过该类，您可以在给定的用户模型上执行所有编码任务。

#### 用户身份抽象

在ASP.NET Identity中，用户身份的模型成为您在机器中注入的参数，并且由于用户身份抽象机制和底层用户存储抽象，它或多或少地透明地工作。

ASP.NET Identity提供了一个基本用户类，该用户类已包含您希望在用户实体上拥有的许多常用属性，例如主键，用户名，密码哈希，电子邮件地址和电话号码。 ASP.NET Identity还提供更复杂的属性，例如电子邮件确认，锁定状态，访问失败计数以及角色和登录列表。 ASP.NET Identity中的基本用户类是IdentityUser。您可以直接使用它，也可以从中派生自己的类。

```c#
public class YourAppUser : IdentityUser
{
    // App-specific properties
    public string Picture { get; set; }
    public string Status { get; set; }
}
```

IdentityUser类将一些方面硬编码到框架中。将类保存到数据库时，Id属性被视为主键。即使我很难想到这样做的原因，也无法改变这方面。默认情况下，主键呈现为字符串，但在框架设计中甚至已经抽象了主键的类型，因此您可以在从IdentityUser派生时根据自己的喜好进行更改。

```c#
public class YourAppUser : IdentityUser<int>
{
    // App-specific properties
    public string Picture { get; set; }
    public string Status { get; set; }
}
```

事实上，Id属性定义如下：

```c#
public virtual TKey Id { get; set; }
```

注意在旧版本的ASP.NET Identity中 - 对于经典ASP.NET - 主键呈现为GUID，这在某些应用程序中产生了一些问题。在ASP.NET Core中，您可以根据需要使用GUID。

#### 用户存储抽象

身份用户类通过某些存储API的服务保存到某个持久层。最喜欢的API基于Entity Framework Core，但是用户存储的抽象允许您插入几乎任何知道如何存储信息的框架。主存储接口是IUserStore <TUser>。这是摘录。

```c#
public interface IUserStore<TUser, in TKey> : IDisposable where TUser : class, IUser<TKey>
{
   Task CreateAsync(TUser user);
   Task UpdateAsync(TUser user);
   Task DeleteAsync(TUser user);
   Task<TUser> FindByIdAsync(TKey userId);
   Task<TUser> FindByNameAsync(string userName);
   ...
}
```

如您所见，抽象是身份用户类之上的普通CRUD API。查询功能非常基本，因为它只允许您按名称或ID检索用户。

但是，具体的ASP.NET Identity用户存储比IUserStore接口建议的要多得多。表8-2列出了其他功能的存储接口。

表8-2一些其他存储接口:

| 附加接口              | 描述                                                         |
| --------------------- | ------------------------------------------------------------ |
| IUserClaimStore       | 接口组用于存储有关用户的声明。如果您将声明作为用户实体本身属性的不同信息存储，则此选项非常有用。 |
| IUserEmailStore       | 接口组用于存储电子邮件信息，例如用于密码重置。               |
| IUserLockoutStore     | 接口组用于存储锁定数据以跟踪暴力攻击。                       |
| IUserLoginStore       | 接口组用于存储通过外部提供程序获取的链接帐户。               |
| IUserPasswordStore    | 接口组用于存储密码和执行相关操作。                           |
| IUserPhoneNumberStore | 接口组用于存储要在2FA中使用的电话信息。                      |
| IUserRoleStore        | 接口组用于存储角色信息。                                     |
| IUserTwoFactorStore   | 接口组用于存储与2FA相关的用户信息。                          |

所有这些接口都由实际的用户存储实现。如果您创建自定义用户存储（例如，针对自定义SQL Server架构或自定义NoSQL存储的用户存储），则您负责实施。 ASP.NET Identity附带了一个基于Entity Framework的用户存储，可通过Microsoft.AspNetCore.Identity.EntityFrameworkCore NuGet包获得。该存储支持表8-2中列出的接口。

#### 配置ASP.NET标识

要开始使用ASP.NET Identity，首先需要选择（或创建）用户存储组件并设置基础数据库。假设您选择实体框架用户存储，您必须做的第一件事是在您的应用程序中创建一个DbContext类。 DbContext类及其所有依赖项的作用将在第9章中详细解释，第9章完全专注于Entity Framework Core。

简而言之，DbContext类表示通过Entity Framework以编程方式访问数据库的中央控制台。与ASP.NET Identity一起使用的DbContext类继承自系统提供的基类（IdentityDbContext类），并包含用于用户和其他实体（如登录，声明和电子邮件）的DbSet类。这是你如何布置课程。

```c#
public class YourAppDatabase : IdentityDbContext<YourAppUser>
{
   ...
}
```

要将连接字符串配置为实际数据库，请使用常规Entity Framework Core代码。稍后详细介绍，然后在第9章。

在IdentityDbContext中，您将注入用户标识类以及许多其他可选组件。这是班级的完整签名。

```c#
public class IdentityDbContext<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim> :
             DbContext
    where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>
    where TRole : IdentityRole<TKey, TUserRole>
    where TUserLogin : IdentityUserLogin<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
{
   ...
}
```

如您所见，您可以注入用户标识，角色类型，用户标识的主键，用于链接外部登录的类型，用于表示用户/角色映射的类型以及表示声明的类型。

启用ASP.NET标识的最后一步是使用ASP.NET Core注册框架。此步骤发生在启动类的ConfigureServices方法中。

```c#
public void ConfigureServices(IServiceCollection services)
{
    // Grab the connection string to use (or have it fixed)
    // Assume Configuration is set in the Startup class constructor (see Ch.7)
    var connString = Configuration.GetSection("database").Value;     

    // Normal EF code to register a DbContext around a SQL Server database
    services.AddDbContext<YourAppDatabase>(options =>       
               options.UseSqlServer(connString));              

    // Attach the previously created DbContext to the ASP.NET Identity framework
    services.AddIdentity<YourAppUser, IdentityRole>()               
            .AddEntityFrameworkStores<YourIdentityDatabase>();   
}
```

一旦知道连接所选数据库的连接字符串，就可以使用普通的实体框架代码在ASP.NET Core堆栈中注入给定数据库的DbContext。接下来，注册用户身份角色模型，角色身份模型和基于实体框架的用户存储。

在配置时，您还可以指定要创建的身份验证cookie的参数。这是一个例子。

```c#
ervices.ConfigureApplicationCookie(options =>
{
   options.Cookie.HttpOnly = true;
   options.Cookie.Expiration = TimeSpan.FromMinutes(20);
   options.LoginPath = new PathString("/Account/Login");  
   options.LogoutPath = new PathString("/Account/Logout");
   options.AccessDeniedPath = new PathString("/Account/Denied");
   options.SlidingExpiration = true;
});
```

同样，您也可以更改cookie名称，并且通常可以完全控制cookie。

### 使用用户管理器

UserManager对象是您通过其使用和管理基于ASP.NET标识的成员资格系统的中心对象。您不直接创建它的实例;当您在启动时注册ASP.NET身份时，它的一个实例将静默注册到DI系统。

```c#
public class AccountController : Controller
{
    UserManager<YourAppUser> _userManager;
    public AccountController(UserManager<YourAppUser> userManager)
    {
        _userManager = userManager;
    }

    // More code here
    ...
}
```

在您需要使用它的任何控制器类中，您只需以某种方式注入它;例如，您可以通过构造函数注入它，如前面的代码片段所示。

#### 与用户打交道

要创建新用户，请调用CreateAsync方法并将具有ASP.NET标识的应用程序中使用的用户对象传递给它。该方法返回一个IdentityResult值，该值包含一个错误对象列表和一个Boolean属性来表示成功或失败。

```c#
public class IdentityResult
{
    public IEnumerable<IdentityError> Errors { get; }
    public bool Succeeded { get; protected set; }
}

public class IdentityError
{
    public string Code { get; set; }
    public string Description { get; set; }
}
```

CreateAsync方法有两个重载：一个只接受用户对象，另一个接受密码。前一种方法只是没有为用户设置任何密码。通过使用ChangePasswordAsync方法，您可以稍后设置或更改密码。

将用户添加到成员资格系统时，您将面临确定如何以及在何处验证添加到系统中的数据的一致性的问题。您是否应该拥有一个知道如何验证自身的用户类，或者您是否应该将验证部署为单独的层？ ASP.NET Identity选择后一种模式。可以支持接口IUserValidator <TUser>来实现给定类型的任何自定义验证器。

```c#
public interface IUserValidator<TUser>
{
    Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
}
```

您可以创建实现该接口的类，然后在应用程序启动时将其注册到DI系统。

可以通过调用DeleteAsync来删除成员资格系统中的用户。该方法与CreateAsync具有相同的签名。相反，要更新现有用户的状态，您有许多预定义方法，例如SetUserNameAsync，SetEmailAsync，SetPhoneNumberAsync，SetTwoFactorEnabledAsync等。要编辑声明，您可以使用AddClaimAsync，RemoveClaimAsync和类似方法来处理登录。

每次调用特定更新方法时，都会执行对基础用户存储的调用。或者，您可以在内存中编辑用户对象，然后使用UpdateAsync方法以批处理模式应用所有更改。

#### 获取用户

ASP.NET身份会员系统提供了两种用于获取用户数据的模式。您可以通过参数查询用户对象，无论是ID，电子邮件还是用户名，或者您可以使用LINQ。以下代码段说明了一些查询方法的使用。

```c#
var user1 = await _userManager.FindByIdAsync(123456);

var user2 = await _userManager.FindByNameAsync("dino");

var user3 = await _userManager.FindByEmailAsync("dino@yourapp.com");
```

如果用户存储支持IQueryable接口，则可以在从UserManager对象公开的Users集合之上构建任何LINQ查询。

```c#
var emails = _userManager.Users.Select(u => u.Email);
```

如果您只需要特定的信息，例如电子邮件或电话号码，那么您可以使用单个API调用-GetEmailAsync，GetPhoneNumberAsync等来完成。

#### 处理密码

在ASP.NET Identity中，密码使用RFC2898算法自动进行哈希散列，并进行一万次迭代。从安全角度来看，这是一种非常安全的密码存储方式。散列通过IPasswordHasher接口的服务进行。像往常一样，您可以通过在DI系统中添加新的垫圈来替换您自己的垫圈。

要验证密码的强度 - 并拒绝弱密码 - 您可以依赖内置的验证器基础结构并进行配置，或者您可以创建自己的密码。配置内置验证器意味着设置最小长度并确定是否需要字母和/或数字。这是一个例子：

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddIdentity<YourAppUser, IdentityRole>(options=>
    {
        // At least 6 characters long and digits required
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<YourDatabase>();
}
```

要使用自定义密码验证程序，您需要创建一个实现IPasswordValidator的类，并在调用AddIdentity后在应用程序启动时使用AddPasswordValidator注册它。

#### 处理角色

在一天结束时，角色只是声明，事实上，在本章的前面，我们已经看到存在名为Role的预定义声明。抽象地说，角色只是一个没有权限和逻辑映射到它的字符串，它描述了用户可以在应用程序中扮演的角色。将角色映射逻辑和权限对于使应用程序更加生动并使其变得现实是必要的。但是，这个责任属于开发人员。

但是，在成员制度的背景下，角色的意图更加具体。像ASP.NET Identity这样的会员系统包含了开发人员应该自己保存和检索用户及相关信息的大部分工作。成员资格系统所做的部分工作是将用户映射到角色。在此上下文中，角色成为用户可以或不可以对应用程序执行的操作的列表。在ASP.NET Core和ASP.NET Identity中，角色是保存在用户存储中的命名的声明组。

在ASP.NET Identity应用程序声明中，用户，支持的角色以及用户和角色之间的映射是分开存储的。涉及角色的所有操作都分组在RoleManager对象中。与UserManager对象一样，当在应用程序启动时调用AddIdentity时，也会将RoleManager添加到DI系统。同样，您通过DI在控制器中注入RoleManager实例。角色存储在不同的角色存储中。在EF场景中，它只是同一SQL Server数据库中的一个不同的表。

以编程方式管理角色几乎与以编程方式管理用户相同。以下是如何创建角色的示例。

```c#
// Define the ADMIN role
var roleAdmin = new IdentityRole
{
    Name = "Admin"
};

// Create the ADMIN role in the ASP.NET Identity system
var result = await _roleManager.CreateAsync(roleAdmin);
```

在ASP.NET Identity中，在用户映射到角色之前，角色无效。

```c#
var user = await _userManager.FindByNameAsync("dino");
var result = await _userManager.AddToRoleAsync(user, "Admin");
```

要将用户添加到角色，请使用UserManager类的API。除了AddToRoleAsync之外，管理器还提供了RemoveFromRoleAsync和GetUsersInRoleAsync等方法。

#### 验证用户

使用ASP.NET Identity对用户进行身份验证需要许多步骤，因为框架的复杂性和复杂性。步骤涉及诸如验证凭据，处理失败尝试和锁定用户，处理禁用用户以及处理2FA逻辑（如果启用该功能）等操作。然后，您必须使用声明填充ClaimsPrincipal对象并发出身份验证cookie。

所有步骤都封装在SignInManager类公开的API中。登录管理器通过DI获得，方法与您在UserManager和RoleManager对象中看到的方式相同。要执行登录页面的所有步骤，请使用PasswordSignInAsync方法。

```c#
public async Task<IActionResult> Login(string user, string password, bool rememberMe)
{
    var shouldConsiderLockout = true;
    var result = await _signInManager.PasswordSignInAsync(
                           user, password, rememberMe, shouldConsiderLockout);
    if (result.Succeeded)
    {
        // Redirect where needed
        ...

    }
    return View("error", result);
}
```

PasswordSignInAsync方法使用用户名和密码（作为明文）以及一些布尔标志来表示生成的身份验证cookie的持久性，以及是否应该考虑锁定。

注意用户锁定是ASP.NET Identity内置功能，通过该功能可以禁止用户登录系统。该功能由两条信息控制 - 是否为应用程序启用了锁定和锁定结束日期。您具有启用和禁用锁定的临时方法，并且您具有设置锁定结束日期的临时方法。如果禁用锁定或启用了锁定，则用户处于活动状态，但当前日期已超过锁定结束日期。

登录过程的结果由SignInResult类型汇总，该类型通知验证是否成功，是否需要2FA，或者用户是否被锁定。



## 授权政策

软件应用程序的授权层确保允许当前用户访问给定资源，执行给定操作或对给定资源执行给定操作。在ASP.NET Core中，有两种方法可以设置授权层。您可以使用角色，也可以使用策略。从以前版本的ASP.NET平台维护了以前的基于角色的授权。基于策略的授权对于ASP.NET Core来说是全新的，并且非常强大和灵活。

### 基于角色的授权

授权比认证更进一步。身份验证是关于发现用户的身份以跟踪其活动，并且仅允许已知用户进入系统。授权更具体，是关于定义用户调用预定义应用程序端点的要求。受权限约束并随后授权层的任务的常见示例包括显示或隐藏用户界面的元素，执行动作或仅流入其他服务。在ASP.NET中，角色是从早期开始实施授权层的常用方法。

从技术上讲，角色是一个没有附加行为的普通字符串。但是，它的值被ASP.NET和ASP.NET Core安全层视为元信息。例如，两个层都检查主体对象中存在的角色。 （请参阅主体中标识对象中的方法IsInRole。）除此之外，应用程序还使用角色向该角色中的所有用户授予权限。

在ASP.NET Core中，已记录用户的声明中角色信息的可用性取决于支持标识存储。例如，如果您使用社交身份验证，则根本不会看到角色。通过Twitter或Facebook进行身份验证的用户不会带来任何可能对您的应用程序有重要意义的角色信息。但是，您的应用程序可能会根据内部和特定于域的规则为该用户分配角色。

总之，角色只是元信息，应用程序 - 只有应用程序 - 可以转化为执行或不执行某些操作的权限。 ASP.NET Core Framework仅提供一些基础结构来保存，检索和承载角色。用户和角色之间支持的角色和映射列表通常存储在基础成员资格系统中（无论是自定义还是基于ASP.NET标识），并在验证用户凭据时检索。接下来，在某种程度上，角色信息附加到用户帐户并暴露给系统。身份对象上的IsInRole方法（ASP.NET Core中的ClaimsIdentity）是用于实现基于角色的授权的杠杆。

#### 授权属性

Authorize属性是保护控制器或其某些方法的声明性方法。

```c#
[Authorize]
public class CustomerController : Controller
{
	...
}
```

请注意，如果指定不带参数，则Authorize属性仅检查用户是否经过身份验证。在上面的代码片段中，所有可以成功登录系统的用户同样可以调用CustomerController类的任何方法。要仅选择用户的子集，请使用角色。

Authorize属性上的Roles属性表示只有任何列出的角色中的用户才能被授予对控制器方法的访问权限。

在下面的代码中，Admin和System用户同样可以调用BackofficeController类。

```c#
[Authorize(Roles="Admin, System")]
public class BackofficeController : Controller
{
   ...  

   [Authorize(Roles="System")]
   public IActionResult Reset()
   {
      // You MUST be a SYSTEM user to get here
      ...
   }

   [Authorize]
   public IActionResult Public()
   {
      // You just need be authenticated and can view this 
      // regardless of role(s) assigned to you
      ...
   }

   [AllowAnonymous)]
   public IActionResult Index()
   {
      // You don't need to be authenticated to get here
      ...
   }
}
```

Index方法根本不需要身份验证。 Public方法只需要经过身份验证的用户。重置方法严格要求系统用户。您可能与Admin或System用户一起使用的所有其他方法。

如果访问控制器需要多个角色，则可以多次应用“授权”属性。或者，您始终可以编写自己的授权过滤器。在下面的代码中，只有具有Admin和System角色的用户才被授予调用控制器的权限。

```c#
[Authorize(Roles="Admin")]
[Authorize(Roles="System")]
public class BackofficeController : Controller
{
   ...  
}
```

（可选）Authorize属性还可以通过ActiveAuthenticationSchemes属性接受一个或多个身份验证方案。

```c#
[Authorize(Roles="Admin, System", ActiveAuthenticationSchemes="Cookies"]
public class BackofficeController : Controller
{
   ...  
}
```

ActiveAuthenticationSchemes属性是一个逗号分隔的字符串，列出授权层在当前上下文中将信任的身份验证组件。换句话说，它声明仅当用户通过Cookies方案进行身份验证并具有任何列出的角色时才允许访问BackofficeController类。如上所述，传递给ActiveAuthenticationSchemes属性的字符串值必须符合在应用程序启动时向身份验证服务注册的处理程序。随后，身份验证方案本质上是一个选择处理程序的标签。

#### 授权过滤器

Authorize属性提供的信息由预定义的系统提供的授权过滤器使用。此筛选器在任何其他ASP.NET Core筛选器之前运行，因为它负责检查用户是否可以执行所请求的操作。如果不是，则授权过滤器使管道短路并取消当前请求。

可以创建自定义授权过滤器，但通常不需要这样做。实际上，最好配置默认过滤器所依赖的现有授权层。

#### 角色，权限和替代

角色是一种简单的方法，可以根据应用程序的用户或不能做的事情对用户进行分组。然而，角色并不是很有表现力;至少不足以满足大多数现代应用的需求。例如，考虑一个相对简单的授权体系结构：站点的常规用户和授权访问后台并更新内容的高级用户。基于角色的授权层可以围绕两个角色 - 用户和管理员构建。在此基础上，您可以定义每组用户可以访问的控制器和方法。

问题在于，在现实世界中，事情很少如此简单。在现实世界中，通常会遇到用户在给定用户角色中可以做或不可做的细微区别。你有角色，但你需要做出例外和否决。例如，在可以访问后台的用户中，有些只被授权编辑客户数据，有些应该只对内容有效，有些可以同时进行。您将如何呈现如图8-4所示的授权方案？

![8_4](assets/8_4.jpg)

这是由框和箭头图表做的例证。 “用户”框和“管理员”框具有灰色背景，“管理员”框具有出站箭头，分别将其连接到标有“客户”，“内容”和“客户+内容”的其他白色框。

角色基本上是扁平的概念。即使是如图8-4所示的简单层次结构，你会如何扁平化？例如，您可以创建四个不同的角色：User，Admin，CustomerAdmin和ContentsAdmin。 Admin角色将是CustomerAdmin和ContentsAdmin的联合。

它可以工作，但是当严格遵守业务规则的否决数量增加时，所需角色的数量将显着增加。

最重要的是，角色不一定是处理授权的最有效方式，尽管它们对于向后兼容性和非常简单的场景非常有用。对于其他情况，还需要其他东西。输入基于策略的授权。

### 基于策略的授权

在ASP.NET Core中，基于策略的授权框架旨在解耦授权和应用程序逻辑。策略是设计为需求集合的实体。要求是当前用户必须满足的条件。最简单的策略是用户通过身份验证。另一个常见要求是用户与给定角色相关联。另一个要求是用户具有特定权利要求或具有特定价值的特定权利要求。在大多数通用术语中，需求是关于用户身份的断言，必须证明该声明对于用户被授予访问给定方法的权限。

#### 定义授权策略

您可以使用以下代码创建策略对象：

```c#
var policy = new AuthorizationPolicyBuilder()
    .AddAuthenticationSchemes("Cookie, Bearer")
    .RequireAuthenticatedUser()
    .RequireRole("Admin")
    .RequireClaim("editor", "contents")
    .RequireClaim("level", "senior")
    .Build();
```

构建器对象使用各种扩展方法收集需求，然后构建策略对象。如您所见，需求依据身份验证状态和方案，角色以及通过身份验证cookie（或者，如果使用的话，不记名令牌）读取的任何声明组合。

注释承载令牌是身份验证cookie的替代方法，用于携带有关用户身份的信息。承载令牌通常由非浏览器客户端调用的Web服务使用，例如移动应用程序。我们将在第10章“设计Web API”中处理持有者令牌。

如果用于定义需求的预定义扩展方法都不适用于您，那么您始终可以通过自己的断言来定义新的需求。就是这样：

```c#
var policy = new AuthorizationPolicyBuilder()
    .AddAuthenticationSchemes("Cookie, Bearer")
    .RequireAuthenticatedUser()
    .RequireRole("Admin")
    .RequireAssertion(ctx =>
    {
        return ctx.User.HasClaim("editor", "contents") ||
               ctx.User.HasClaim("level", "senior");
    })
    .Build();
```

RequireAssertion方法接受一个接收HttpContext对象的lambda并返回一个布尔值。因此，断言是一种条件陈述。请注意，如果在策略定义中多次连接RequireRole，则用户必须遵守所有角色。如果你想表达一个OR条件，那么你必须诉诸一个断言。实际上，在上面的示例中，策略允许作为内容编辑者或高级用户的用户。

定义后，还必须在授权中间件中注册策略。

#### 注册政策

授权中间件首先在启动类的ConfigureServices方法中注册为服务。在此过程中，您将使用所有必需的策略配置服务。可以通过构建器对象创建策略，并通过AddPolicy扩展方法添加（或仅声明）。

```c#
services.AddAuthorization(options=>
{
   options.AddPolicy("ContentsEditor", policy =>
   {      policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
       policy.RequireAuthenticatedUser();
       policy.RequireRole("Admin");
       policy.RequireClaim("editor", "contents");
   });
};
```

添加到授权中间件的每个策略都有一个名称，然后该名称将用于引用控制器类的Authorize属性中的策略。以下是如何设置策略而不是角色来定义控制器方法的权限。

```c#
[Authorize(Policy = "ContentsEditor")]
public IActionResult Save(Article article)
{
	...
}
```

通过Authorize属性，您可以以声明方式设置策略，并允许ASP.NET Core的授权层在方法执行之前强制执行。或者，您可以以编程方式强制执行策略。这是必要的代码。

```c#
public class AdminController : Controller
{
    private IAuthorizationService _authorization;
    public AdminController(IAuthorizationService authorizationService)
    {
        _authorization = authorizationService;
    }

    public async Task<IActionResult> Save(Article article)
    {
        var allowed = await _authorization.AuthorizeAsync(
              User, "ContentsEditor");
        if (!allowed.Succeeded)
            return new ForbiddenResult();

        // Proceed with the method implementation 
        ...
    }
}
```

像往常一样，通过DI注入对授权服务的引用。 AuthorizeAsync方法获取应用程序的主体对象和策略名称，并返回具有Succeeded布尔属性的AuthorizationResult对象。当其值为false时，您会找到FailCalled或FailRequirements of Failure属性的原因。如果权限的编程检查失败，则应返回ForbiddenResult对象。

注意当权限检查失败时返回ForbiddenResult或ChallengeResult之间存在细微差别;如果考虑ASP.NET Core 1.x，差异甚至更加棘手。 ForbiddenResult是一个很好的答案 - 你失败了 - 并且返回了一个HTTP 401状态代码。 ChallengeResult是一种温和的回应。如果用户已登录，它最终会出现在ForbiddenResult中，如果未记录，则会重定向到登录页面。但是，从ASP.NET Core 2.0开始，ChallengeResult不再将未登录的用户重定向到登录页面。因此，对失败权限作出反应的唯一合理方法是通过ForbiddenResult。

### Razor Views的政策

到目前为止，我们已经看到了控制器方法的策略检查。您还可以在Razor视图中执行相同的检查，特别是如果您正在使用第5章“ASP.NET MVC视图”中讨论的Razor页面。

```asp
@{ 
   var authorized = await Authorization.AuthorizeAsync(User, "ContentsEditor")
}
@if (!authorized)
{
   <div class="alert alert-error">
      You're not authorized to access this page.
   </div>
}
```

要使以前的代码起作用，必须首先注入对授权服务的依赖关系。

```c#
@inject IAuthorizationService Authorization
```

在视图中使用授权服务可以帮助隐藏当前用户无法访问的用户界面的片段。

重要仅基于授权权限检查显示或隐藏用户界面元素（例如，指向安全页面的链接）是不够安全的。只要您还在控制器方法级别执行权限检查，这样做也可以。请记住，控制器方法是访问系统后端的唯一方法，人们总是可以通过在浏览器中键入URL来尝试直接访问页面。隐藏的链接并不完全安全。理想的方法是检查门的权限，门是控制器级别。唯一的例外是从ASP.NET Core 2.0开始，您使用Razor页面。

#### 自定义要求

库存需求包括声明和身份验证，并提供基于断言进行自定义的通用机制。您也可以创建自定义要求。

策略要求由两个元素组成 - 一个只保存数据的需求类和一个将根据用户验证数据的授权处理程序。如果未能使用库存工具表达所需的策略，则可以创建自定义要求。

例如，假设我们希望通过添加用户必须具有至少三年经验的要求来扩展ContentsEditor策略。这是自定义要求的示例类。

````c#
public class ExperienceRequirement : IAuthorizationRequirement
{
    public int Years { get; private set; }
    public ExperienceRequirement(int minimumYears)
    {
        Years = minimumYears;
    }
}
````

需求必须至少有一个授权处理程序。处理程序是AuthorizationHandler <T>类型的类，其中T是需求类型。下面的代码说明了ExperienceRequirement类型的示例处理程序。

```c#
public class ExperienceHandler : AuthorizationHandler<ExperienceRequirement>
{
    protected override Task HandleRequirementAsync( 
         AuthorizationHandlerContext context, 
         ExperienceRequirement requirement)
    {

        // Save User object to access claims
        var user = context.User;
        if (!user.HasClaim(c => c.Type == "EditorSince"))
           return Task.CompletedTask;
        var since = int.Parse(user.FindFirst("EditorSince").Value);
        if (since >= requirement.Years)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

样本授权处理程序读取与用户关联的声明并检查自定义EditorSince声明。如果没有找到，它只会返回而不做任何事情。仅当索赔存在且索赔包含不小于指定年数的整数值时，才会返回成功。自定义声明应该是以某种方式链接到用户的一条信息 - 例如，Users表中的一列保存到身份验证cookie。但是，一旦您拥有对用户的引用，您始终可以从声明中找到用户名并对数据库或外部服务运行查询，以了解经验年数并使用处理程序中的信息。

不可否认，如果EditorSince值保持DateTime并且计算自用户作为编辑器开始以来已经过了给定的年数，则上面的示例会更加真实。

授权处理程序调用方法Succeed，指示已成功验证该要求。如果要求没有通过，那么处理程序不需要做任何事情而只能返回。但是，如果处理程序想要确定需求的失败，那么无论同一需求上的其他处理程序是否成功，它就会在授权上下文对象上调用Fail方法。

重要通常，从处理程序调用Fail应被视为特殊情况。事实上，授权处理程序通常会成功或什么也不做，因为需求可以有多个处理程序，而另一个可能会成功。无论如何，当您想要阻止任何其他处理程序成功时，Calling Fail仍然是关键情况的选项。另请注意，即使以编程方式调用Fail，授权层也会评估其他所有需求，因为处理程序可能具有日志记录等副作用。

以下是向策略添加自定义要求的方法。由于这是自定义要求，因此您没有扩展方法，并且必须继续执行策略对象的“要求”集合。

```c#
services.AddAuthorization(options =>
{
    options.AddPolicy("AtLeast3Years",
        policy => policy
                  .Requirements
                  .Add(new ExperienceRequirement(3)));
});
```

此外，您还必须在IAuthorizationHandler类型的范围内向DI系统注册新处理程序。

````c#
services.AddSingleton<IAuthorizationHandler, ExperienceHandler>();
````

如上所述，需求可以有多个处理程序。当在DI系统中为授权层中的相同要求注册多个处理程序时，至少有一个成功就足够了。

在授权处理程序的实现中，有时可能需要检查请求属性或路由数据。

```c#
if (context.Resource is AuthorizationFilterContext)
{
    var url = mvc.HttpContext.Request.GetDisplayUrl();
    ...
}
```

在ASP.NET Core中，AuthorizationHandlerContext对象将Resource属性集公开给过滤器上下文对象。根据所涉及的框架，上下文对象是不同的。例如，MVC和SignalR发送它们自己的特定上下文对象。是否转换Resource属性中保存的值取决于您需要访问的内容。例如，用户信息始终存在，因此您无需为此进行强制转换。但是，如果您需要特定于MVC的详细信息，例如路由或URL和请求信息，那么您必须进行强制转换。



## 概要

保护ASP.NET Core应用程序需要通过两层身份验证和授权。身份验证是旨在将身份与来自特定用户代理的请求相关联的步骤。授权旨在检查该身份是否可以某种方式执行它所请求的操作。身份验证通过以创建身份验证cookie为中心的基本API，并且还可以依赖于提供高度可定制的成员身份系统-ASP.NET身份的专用框架的服务。授权有两种形式。一种是传统的基于角色的授权，其工作方式与它在经典ASP.NET MVC中的工作方式相同。另一种是基于策略的身份验证，这是一种新方法，可以创建更丰富，更具表现力的权限模型。策略是基于声明和自定义逻辑的需求集合，基于可以从HTTP上下文或外部源注入的任何其他信息。需求与一个或多个处理程序相关联，处理程序负责实际评估需求。

在讨论ASP.NET身份时，我们讨论了一些与数据库相关的对象和概念。在下一章中，我们将只讨论ASP.NET Core中的数据访问。



