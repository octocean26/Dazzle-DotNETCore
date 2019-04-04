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

例如，`<p bold>测试字符串</p>`，bold是一个自定义的标记帮助程序，在这里作为标记的属性进行解析，【待续】



### 标记帮助程序的默认约定概述

- 创建标记帮助程序的类，推荐类名以TagHelper结尾。
- 在未使用HtmlTargetElement特性指定标记目标名称的情况下，约定使用不包含后缀“TagHelper”的类名的小写形式作为标记的目标名称。如果类名由多个首字母大写的单词组成，那么使用短横线分割各个单词的形式，作为标记的目标名称。例如：MyEmailTagHelper，就使用`<my-email>`作为标记的目标名称。同时，该规则也同样适用于标记帮助程序中的属性。



















