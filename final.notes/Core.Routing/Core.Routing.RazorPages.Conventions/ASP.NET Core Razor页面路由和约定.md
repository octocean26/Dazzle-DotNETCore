# ASP.NET Core Razor页面路由和约定

**本文中的术语说明**

- 路由：本文中的路由均指的是在Razor页面应用下的路由，非MVC控制器路由，当然中间有与MVC控制器路由相关联或相同的地方。在ASP.NET Core的内部，Razor Pages 路由和 MVC 控制器路由共享一个实现。
- 应用程序模型：用于表示Web 应用程序的各个组件的抽象接口和具体实现类，通过使用应用程序模型，可以修改应用以遵循与默认行为不同的约定。
- 约定：英文名Convention，默认情况下，Web应用（例如MVC应用程序）遵循特定的约定，以确定将哪些类（模型）视为控制器，这些类上的哪些方法是操作，以及参数和路由的行为方式。可以创建自己的约定来满足应用的需要，将它们应用于全局或作为属性应用。

**快速理解技巧**

- 如果类名、接口名、方法名、属性名等，只要名称中出现“Convention”，都和“约定”有关。
- 如果名称中出现“ModelConvention”，都和“模型约定”相关。

**本文关联的成员**

成员的代码来源：

```c#

```

Microsoft.AspNetCore.Mvc.ApplicationModels.IPageRouteModelConvention：

Microsoft.AspNetCore.Mvc.ApplicationModels.IPageApplicationModelConvention：

Microsoft.AspNetCore.Mvc.ApplicationModels.IPageHandlerModelConvention：







IPageConvention

options.Conventions







## 模型约定（Model Convention）

TODO：模型约定要如何理解最容易。

IPageRouteModelConvention：路由模型约定

IPageApplicationModelConvention：应用程序模型约定

IPageHandlerModelConvention：处理程序模型约定





### 路由模型约定

### 应用模型约定

### 处理程序模型约定



## 页面路由操作约定

AddFolderRouteModelConvention



### 文件夹路由模型约定

### 页面路由模型约定



## 参数转换器用于自定义页面路由



## 配置页面路由



## 页面模型操作约定

### 文件夹应用模型约定

### 页面应用模型约定

### 配置筛选器

### 配置筛选器工厂



## 替换默认页面应用模型提供程序

### 默认的未命名处理程序方法

### 默认的已命名处理程序方法

### 自定义处理程序方法名称



## MVC 筛选器和页面筛选器 (IPageFilter)



















## 自定义路由