# ASP.NET Core MVC 视图

在MVC模式中，视图用于用户数据交互和呈现。它是一个嵌入了Razor标记的HTML模板，即在Razor标记中使用C#编程语言的.cshtml文件。

按照约定，所有的视图文件均存放在Views文件夹下的与控制器名称相同的子文件夹内：Views/[ControllerName]/[ViewName].cshtml

视图按照功能可分为：

- 普通视图，基于控制器操作创建的视图。
- 布局视图，用于页面的布局，通过提供一致的网页部分减少代码重复。
- 分部视图，基于静态页面的可重用部分，例如博客网站的作者简介。分部视图包含的是不需要执行后端代码就能生成页面的内容。
- 组件视图，和分部视图类似，但组件视图可以包含需要运行后端代码才能呈现页面的视图内容。例如网站购物车。



## 创建普通视图

在Views/[ControllerName]文件夹中创建基于该控制器的视图。

各个控制器之间共享的视图放在Views/Shared文件夹内。

按照约定，视图的名称和控制器中的操作方法名称相同。



## 在控制器中指定视图

大多数的控制器操作方法返回的类型为IActionResult接口，实现IActionResult接口的常见成员有：

- ActionResult类及其子类
- ViewResult类（该类继承自ActionResult）及其子类，返回该类型的方法常见的有：
  - View()及其重载方方法
- JsonResult类（该类继承自ActionResult），返回该类型的常见方法有：
  - Json()及其重载方法
- PartialViewResult类（该类继承自ActionResult），返回该类型的常见方法有：
  - PartialView()及其重载方法
- ViewComponentResult类（该类继承自ActionResult），返回该类型的常见方法有：
  - ViewComponent()及其重载方法

上述这些方法均是Controller类的成员方法，由于创建的控制器都继承自Controller方法，因此若要指定控制器操作方法对应的视图，只需要调用View()方法（或其重载方法）即可。

### View()重载方法

View()方法有以下几种重载版本：

```c#
public virtual ViewResult View();
public virtual ViewResult View(string viewName, object model);
public virtual ViewResult View(object model);
public virtual ViewResult View(string viewName);
```

#### View()

直接返回与当前控制器操作方法同名的视图。

#### View(string viewName)

返回指定视图名称的视图。也可以提供视图文件路径，如果使用从应用根目录开始的绝对路径（可选择以“/”或“/”开头），则必须指定.cshtml扩展名：

```c#
return View("Views/Home/About.cshtml");
```

也可使用相对路径在不同目录中指定视图，而无需指定 .cshtml 扩展名。

例如，在 HomeController 内，可以使用相对路径返回 Manage 视图的 Index 视图：

```c#
return View("../Manage/Index");
```

同样，可以用“./”前缀来指示当前的控制器特定目录：

```c#
return View("./About");
```

#### View(object model)

将模型传递给当前控制器操作方法对应的视图并返回。

#### View(string viewName, object model)

将模型传递给指定视图名称的视图并返回。

### 视图发现的过程

操作返回一个视图时，会发生称为“视图发现”的过程。 此过程基于视图名称确定使用哪个视图文件。

View()方法的默认行为是返回与调用控制器中的操作方法同名的视图，视图发现会按照以下顺序搜索匹配的视图文件：

1. Views/[ControllerName]/[ViewName].cshtml
2. Views/Shared/[ViewName].cshtml

如果上述1中没有找到视图文件，就会搜索上述2中的Views/Shared文件夹。

注意：视图发现依赖于按文件名称查找视图文件。 如果基础文件系统区分大小写，则视图名称也可能区分大小写。为了各操作系统的兼容性，请在控制器与操作名称之间，关联视图文件夹与文件名称之间匹配大小写。

也就是说，控制器名称、操作名称要与关联的视图文件夹名称、视图文件名称的大小写保持一致。



## 向视图传递数据

将数据传递给视图有以下几种方式：

- 强类型数据：ViewModel
- 弱类型数据：ViewData（或ViewDataAttribute）和ViewBag

### ViewModel

推荐使用该方式进行数据传递，需要在视图中使用@model指令指定模型的类型。

例如：

```c#
public IActionResult Contact()
{
    ViewData["Message"] = "Your contact page.";

    var viewModel = new Address()
    {
        Name = "Microsoft",
        Street = "One Microsoft Way",
        City = "Redmond",
        State = "WA",
        PostalCode = "98052-6399"
    };

    return View(viewModel);
}
```

在视图中使用@model指令，指定上述传入的数据的模型类型：

```html
@model WebApplication1.ViewModels.Address

<h2>Contact</h2>
<address>
    @Model.Street<br>
    @Model.City, @Model.State @Model.PostalCode<br>
    <abbr title="Phone">P:</abbr> 425.555.0100
</address>
```

建议将传递给视图的数据类型存储在应用根目录下的单独ViewModels文件夹中，并且数据类型以ViewModel结尾，例如：AddressViewModel。

### ViewData、ViewDataAttribute和ViewBag

注意：ViewBag在Razor Pages中不可用，只可以用于Razor视图。

ViewData属性是弱类型对象的字典，ViewBag属性是ViewData的包装器，为基础ViewData集合提供动态属性。

ViewData 和 ViewBag 在运行时进行动态解析。 由于它们不提供编译时类型检查，因此使用这两者通常比使用 viewmodel 更容易出错。因此尽量减少或根本不使用 ViewData 和 ViewBag进行数据传递。

#### ViewData

```c#
public ViewDataDictionary ViewData { get; set; }
```

ViewData属性实际访问的是一个ViewDataDictionary类型的键值对，通过传入string类型的键获取对应的object类型的值。字符串数据可以直接存储和使用，而不需要强制转换，但是在提取其他 ViewData 对象值时必须将其强制转换为特定类型。 可以使用 ViewData 将数据从控制器传递到视图，以及在视图（包括分部视图和布局）内传递数据。

```c#
public IActionResult SomeAction()
{
    ViewData["Greeting"] = "Hello";
    ViewData["Address"]  = new Address()
    {
        Name = "Steve",
        Street = "123 Main St",
        City = "Hudson",
        State = "OH",
        PostalCode = "44236"
    };
    return View();
}
```

在视图中获取数据：

```c#
@{
    // Since Address isn't a string, it requires a cast.
    var address = ViewData["Address"] as Address;
}

@ViewData["Greeting"] World!

<address>
    @address.Name<br>
    @address.Street<br>
    @address.City, @address.State @address.PostalCode
</address>
```

#### ViewDataAttribute

另一种会使用 ViewDataDictionary 的方法是 ViewDataAttribute。 控制器或 Razor 页面模型上使用 [ViewData] 修饰的属性将其值存储在字典中并从此处进行加载。

```c#
public class HomeController : Controller
{
    [ViewData]
    public string Message { get; set; }

    public IActionResult Index()
    {
        Message = "Hello，MVC";
        return View();
    }
}
```

在视图中，通过ViewData字典读取：

```c#
@ViewData["Message"]
```

#### ViewBag

ViewBag只能用于Razor视图中，不能用于Razor Pages中。ViewBag 是 DynamicViewData 对象，可提供对存储在 ViewData 中的对象的动态访问。 ViewBag 不需要强制转换，因此使用起来更加方便。

```c#
@ViewBag.Message
```

由于ViewBag读取的也是存储在ViewData中的数据，因此ViewBag和ViewData可以混合使用。

#### ViewData和ViewBag之间的差异

ViewData：

- ViewData派生自ViewDataDictionary，因此它有可用的字典属性，如 ContainsKey、Add、Remove 和 Clear。
- ViewData字典中的键是字符串，因此允许空格，例如：ViewData["Some Key With Whitespace"]
- 在视图中，除了string类型的值之外，其他类型的值在使用ViewData进行获取时均需要进行强制类型转换。

ViewBag：

- ViewBag只能用于Razor视图中，不能用于Razor Pages。
- ViewBag派生自 DynamicViewData，因此它可使用点表示法 (@ViewBag.SomeKey = <value or object>) 创建动态属性，且无需强制转换。 ViewBag 的语法使添加到控制器和视图的速度更快。
- ViewBag更易于检查 NULL 值。 示例：@ViewBag.Person?.Name。

综上所述，在能使用ViewBag的情况下，优先使用ViewBag，并且养成检查NULL值的习惯。当然，最好的方案是采用强类型模型，可以避免在运行时动态解析错误。

### 动态视图（不推荐）

动态视图是指在视图中不适用@model声明模型类型，但是有模型实例通过控制器操作方法传递给它们的视图（例如，return View(MyData);），在视图页面中依然可以使用该类型实例的属性。

```c#
<address>
    @Model.Street<br>
    @Model.City, @Model.State @Model.PostalCode<br>
    <abbr title="Phone">P:</abbr> 425.555.0100
</address>
```

虽然表面上提供了灵活性（生成不会报错），但不提供编译保护或 IntelliSense。 如果属性不存在，则网页生成在运行时会失败。所以非常不推荐使用动态视图。



## 分部视图

分部视图不仅应用于MVC的Razor视图中，也同样可以应用于Razor Pages页面中。分部视图也是一个Razor标记文件（.cshtml），通常应用于另一个标记文件（.cshtml）呈现的输出中。

### 何时使用分部视图

分部视图不应该包含复杂的呈现逻辑，对于需要执行后端程序代码才能呈现标记的内容，不应该使用分部视图。分部视图应该包含呈现简单的静态页面的内容，分部视图是执行下列操作的有效方式：

- 将大型的HTML标记文件分解为更小的组件。
- 在由多个逻辑部分组成的大型复杂标记文件中，可以使用分部视图处理隔开的每个部分。
- 对于多个重复的标记内容，可以使用分部视图进行输出。

注意：分部视图中不应该包含常见的布局元素，对于复杂的呈现逻辑或需要执行后端代码进行呈现的内容应该使用组件视图而不是分部视图。

### 创建分部视图

分部视图是在 Views 文件夹 (MVC) 或 Pages 文件夹 (Razor Pages) 中维护的 .cshtml 标记文件。

与MVC的Razor视图或Razor Pages页面呈现不同，分部视图不会运行_ViewStart.cshtml。

分部视图的文件名通常以下划线（_）开头。

创建分部视图和创建普通视图的方式相同，都是一个.cshtml标记文件。

### 引用分部视图

在.cshtml文件中，有多种方法可以引用分部视图，建议使用以下异步呈现方法之一：

- 分部标记帮助程序
- 异步HTML帮助程序

#### 分部标记帮助程序

分部标记帮助程序会异步呈现内容并使用类似 HTML 的语法：

```c#
<partial name="_PartialName" />
```

当存在文件扩展名时，标记帮助程序会引用分部视图，该分部视图必须与调用分部视图的标记文件位于同一文件夹中：

```c#
<partial name="_PartialName.cshtml" />
```

也可以使用绝对路径从应用程序根目录引用分部视图，以波形符斜杠 (~/) 或斜杠 (/) 开头的路径指代应用程序根目录：

Razor Pages页面：

```c#
<partial name="~/Pages/Folder/_PartialName.cshtml" />
<partial name="/Pages/Folder/_PartialName.cshtml" />
```

MVC Razor视图：

```c#
<partial name="~/Views/Folder/_PartialName.cshtml" />
<partial name="/Views/Folder/_PartialName.cshtml" />
```

也可以使用相对路径引用分部视图：

```c#
<partial name="../Account/_PartialName.cshtml" />
```

关于分部标记帮助程序的详细使用，可参阅：[ASP.NET Core 中的部分标记帮助程序](https://docs.microsoft.com/zh-cn/aspnet/core/mvc/views/tag-helpers/built-in/partial-tag-helper?view=aspnetcore-2.2)。

#### 异步HTML帮助程序

在.cshtml文件中，使用HTML帮助程序，通过调用Html.PartialAsync()或Html.RenderPartialAsync()方法来引用分部视图。

Html.PartialAsync()方法返回包含在Task<TResult>中的IHtmlContent类型，因此可以直接在await的调用前添加@字符前缀来引用该方法：

```c#
@await Html.PartialAsync("_PartialName")
```

当存在文件扩展名时，HTML 帮助程序会引用分部视图，该分部视图必须与调用分部视图的标记文件位于同一文件夹中：

```c#
@await Html.PartialAsync("_PartialName.cshtml")
```

使用绝对路径从应用程序根目录引用分部视图。以波形符斜杠 (~/) 或斜杠 (/) 开头的路径指代应用程序根目录：

Razor Pages页面：

```c#
@await Html.PartialAsync("~/Pages/Folder/_PartialName.cshtml")
@await Html.PartialAsync("/Pages/Folder/_PartialName.cshtml")
```

MVc Razor视图：

```c#
@await Html.PartialAsync("~/Views/Folder/_PartialName.cshtml")
@await Html.PartialAsync("/Views/Folder/_PartialName.cshtml")
```

使用相对路径引用分部视图：

```c#
@await Html.PartialAsync("../Account/_LoginPartial.cshtml")
```

Html.RenderPartialAsync()和Html.PartialAsync()使用方式相同，只不过Html.RenderPartialAsync()方法不返回IHtmlContent，它将呈现的输出直接流式传输到响应。 因为该方法不返回结果，所以必须在 Razor 代码块内调用它，即必须使用@{}进行包裹：

```c#
@{
    await Html.RenderPartialAsync("_AuthorPartial");
}
```

这两种方法都可以通过Html帮助程序呈现分部视图，由于RenderPartialAsync 流式传输呈现的内容，因此在某些情况下它可提供更好的性能（但是并不总是这样）。建议在性能起关键作用的情况下，可以使用两种方法对页面进行基准测试，并使用生成更快响应的方法。

#### 同步HTML帮助程序（不推荐）

Partial 和 RenderPartial 分别是 PartialAsync 和 RenderPartialAsync 的同步等效项。 但不建议使用同步等效项，因为可能会出现死锁的情况。

### 分部视图发现的过程

如果按名称（无文件扩展名）引用分部视图，则按所述顺序搜索以下位置：

**Razor 页面**

1. 当前正在执行页面的文件夹
2. 该页面文件夹上方的目录图
3. `/Shared`
4. `/Pages/Shared`
5. `/Views/Shared`

**MVC**

1. `/Areas/<Area-Name>/Views/<Controller-Name>`
2. `/Areas/<Area-Name>/Views/Shared`
3. `/Views/Shared`
4. `/Pages/Shared`

以下约定适用于分部视图发现：

- 当分部视图位于不同的文件夹中时，允许使用具有相同文件名的不同分部视图。
- 当按名称（无文件扩展名）引用分部视图且分部视图出现在调用方的文件夹和 文件夹中时，调用方文件夹中的分部视图会提供分部视图（调用方文件夹中的分部视图优先被采用）。 如果调用方文件夹中不存在分部视图，则会从 文件夹中提供分部视图。 文件夹中的分部视图称为“共享分部视图”或“默认分部视图”。
- 可以链接分部视图—如果调用没有形成循环引用，则分部视图可以调用另一个分部视图。 相对路径始终相对于当前文件，而不是相对于文件的根视图或父视图。

注意：

分部视图中定义的 Razor section 对父标记文件不可见。 section 仅对定义它时所在的分部视图可见。

### 通过分部视图访问数据

实例化分部视图时，它会获得父视图的 ViewData 字典的副本。 

在分部视图内对数据所做的更新不会保存到父视图中。 在分部视图中的 ViewData 更改会在分部视图返回时丢失。

也就是说，分部视图只能读取数据，不能更改数据。

将ViewData传递给分部视图：

```c#
@await Html.PartialAsync("../_PartialView1", ViewData)
```

将模型传递给分部视图。模型可以是自定义对象。 可以使用 PartialAsync（向调用方呈现内容块）或 RenderPartialAsync（将内容流式传输到输出）传递模型：

```c#
@await Html.PartialAsync("_PartialName", model)
```

Razor Pages中使用分部视图的复杂示例：

```c#
@model ReadRPModel

<h2>@Model.Article.Title</h2>
@await Html.PartialAsync("../Shared/_AuthorPartialRP", Model.Article.AuthorName)
@Model.Article.PublicationDate
@{
    var index = 0;

    @foreach (var section in Model.Article.Sections)
    {
        @await Html.PartialAsync("_ArticleSectionRP", section,
                                 new ViewDataDictionary(ViewData)
                                 {
                                     { "index", index }
                                 })

        index++;
    }
}
```

上述示例中，包含两个分部视图，第二个分部视图将模型和 ViewData 传入分部视图。 ViewDataDictionary 构造函数重载可用于传递新 ViewData 字典，同时保留现有的 ViewData 字典。

Pages/Shared/_AuthorPartialRP.cshtml 是 上述示例中引用的第一个分部视图：

```c#
@model string
<div>
    <h3>@Model</h3>
    This partial view from /Pages/Shared/_AuthorPartialRP.cshtml.
</div>
```

Pages/ArticlesRP/_ArticleSectionRP.cshtml 是示例中引用的第二个分部视图：

```c#
@using PartialViewsSample.ViewModels
@model ArticleSection

<h3>@Model.Title Index: @ViewData["index"]</h3>
<div>
    @Model.Content
</div>
```

在MVC中的Razor视图中使用分部视图和上述示例类似，代码如下：

```c#
@model PartialViewsSample.ViewModels.Article

<h2>@Model.Title</h2>
@await Html.PartialAsync("_AuthorPartial", Model.AuthorName)
@Model.PublicationDate

@{
    var index = 0;

    @foreach (var section in Model.Sections)
    {
        @await Html.PartialAsync("_ArticleSection", section,
                                 new ViewDataDictionary(ViewData)
                                 {
                                     { "index", index }
                                 })

        index++;
    }
}
```

其中Views/Articles/_ArticleSection.cshtml 是上述代码中引用的第二个分部视图：

```c#
@using PartialViewsSample.ViewModels
@model ArticleSection

<h3>@Model.Title Index: @ViewData["index"]</h3>
<div>
    @Model.Content
</div>
```



## 视图组件







## 分部视图和视图组件的区别



## 视图中的依赖关系注入

ASP.NET Core支持将依赖关系注入到视图，但是实际应用中，应避免直接使用注入到视图的服务，而应该通过控制器传入。

详细请参考：https://docs.microsoft.com/zh-cn/aspnet/core/mvc/views/dependency-injection?view=aspnetcore-2.2