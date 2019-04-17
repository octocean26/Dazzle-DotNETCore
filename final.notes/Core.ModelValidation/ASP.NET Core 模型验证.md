# ASP.NET Core 模型验证

模型（Model）是一个经常被提到的词汇，无论是Entity Framework中的模型，还是ASP.NET Core中的模型，它的实质都是一个C#实体类。在Entity Framework中，模型实体类是用于映射数据库表结构的自定义类，实现数据的存储和处理。在ASP.NET Core中，模型实体类是一个自定义的ViewModel类，用于视图的数据绑定和呈现，这个自定义的ViewModel类也可以直接使用数据库操作（例如Entity Framework）的实体类（通常不建议这么做，而是自定义一个ViewModel类，用于模型绑定）。

模型验证指的是为了防止错误的或存在安全威胁的数据被处理，而在模型进行绑定时所做的验证。







如果 Web API 控制器具有 `[ApiController]` 特性，则它们不必检查 `ModelState.IsValid`。 在此情况下，如果模型状态无效，将返回包含问题详细信息的自动 HTTP 400 响应。

























## ViewModel模型验证



