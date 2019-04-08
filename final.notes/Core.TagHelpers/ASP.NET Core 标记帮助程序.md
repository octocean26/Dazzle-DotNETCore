# ASP.NET Core 标记帮助程序



## 标记帮助程序概述

标记帮助程序使用 C# 创建，基于元素名称、属性名称或父标记以 HTML 元素为目标，它使服务器端代码可以在Razor文件中参与创建和呈现HTML元素。

### 标记帮助程序和HTML帮助程序的比较

标记帮助程序有利于减少 Razor 视图中 HTML 和 C# 之间的显式转换。 通常情况下，如果能够使用标记帮助程序进行HTML标签的呈现，推荐优先使用标记帮助程序；如果使用标记帮助程序不能够满足需要，可以使用HTML帮助程序，它是标记帮助程序的替换方法，即：能够使用标记帮助程序的，一定可以使用HTML帮助程序进行替换。

注意：能够使用HTML帮助程序的，不一定可以使用标记帮助程序替换，因为，并非每个 HTML 帮助程序都有对应的标记帮助程序。



## 标记帮助程序作用域

控制标记帮助程序的作用域主要有以下几种方式：

- @addTagHelper
- @removeTagHelper
- 使用“`!`”操作符
- @tagHelperPrefix

### @addTagHelper

@addTagHelper指令指示视图可以使用标记帮助程序，一般会将该指令内容添加到`_ViewImports.cshtml`文件中，与`_ViewImports.cshtml`同级的文件和子文件夹都会应用其指定的指令，常见的指令内容如下：

```c#
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

@addTagHelper指令需要指定两个参数，第一个参数指定要加载的标记帮助程序，如果是单一的标记帮助程序，必须指定该标记帮助程序的完整限定名，一般更常用的是使用`*`表示加载该程序集（由第二个参数指定）中包含的所有标记程序；第二个参数指定包含标记帮助程序（由第一个参数指定）的程序集。

例如：`@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`表示加载Microsoft.AspNetCore.Mvc.TagHelpers程序集中包含的所有标记帮助程序。

Microsoft.AspNetCore.Mvc.TagHelpers 是ASP.NET Core 内置的标记帮助程序的程序集。

“`*`”在这里使用的是通配符语法，可以直接插入通配符“`*`”作为后缀，例如：

```
@addTagHelper AuthoringTagHelpers.TagHelpers.E*, AuthoringTagHelpers
@addTagHelper AuthoringTagHelpers.TagHelpers.Email*, AuthoringTagHelpers
```

### @removeTagHelper

@removeTagHelper 与 @addTagHelper 具有相同的两个参数，它会删除之前添加的标记帮助程序。 例如，应用于特定视图的 @removeTagHelper 会删除该视图中的指定标记帮助程序。 在 Views/Folder/_ViewImports.cshtml 文件中使用 @removeTagHelper，将从 Folder 中的所有视图删除指定的标记帮助程序。

### 选择退出字符“`!`”

使用标记帮助程序选择退出字符（“!”），可在元素级别禁用标记帮助程序。例如：

```
<!span asp-validation-for="Email" class="text-danger"></!span>
```

须将标记帮助程序选择退出字符应用于开始和结束标记。 （将选择退出字符添加到开始标记时，Visual Studio 编辑器会自动为结束标记添加相应字符）。 添加选择退出字符后，元素和标记帮助程序属性不再以独特字体显示。

### @tagHelperPrefix

@tagHelperPrefix 指令可指定一个标记前缀字符串，只有使用了该前缀的元素才支持标记帮助程序。

例如：

```
@tagHelperPrefix th:
```

上述代码表示，只有使用了前缀th:的元素才支持标记帮助程序。

```html
<th:label asp-for="wy"></th:label>
<label asp-for="wy2">women</label>
```

上述代码只有`<th:label>`中的asp-for属性能够被标记帮助程序解析，`<label>`不能被解析。

*提示：可以使用标记帮助程序的元素及属性使用独特的字体进行显示，可以通过该特性查看哪些元素能够被解析。*



## 创建标记帮助程序

标记帮助程序是实现ITagHelper接口的任何类。但是通常通过派生自TagHelper类来创建自己的标记帮助程序，TagHelper提供了供子类重写的Process()方法。

TagHelper类的定义如下：

```c#
public abstract class TagHelper : ITagHelper, ITagHelperComponent
{
	protected TagHelper();
	public virtual int Order { get; }
	public virtual void Init(TagHelperContext context);
	public virtual void Process(TagHelperContext context, TagHelperOutput output);
	public virtual Task ProcessAsync(TagHelperContext context, TagHelperOutput output);
}
```

### 创建简单的标记帮助程序

创建一个派生自TagHelper的类，推荐类名以TagHelper结尾，并重写父类的Process()方法，如下：

```c#
namespace My.TagHelpers.Study.CusTagHelpers
{
    public class EmailTagHelper:TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
        }
    }
}
```

上述代码将标记名称为email的标记，转换为a标签。

标记帮助程序使用不包含“TagHelper”后缀的类名的小写形式作为标记的目标名称，此处为`<email>`。重写的Process()方法中，可以使用包含与执行当前HTML标记相关的信息的下文参数TagHelperContext，和输出参数TagHelperOutput（它包含监控状态的HTML元素，代表用于生成 HTML 标记和内容的原始源）。

要让标记帮助程序用于Razor视图，还需要在Razor视图中（通常是Views/_ViewImports.cshtml 文件）使用@addTagHelper指令。

例如：

```html
@addTagHelper My.TagHelpers.Study.CusTagHelpers.EmailTagHelper,My.TagHelpers.Study
<email>Support</email>
```

执行程序，上述标记将被转换为：

```
<a>Support</a>
```

注意：转换后的结果`<a>`标签并不包含href属性。

### 创建包含属性的标记帮助程序

如果想要标记支持属性，只需要在对应的标记帮助程序中，定义该类的属性即可。对上述的EmailTagHelper类进行重构，代码如下：

```c#
public class EmailTagHelper:TagHelper
{
    private const string EmailDomain = "163.com";
    public string MailTo { get; set; }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        var address = $"{MailTo}@{EmailDomain}";
        output.Attributes.SetAttribute("href", "mailto:" + address);
        output.Content.SetContent(address);
    }
}
```

该标记帮助程序定义了MailTo属性，在页面使用<email>标记时，不能直接使用MailTo作为标记的属性，而是使用由短横线分割各个单词的格式的小写形式作为属性，即：`<email mail-to="value"></email>`。

使用上述定义的标记帮助程序，Razor页面的代码如下：

```html
<email mail-to="wy"></email>
```

执行程序生成的页面元素如下：

```html
<a href="mailto:wy@163.com">wy@163.com</a>
```

### 创建标记自结束的标记帮助程序

如果想使用标记自结束的标记，例如，上述中直接使用`<email mail-to="wy" />`，可以在标记帮助程序中，指定HtmlTargetElement特性，并为该特性的TagStructure属性设置为TagStructure.WithoutEndTag。

代码如下：

```c#
[HtmlTargetElement("email",TagStructure =TagStructure.WithoutEndTag)]
public class EmailTagHelper:TagHelper
{
	...
}
```

上述代码可以在Razor页面中直接使用`<email mail-to="wy" />`自结束形式的标记，需要注意的是，虽然可以使用自结束形式的标记，但是在实际页面输出时，并不能正确的显示结果：

```
<a href="mailto:wy@163.com"></a>
```

可以看到，上述的<a>标记中，并不包含内容部分，这是因为自结束形式的标记，最终输出的结果也将是自结束的，也就是会输出`<a href="mailto:wy@163.com" />`，它并不是一个有效的HTML，被浏览器解析时，就变成了内容为空的超链接。

解决方案是，在Process()方法中，设置output.TagMode的值为TagMode.StartTagAndEndTag，代码如下：

```c#
public override void Process(TagHelperContext context, TagHelperOutput output)
{
    ...
    output.TagMode = TagMode.StartTagAndEndTag;
}
```

此时，虽然在Razor页面中使用的是自结束形式的标记，但是在最终输出时，会添加HTML结束标记。

Razor页面代码如下：

```html
<email mail-to="wy" />
```

最终输出结果为：

```html
<a href="mailto:wy@163.com">wy@163.com</a>
```

注意：一旦使用HtmlTargetElement特性指定了TagStructure属性值为TagStructure.WithoutEndTag，那么在Razor页面，只能使用自结束的形式，否则运行会报错。

### 重写ProcessAsync异步方法创建标记帮助程序

和重写Process()方法类似，不同的是通过异步 GetChildContentAsync方法 返回包含 TagHelperContent 的 Task，并通过output参数获取HTML元素的内容。

```c#
public class OneTagHelper:TagHelper
{
    private const string EmailDomain = "163.com";
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        var content = await output.GetChildContentAsync();
        var target = content.GetContent() + "@" + EmailDomain;

        output.Attributes.SetAttribute("href", "mailto:" + target);
        output.Content.SetContent(target);
         
    }
}
```

### 创建以属性形式呈现的标记帮助程序

通过为标记帮助程序指定HtmlTargetElement特性，并为属性Attributes指定属性值，可以将其作为属性进行呈现。

例如，`<p bold>测试字符串</p>`，bold是一个自定义的标记帮助程序，在这里作为标记的属性进行解析，详细代码如下：

```c#
[HtmlTargetElement(Attributes = "bold")]
public class BoldTagHelper:TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll("bold");
        output.PreContent.SetHtmlContent("<strong>");
        output.PostContent.SetHtmlContent("</strong>");
    }
}
```

实际应用时，BoldTagHelper对应bold只能作为标签的属性被解析，Razor页面内容如下：

```html
<p bold>nnnnnnnnnn</p>
```

解析后，生成的HTML片段如下：

```html
<p><strong>nnnnnnnnnn</strong></p>
```

如果想要正确解析的解析下述标记内容：

```html
<bold>无效的bold标签</bold>
```

需要在BoldTagHelper中，使用[HtmlTargetElement("bold")]特性进行标注。

```c#
[HtmlTargetElement("bold")]
[HtmlTargetElement(Attributes = "bold")]
public class BoldTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll("bold");
        output.PreContent.SetHtmlContent("<strong>");
        output.PostContent.SetHtmlContent("</strong>");
    }
}
```

使用多个[HtmlTargetElement]特性修饰标记帮助程序类时，彼此之间是逻辑OR的联系；如果将多个属性添加到同一个语句中，运行时会将其视为逻辑AND。

```c#
[HtmlTargetElement("bold", Attributes = "bold")]
```

上述代码，HTML 元素必须命名为“bold”并具有名为“bold”的属性 (`<bold bold />)` 才能匹配。

也可以使用[HtmlTargetElement] 更改目标元素的名称，该名称是最终在Razor页面引用标记的名称，例如：

```c#
[HtmlTargetElement("MyBold")]
```

在Razor页面时，就可以使用`<mybold>`标记。

### 在标记帮助程序中使用模型（Model类）

这里的模型实质上就是一个自定义的C#实体类，例如：

```c#
public class WebsiteContext
{
    public Version Version { get; set; }
    public int CopyrightYear { get; set; }
    public bool Approved { get; set; }
    public int TagsToShow { get; set; }
}
```

如果想要将模型传递给标记帮助程序，只需要将模型作为该标记帮助程序的一个属性进行使用即可。

```c#
public class MyModelTagHelper:TagHelper
{
	public WebsiteContext MyInfo { get; set; }
	public override void Process(TagHelperContext context, TagHelperOutput output)
    {
    	output.TagName = "section";
        output.Content.SetHtmlContent($@"
<ul>
    <li>Version：{MyInfo.Version}</li>
    <li>Approved：{MyInfo.Approved}</li>
    <li>CopyrightYear：{MyInfo.CopyrightYear}</li>
</ul>");
		output.TagMode = TagMode.StartTagAndEndTag;
	}
}
```

该标记帮助程序在不使用[HtmlTargetElement]特性的情况下， 在Razor页面将以<my-model>作为标记的目标名称，同样属性MyInfo在Razor页面进行应用时，也以my-info作为标记属性的目标名称。

Razor页面内容如下：

```html
@{ 
    WebsiteContext mycontext = new WebsiteContext()
    {
        Version = new Version(1, 0, 0),
        CopyrightYear = 2019,
        Approved = true,
        TagsToShow = 111
    };

}
<my-model my-info="mycontext" />
```

上述在@{}代码块中，定义了一个实体对象，在应用标记时，将其作为my-info的属性值被指定。

由于在标记帮助程序中，指定了标记模式为TagMode.StartTagAndEndTag，因此最终会生成一个被<section>j标签包裹的片段，运行程序最终呈现的HTML片段如下：

```html
<section>
<ul>
    <li>Version：1.0.0</li>
    <li>Approved：True</li>
    <li>CopyrightYear：2019</li>
</ul></section>
```

### 创建条件标记帮助程序

条件标记帮助程序的实质是，在标记帮助程序类中添加了一组用来控制该标记是否可用的布尔类型的属性。

```c#
[HtmlTargetElement(Attributes =nameof(Where))]
public class WhereTagHelper:TagHelper
{
    public bool Where { get; set; }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!Where)
        {
            output.SuppressOutput();
        }
    }
}
```

上述代码中的Where属性，只有属性值为true时，才会呈现输出。

由于使用了[HtmlTargetElement(Attributes =nameof(Where))]，因此where只能作为标记的属性才能被解析。

注意：在Razor页面中，所有应用标记的属性，都是解析的是C#代码，因此属性值可以指定能够被C#理解的代码。

例如，可以直接指定值：

```c#
<div where="false">test</div>
```

(值为false，页面将看不到该div，只要为true才能看到)

也可以指定为对象的属性：

```c#
<div where="mycontext.Approved">test</div>
```

无论哪种形式，只要最终解析的结果为true，就能够显示该`<div>`内容。

通过这种方式，可以很好的控制页面元素的动态显示，也是条件标记帮助程序最常见的用途。

下面是一个很常见的使用场景的代码。

在控制器中，需要捕捉是否显示元素的业务数据，可以使用查询字符串的形式进行获取：

```c#
public IActionResult Index(bool approved = false)
{
    return View(new WebsiteContext
    {
        Approved = approved,
        CopyrightYear = 2015,
        Version = new Version(1, 3, 3, 7),
        TagsToShow = 20
    });
}
```

当查询字符串使用“?approved=true”形式的URL时（例如，`http://localhost:1235/Home/Index?approved=true`），将会捕捉到approved的参数值，在Razor页面时，就可以通过Model进行获取和使用。

```html
@model WebsiteContext
<div where="Model.Approved">
        <p>
            This website has <strong surround="em">@Model.Approved</strong> been approved yet.
            Visit www.contoso.com for more information.
        </p>
    </div>
```

### 标记帮助程序冲突

如果定义了可以同时作用于同一个标签的多个标记帮助程序，在应用多个标记帮助程序时，产生了冲突，需要对TagHelper的属性Order进行重写，该值越小，对应的优先级越高，默认为0。

```c#
public override int Order
{
	get  {  return int.MinValue;   }
}
```

另外，**特别需要注意的是**，如果在标记帮助程序中，需要对最终呈现的内容进行重绘，一定要使用output.Content.IsModified属性监测内容是否已被修改，如果已修改，则从输出缓冲区获取内容。

```c#
var childContent = output.Content.IsModified ? output.Content.GetContent() : 
    (await output.GetChildContentAsync()).GetContent();
```

这段代码应该作为所有自定义标记帮助程序中，必不可少的代码片段。

### 检索和处理标记内容

标记帮助程序的主要作用就是对标记中的内容，按照设定的方式或格式进行展示和处理。重写的ProcessAsync()方法中，提供了多种检索标记内容的方式。

- 可将 GetChildContentAsync 的结果追加到 output.Content。

- 可使用 GetContent() 获取GetChildContentAsync 的结果。

- 如果直接修改output.Content，则不会执行或呈现 TagHelper 主体，除非调用GetChildContentAsync方法。

  ```c#
  public class AutoLinkerHttpTagHelper : TagHelper
  {
      public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
      {
          var childContent = output.Content.IsModified ? output.Content.GetContent() :
              (await output.GetChildContentAsync()).GetContent();
  
          // Find Urls in the content and replace them with their anchor tag equivalent.
          output.Content.SetHtmlContent(Regex.Replace(
               childContent,
               @"\b(?:https?://)(\S+)\b",
                "<a target=\"_blank\" href=\"$0\">$0</a>"));  // http link version}
      }
  }
  ```

- 对 GetChildContentAsync 的多次调用返回相同的值，且不重新执行 TagHelper 主体，除非传入一个指示不使用缓存结果的 false 参数。

### 补充：标记帮助程序的默认约定概述

- 创建标记帮助程序的类，推荐类名以TagHelper结尾。
- 在未使用HtmlTargetElement特性指定标记目标名称的情况下，约定使用不包含后缀“TagHelper”的类名的小写形式作为标记的目标名称。如果类名由多个首字母大写的单词组成，那么使用短横线分割各个单词的形式，作为标记的目标名称。例如：MyEmailTagHelper，就使用`<my-email>`作为标记的目标名称。同时，该规则也同样适用于标记帮助程序中的属性。



## ASP.NET Core中内置的标记帮助程序

### `<a>`

该标记最终会生成一个超链接标签（`<a href="..."></a>`）。

为了说明该标记帮助程序的详细用法，首先创建一个控制器：

```c#
public class Speaker
{
    public int SpeakerId { get; set; }
}

public class SpeakerController : Controller
{
    private List<Speaker> Speakers = new List<Speaker>
    {
        new Speaker{ SpeakerId=1 },
        new Speaker{ SpeakerId=2 },
        new Speaker{ SpeakerId=3 }
    };

    public IActionResult Index()
    {
        return View(Speakers);
    }

    [Route("/Speaker/Evaluations", Name = "speakerevals")]
    public IActionResult Evaluations()
    {
        return View();
    }

    [Route("/Speaker/EvaluationsCurrent", Name = "speakervalscurrent")]
    public IActionResult Evaluations(int speakerId, bool currentYear)
    {
        return View();
    }

    [Route("Speaker/{id:int}")]
    public IActionResult Detail(int id)
    {
        return View(Speakers.FirstOrDefault(a => a.SpeakerId == id));
    }

    [Route("Speaker")]
    public IActionResult Detail2(int speakerid)
    {
        return View(Speakers.FirstOrDefault(a => a.SpeakerId == speakerid));
    }
    
    public IActionResult Tag()
    {
        return View();
    }

}
```

系统提供的<a>标记帮助程序的可用属性均以asp-开头，其中以asp-page开头的属性，均只能适用于Razor Pages页面，而不是Razor视图。

**说明：`<a>`标记帮助程序的使用示例，都是作用在Home/Tag视图页面，对应的操作方法为Home控制器下的Tag方法。**

Home控制器的内容如下：

```c#
public class HomeController : Controller
{
    public IActionResult Tag()
    {
        return View();
    }

    public IActionResult Test()
    {
        return View();
    }
}
```

#### asp-controller

asp-controller属性指定用于生成URL的控制器。

```html
<a asp-controller="Speaker">只使用asp-controller示例</a>
```

上述代码没有指定asp-action属性，默认以当前视图对应的控制器操作方法为主，由于是在Home/Tag下访问的Razor视图页面，因此asp-action默认就以Home控制器下的Tag操作方法为主，最终生成的HTML片段内容如下：

```html
<a href="/Speaker/Tag">只使用asp-controller示例</a>
```

注意：Speaker控制器中一定要存在Tag操作方法，否则仍然无法得到上述结果。

#### asp-action

asp-action属性指定用于生成URL的控制器中的操作方法名称，该操作方法必须是真实存在的。

```html
<a asp-controller="Speaker" asp-action="Evaluations">asp-action的使用</a>
```

生成的HTML：

```html
<a href="/Speaker/Evaluations">asp-action的使用</a>
```

如果没有指定asp-controller属性，则默认以当前呈现视图的控制器为主。

```html
<a asp-action="Test">只使用asp-action的示例</a>
```

上述代码当前呈现的视图是Home/Tag，因此在不指定asp-controller属性的情况下，默认也以Home为主，最终生成的HTML如下：

```html
<a href="/Home/Test">只使用asp-action的示例</a>
```

**注意：最终应用的控制器操作方法一定要在控制器中真实的存在，否则不能正确解析。**

#### asp-route-{value}

该属性用于指定路由参数，如果在路由模板中找不到该指定的参数，就将该请求参数和值追加到生成的href属性，否则，将在路由模板中替换该值。

在Speaker控制器中存在下述的操作方法：

```c#
[Route("Speaker/{id:int}")]
public IActionResult Detail(int id)
{
    return View(Speakers.FirstOrDefault(a => a.SpeakerId == id));
}
```

若要正确的解析出路由模板中的id参数，需要为其设置asp-route-id属性值：

```html
<a asp-controller="Speaker" asp-action="Detail" asp-route-id="2">
    asp-route-{value}的使用</a>
```

生成的HTML也将依据指定的模板对href属性进行呈现：

```html
<a href="/Speaker/2">asp-route-{value}的使用</a>
```

如果路由模板中没有指定参数，或者没有使用Route特性指定路由模板，如果需要在控制器中接受Razor页面中指定的asp-route-{value}属性对应的参数值，例如：

```html
<a asp-controller="Speaker" asp-action="Detail2" asp-route-speakerid="2">
	asp-route-{value}的使用子自定义参数
</a>
```

要想在控制器中解析并获取speakerid参数的值，需要在控制器的操作方法中，定义对应的参数：

```c#
[Route("Speaker")]
public IActionResult Detail2(int speakerid)
{
    return View(Speakers.FirstOrDefault(a => a.SpeakerId == speakerid));
}
```

由于没有在路由模板中指定参数，因此最终生成的HTML如下：

```html
<a href="/Speaker?speakerid=2">asp-route-{value}的使用子自定义参数</a>
```

指定的asp-route-speakerid属性中的参数speakerid将以查询字符串的形式追加到url的末尾。

#### asp-route

asp-route用于创建直接链接到命名路由的URL。所谓命名路由指的是使用了Route特性并指定Name属性的路由。

```c#
[Route("/Speaker/Evaluations", Name = "speakerevals")]
public IActionResult Evaluations()
{
    return View();
}
```

在Razor页面，使用asp-route属性示例如下：

```html
<a asp-route="speakerevals">asp-route的使用</a>
```

上述的asp-route属性指定路面名称，生成的HTML如下，其中href属性值为路由模板对应的url：

```html
<a href="/Speaker/Evaluations">asp-route的使用</a>
```

需要注意的是，由于asp-controller或asp-action属性，都可以生成相应的路由，**因此为了避免路由冲突，不应该将asp-route与asp-controller或asp-action属性组合使用**，否则将不会生成预期的路由。

#### asp-all-route-data

asp-all-route-data属性支持创建键值对字典，键是参数名称，值时参数值。



### `<cache>`

### `<distributed-cache>`

### `<environment>`

### `<form>`

### `<img>`

### `<input>`

### `<label>`

### `<partial>`

### `<select>`

### `<textarea>`

### 验证消息（`<span>`）

### 验证摘要（`<span>`）

































## ASP.NET Core 表单中的标记帮助程序





















