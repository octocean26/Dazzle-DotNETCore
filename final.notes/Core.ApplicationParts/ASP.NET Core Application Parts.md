# ASP.NET Core中的应用程序部件

**名词描述**

应用程序部件（Application Part）： 对应的C#类名为ApplicationPart，AssemblyPart 类表示受程序集支持的应用程序部件。 

**代码引用来源**

```
services.AddMvc().AddApplicationPart(...);
或
services.AddMvc()
    .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(...));
```



## 应用程序部件介绍

应用程序部件从代码层面上来讲，就是一系列派生自抽象类ApplicationPart的后代类（例如AssemblyPart类，就是一种应用程序部件，用于封装程序集引用以及公开类型和编译引用）。

MVC应用可以通过应用程序部件发现和加载MVC功能，比如控制器、视图组件、标记帮助程和Razor编译源等。

应用程序部件是受程序集支持的，它的主要用途是：允许将应用配置为从程序集中发现（或避免加载）MVC 功能。





 ApplicationPartManager 负责跟踪可用于 MVC 应用的应用程序部件和功能提供程序。 