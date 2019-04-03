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



