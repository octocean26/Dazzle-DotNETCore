# ASP.NET Core访问应用程序数据



## .NET Core中的数据访问

在ASP.NET Core应用程序中进行数据访问时，经常会想到的第一个选项是使用Entity Framework Core（EF Core）。 本文将介绍使用EF Core可能执行的基本和最常见的任务。 

### Entity Framework 6.x

实体框架6.x（Entity Framework 6）是多年来我们在.NET应用程序中用于数据访问任务的忠实的O/RM框架。 EF6仅与新的.NET Core平台部分兼容，EF6不完全支持.NET Core。并且仅限于在Windows下运行。

注意，在Windows下运行时，ASP.NET Core应用程序可以在IIS下托管，但它也可以托管在Windows服务中并在Kestrel上运行。即使您丢失了IIS的高级服务，它也会非常高效。但与此同时，您并不总是需要这些服务。像往常一样，这是一个权衡问题。

### 在单独的类库中包装EF6代码

在ASP.NET Core应用程序中使用EF6的推荐方法是将所有类（包括DB上下文和实体类）放在单独的类库项目中，并使其针对完整的框架。接下来，对此项目的引用将添加到新的ASP.NET Core项目中。这个附加步骤是必需的，因为ASP.NET Core项目不支持您可以在EF6上下文类中以编程方式触发的所有功能。因此，不支持在ASP.NET Core项目中直接使用EF6上下文类。

#### 检索连接字符串

EF6上下文类检索其连接字符串的方式与ASP.NET Core的最新且完全重写的配置层不完全兼容。让我们考虑以下常见代码片段。

```c#
public class MyOwnDatabase : DbContext
{
   public MyOwnDatabase(string connStringOrDbName = "name=MyOwnDatabase")
       : base(connStringOrDbName)
   {
   } 
}
```

特定于应用程序的Db上下文类接收连接字符串作为参数或从web.config文件中检索它。在ASP.NET Core中，没有什么比web.config文件更好，因此连接字符串要么变成常量，要么应该通过.NET Core配置层读取并传入。

### 将EF Context与ASP.NET Core DI集成

您在Web上找到的大多数ASP.NET Core数据访问示例都显示了如何通过依赖注入（DI）将DB上下文注入应用程序的所有层。您可以像在任何其他服务中一样在DI系统中注入EF6上下文。理想的作用域是每个请求，这意味着同一个HTTP请求中所有可能的调用者共享同一个实例。

```c#
public void ConfigureServices(IServiceCollection services)
{
    // 这里添加的其他服务
    ...

    // 从配置中获取连接字符串
    var connString = ...;
    services.AddScoped<MyOwnDatabase>(() => new MyOwnDatabase(connString));
}
```

有了上面的配置，您现在可以将EF6 DB上下文直接注入控制器，或者更有可能将其注入存储库类。

```c#
public class SomeController : Controller
{
    private readonly MyOwnDatabase _context;
    public SomeController(MyOwnDatabase context)
    {
        _context = context;
    }

    // More code here
    ...
}
```

### ADO.NET适配器 TODO：

在ASP.NET Core 2.0中，Microsoft带回了旧的ADO.NET API的一些组件，特别是DataTable对象，数据读取器和数据适配器。虽然始终支持作为.NET Framework的组成部分，但近年来ADO.NET经典API在开发支持Entity Framework的新应用程序时逐渐被放弃。因此，它在.NET Core API 1.x的设计中被牺牲，然后在版本2.0中受到大众需求的带回。因此，在ASP.NET 2.0应用程序中，您可以编写数据访问代码来管理连接，SQL命令和游标，就像在.NET时代开始时一样。

#### 发出直接SQL命令

在ASP.NET Core中，ADO.NET API具有与完整.NET Framework中几乎相同的编程接口，并且具有相同的编程范例。首先，您可以通过管理与数据库的连接并以编程方式创建命令及其参数来完全控制每个命令。这是一个例子：

```c#
var conn = new SqlConnection();
conn.ConnectionString = "...";
var cmd = new SqlCommand("SELECT * FROM customers", conn);
```

准备就绪后，必须通过打开的连接发出命令。为实现这一目标，还需要更多代码。

```c#
conn.Open();
var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

// Read data and do any required processing
...
reader.Close();
```

由于打开数据读取器时请求的紧密连接行为，因此在关闭阅读器时会自动关闭连接。 SqlCommand类可以通过各种方法执行命令，如表9-1中所述。

表9-1 SqlCommand类的执行方法

| 运行             | 描述                                                         |
| ---------------- | ------------------------------------------------------------ |
| ExecuteNonQuery  | 执行命令但不返回任何值。非UPDATE等非查询语句的理想选择。     |
| ExecuteReader    | 执行命令并返回指向输出流开头的游标。非常适合查询命令。       |
| ExecuteScalar    | 执行命令并返回单个值。非常适合返回标量值（如MAX或COUNT）的查询命令。 |
| ExecuteXmlReader | 执行命令并返回XML阅读器。非常适合返回XML内容的命令。         |

表9-1中的方法提供了多种选项来获取要执行的任何SQL语句或存储过程的结果。这是一个示例，说明如何浏览数据读取器的记录。

```c#
var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
while(reader.Read())
{
    var column0 = reader[0];	             // returns an Object
    var column1 = reader.GetString(1)    // index of the column to read
    // Do something with data
}
reader.Close();
```

注意.NET Core中的ADO.NET API与.NET Framework中的API相同，并且不支持SQL Server区域中的更新近期开发，例如SQL Server 2016及更高版本中的本机JSON支持。例如，没有像ExecuteJsonReader方法那样将JSON数据解析为类。

#### 在断开的容器中加载数据

如果您需要在保持最小内存量的同时处理长响应，则使用阅读器是理想的选择。否则，最好将查询结果加载到断开连接的容器（如DataTable对象）中。这里有一些设施。

```c#
conn.Open();
var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
var table = new DataTable("Customers");
table.Columns.Add("FirstName");
table.Columns.Add("LastName");
table.Columns.Add("CountryCode");
table.Load(reader);
reader.Close();
```

DataTable对象是具有架构，关系和主键的数据库表的内存中版本。最简单的方法是获取数据读取器光标并在声明的列中加载整个内容。映射按列索引进行，Load方法后面的实际代码非常接近前面介绍的循环。但是，从您的角度来看，它只需要一种方法，但仍然需要您负责管理数据库连接的状态。因此，通常，您可以采取的最安全的方法是使用Dispose模式并在C＃using语句中创建数据库连接。

#### 通过适配器获取

将数据提取到内存容器中的最紧凑方法是通过数据适配器。数据适配器是汇总整个查询过程的组件。它由命令对象或仅选择命令文本和连接对象组成。它负责为您打开和关闭连接，并将查询的所有结果（包括多个结果集）打包到DataTable或DataSet对象中。 （DataSet是DataTable对象的集合。）

```c#
var conn = new SqlConnection();
conn.ConnectionString = "...";
var cmd = new SqlCommand("SELECT * FROM customers", conn);
var table = new DataTable();
var adapter = new SqlDataAdapter(cmd);
adapter.Fill(table);
```

同样，如果您熟悉ADO.NET API，您将在.NET和ASP.NET Core 2.0中找到它，就像它最初一样。这可以保证可以将另外一段遗留代码移植到其他平台。除此之外，对ADO.NET的支持还提供了另一个机会，可以在.NET Core和ASP.NET Core中使用SQL Server 2016的更高级功能，例如JSON支持和更新历史。实际上，对于这些功能，您无需EF6或EF Core的临时支持。

### 使用Micro O / RM框架

O / RM框架执行查询数据行并将它们映射到内存中对象属性的肮脏和值得称道的工作。与上面讨论的DataTable对象相比，O / RM将相同的低级数据加载到强类型类而不是通用的面向表的容器中。对于.NET Framework的O / RM框架，大多数开发人员都会想到Entity Framework或者NHibernate。这些是最受欢迎的框架，但也是最庞大的框架。对于O / RM框架，gigantic的属性与它支持的功能的数量有关，从映射功能到缓存，从事务性到并发。在现代的O / RM for .NET中，对LINQ查询语法的支持至关重要。它产生了很多功能，这些功能不可避免地会影响内存占用，甚至影响单个操作的性能。这就是为什么一些人和公司最近开始使用微型O / RM框架的原因。 ASP.NET Core应用程序存在一些选项。

#### 微型O / RM与完整O / RM

面对现实吧。微型O / RM与完整的O / RM完成相同的基本工作，大多数情况下，您并不需要完全成熟的O / RM。想要一个例子吗？ Stack Overflow是地球上交易量最大的网站之一，它不使用完整的O / RM。 Stack Overflow甚至设法创建他们自己的微型O / RM只是出于性能原因。话虽如此，我个人的感觉是，大多数应用程序仅使用Entity Framework，因为它是.NET Framework的一部分，因为它使编写查询的问题与C＃代码而不是SQL有关。生产力确实很重要，总的来说，我倾向于考虑使用完整的O / RM作为更有成效的选择，因为示例和功能的数量，包括内部优化命令，以确保始终进行充分的权衡。

如果微型O / RM可以拥有更小的内存占用，那么它主要是因为它缺乏功能。问题是任何缺少的功能是否会影响您的应用程序。主要缺少的功能是二级缓存和内置的关系支持。二级缓存是指具有由框架管理的附加缓存层，该缓存层在连接和事务之间保持配置的时间量。 NHibernate支持二级缓存，但实体框架中不支持二级缓存（尽管有些解决方法可以在EF6中实现，并且EF Core存在扩展项目）。这就是说，二级缓存在微观和完整的O / RM框架之间并不是一个很大的区别。更为相关的是其他缺失的功能 - 支持关系.

当您在EF中编写查询时，无论基数如何，您都可以在查询中包含任何外键关系。将查询结果扩展到连接表是语法的一部分，不需要通过不同且更清晰的语法构建查询。您通常不会使用微型O / RM。在微型O / RM中，这正是您进行权衡的关键所在。您可以花费更多时间编写需要更高级SQL技能的更复杂查询，从而提高操作性能。或者，您可以跳过SQL技能并让系统为您完成工作。这种来自框架的额外服务是以内存占用和整体性能为代价的。

此外，完整的O / RM可以提供不是每个人都喜欢和使用的设计者和/或迁移设施，这有助于使整个O / RM的图像更加巨大。

#### 微型O / RM 样品

Stack Overflow团队选择创建一个量身定制的迷你O / RM- Dapper框架 - 负责编写超级优化的SQL查询并添加大量的外部缓存层。 Dapper框架可从http://github.com/StackExchange/Dapper获得。该框架在针对SQL数据库执行SELECT语句并将返回的数据映射到对象时发光。它的性能几乎与使用数据读取器相同 - 这是在.NET中查询数据的最快方式，但它可以返回内存中对象的列表。

```c#
var customer = connection.Query<Customer>(
           "SELECT * FROM customers WHERE Id = @Id", 
           new { Id = 123 });
```

NPoco框架遵循相同的指导原则，甚至代码与Dapper的差别也很小。 NPoco框架可在http://github.com/schotime/npoco上找到。

```c#
using (IDatabase db = new Database("connection_string"))  
{ 
    var customers = db.Fetch<Customer>("SELECT * FROM customers"); 
}
```

微型O / RM系列每天都在增长，其他许多用于ASP.NET Core，例如Insight.Database（http://github.com/jonwagner/Insight.Database）和PetaPoco，它们作为单个提供要整合到您的应用程序中的大文件（http://www.toptensoftware.com/petapoco）。

然而，关于微型O / RM的关键不在于您应该使用哪种，而是使用微型O / RM而不是完整的O / RM。

说明根据Stack Overflow工程师在Dapper主页（http://github.com/StackExchange/Dapper）上发布的数字，性能方面，Dapper在单个查询上的速度比实体框架快10倍。这是一个巨大的差异，但不一定足以让每个人决定使用Dapper或其他微型O / RM。这种选择取决于您运行的查询数量以及编写它们的开发人员的技能，以及您必须提高性能的替代方案。

### 使用NoSQL商店

NoSQL这个术语意味着很多东西并指向许多不同的产品。最后，NoSQL可以概括为当你不想要或不需要关系存储时它是所选择的数据存储范例。总而言之，当您真正想要使用NoSQL存储时，只有一个用例：当记录的模式发生变化但记录在逻辑上相关时。

考虑填写和存储在多租户应用程序中的表单或问卷。每个租户都可以拥有自己的字段列表，您需要为各种用户保存值。每个租户表单可能不同，但结果记录在逻辑上都是相关的，理想情况下应该在同一个商店中。在关系数据库中，除了创建作为所有可能字段的并集的模式之外，您几乎没有其他选项。但即使在这种情况下，为租户添加新字段也需要更改表的架构。按行而不是按列组织数据会带来其他问题，例如每次租户查询超过SQL页面大小时的性能命中。同样，它取决于具体的应用程序使用情况，但事实上，无模式数据对于关系存储来说并不理想。输入NoSQL商店。

如上所述，有许多方法可以对NoSQL商店进行编目。对于本书，我更喜欢将它们分成物理和内存存储。尽管存在物理/内存对比，但区别非常薄。 NoSQL存储主要用作缓存形式，而不太常用作主数据存储。当它们被用作主要数据存储时，通常是因为应用程序具有事件源架构。

#### 经典实体店

物理NoSQL存储是一种无模式数据库，可将.NET Core对象保存到磁盘，并提供获取和过滤它们的功能。最受欢迎的NoSQL商店可能是MongoDB，它与微软的Azure DocumentDB密切相关。有趣的是，只需更改连接字符串，就可以使用MongoDB API编写的应用程序写入DocumentDB数据库。这是为DocumentDB编写的示例查询。

```c#
var client = new DocumentClient(azureEndpointUri, password);
var requestUri = UriFactory.CreateDocumentCollectionUri("MyDB", "questionnaire-items");
var questionnaire = client.CreateDocumentQuery<Questionnaire>(requestUri) 
        .Where(q => q.Id == "tenant-12345" && q => q.Year = 2018) 
        .AsEnumerable() 
        .FirstOrDefault();
```

NoSQL商店的主要优点是能够存储形状不同但相关的数据和规模存储以及简单的查询功能。其他物理NoSQL数据库是RavenDB，CouchDB和CouchBase，它们特别适用于移动应用程序。

#### 内存存储

内存存储本质上是大型缓存应用程序，可用作键值字典。即使它们备份内容，它们也被视为大块内存，其中应用程序驻留数据以便快速检索。 Redis（http://redis.io）是内存存储的一个很好的例子。

要了解此类框架的相关性，请再次考虑Stack Overflow公开记录的体系结构。 Stack Overflow（www.stackoverflow.com）使用Redis的自定义版本作为中间二级缓存，以长时间维护问题和数据，而无需从数据库重新查询。 Redis支持磁盘级持久性，LRU驱逐，复制和分区。 Redis不能直接从ASP.NET Core访问，但可以通过ServiceStack API完成（请参阅http://servicestack.net）。

另一个内存中的NoSQL数据库是Apache Cassandra，它可以通过DataStax驱动程序在ASP.NET Core中访问。



## EF核心常见任务

如果您打算保留ASP.NET Core的完整O / RM领域，则选择仅限于新的，定制的Entity Framework版本，即EF Core。 EF Core支持一种提供程序模型，通过它可以使用各种关系DBMS，特别是SQL Server，Azure SQL数据库，MySQL和SQLite。对于所有这些数据库，EF Core都有一个本机提供程序。此外，存在内存提供程序，这有助于测试目的。对于PostgreSQL，您需要来自http://npgsql.org的外部提供程序。预计2018年初将推出针对EF Core的Oracle提供商。

要在ASP.NET Core应用程序中安装EF Core，您需要Microsoft.EntityFrameworkCore包以及您打算使用的数据库提供程序的特定包（SQL Server，MySQL，SQLite或其他）。您将执行的最常见任务如下所列。

### 建模数据库

EF Core仅支持Code First方法，这意味着它需要一组类来描述数据库和包含的表。这些类的集合可以从头开始编码，也可以通过现有数据库中的工具进行反向工程。

#### 定义数据库和模型

最后，数据库是在从DbContext派生的类之后建模的。此类包含一个或多个类型为DbSet <T>的集合属性，其中T是表中记录的类型。这是示例数据库的结构。

```c#
public class YourDatabase : DbContext
{
   public DbSet<Customer> Customers { get; set; }
}
```

Customer类型描述Customers表的记录。底层物理关系数据库应该有一个名为Customers的表，其模式与Customer类型的公共接口匹配。

```c#
public class EntityBase
{
    public EntityBase()
    {
        Enabled = true;
        Modified = DateTime.UtcNow;
    }
    public bool Enabled { get; set; }
    public DateTime? Modified { get; set; }
}
public class Customer : EntityBase
{
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

在布置Customer类的公共接口时，您仍然可以使用常见的面向对象技术并使用基类来跨所有表共享公共属性。在该示例中，Enabled和Modified是两个属性，自动添加到其映射类继承自EntityBase的所有表中。另请注意，任何将生成表的类都必须定义主键字段。例如，您可以通过Key属性执行此操作。

重要数据库的模式和映射的类必须始终保持同步;否则，EF Core会抛出异常。这意味着即使向表中添加新的可空列也可能是个问题。同时，向其中一个类添加公共属性可能是个问题。但是，在这种情况下，NotMapped属性可以避免您获得异常。事实上，EF Core倾向于假设您只通过其迁移脚本与物理数据库进行交互。迁移脚本是保持模型和数据库同步的官方方式。但是，迁移主要是开发人员的事情，而数据库通常是IT部门的财产。在这种情况下，模型和数据库之间的迁移只能是手动的。

#### 注入连接字符串

在上面的代码中，没有任何内容显示代码和数据库之间的物理链接。你会如何注入连接字符串？从技术上讲，DbContext派生类没有完全配置为在指示提供程序之前对数据库起作用，并且所有信息都要运行它 - 最明显的是连接字符串。您可以设置提供程序覆盖DbContext类的OnConfiguring方法。该方法接收一个选项构建器对象，其中包含每个本机支持的提供程序的扩展方法：对于SQL Server，SQLite以及仅测试的内存数据库。若要配置SQL Server（包括SQL Express和Azure SQL数据库），请按以下步骤操作。

```c#
public class YourDatabase : DbContext
{
   public DbSet<Customer> Customers { get; set; }

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       optionsBuilder.UseSqlServer("...");
   }
}
```

UseSqlServer的参数必须是连接字符串。如果连接字符串是常量是可接受的，您只需在上面的代码片段中看到省略号的位置键入它。更实际的是，您希望根据环境生产，登台，开发等使用不同的连接字符串。在这种情况下，您应该找到一种方法来注入它。因为连接字符串不会动态更改（如果它发生更改，这是一个非常特殊的情况，值得区别对待），首先想到的选项是将全局静态属性添加到要设置的DbContext类中连接字符串。

```c#
public static string ConnectionString = "";
```

现在，ConnectionString属性以静默方式传递给OnConfiguring方法中的UseSqlServer方法。通常从配置文件中读取连接字符串，并在应用程序启动时设置。

```c#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    YourDatabase.ConnectionString = !env.IsDevelopment()
        ? "production connection string"
        : "development connection string";

    // More code here
}
```

同样，您可以使用不同的JSON配置文件进行生产和开发，并存储要使用的各个连接字符串。从DevOps角度来看，这种方法也可能更容易，因为发布脚本只是按照惯例选择了正确的JSON文件。 （请参阅第2章“第一个ASP.NET核心项目。”）

#### 注入DbContext对象

如果您搜索EF Core文章（包括Microsoft官方文档），您会看到许多示例，其中显示了遵循以下准则的代码。

```c#
public void ConfigureServices(IServiceCollection services)
{
     var connString = Configuration.GetConnectionString("YourDatabase");
     services.AddDbContext<YourDatabase>(options =>
            options.UseSqlServer(connString));
}
```

代码将YourDatabase上下文对象添加到DI子系统，以便可以从应用程序的任何位置检索它。在添加上下文时，代码还完全配置了当前请求的范围，并且在示例中，在给定的连接字符串上使用SQL Server提供程序。

或者，您可以自己创建数据库上下文的实例，并为其提供所需的生命周期（例如，单例或作用域），并仅在上下文中注入连接字符串。上面讨论的静态属性是一个选项。这是另一个。

```c#
public YourDatabase(IOptions<GlobalConfig> config)
{
    // Save to a local variable the connection string 
    // as read from the configuration JSON file of the application.
}
```

如第7章“设计注意事项”中所述，您可以应用选项模式并将JSON资源中的全局配置数据加载到类中，并通过DI将该类注入类的构造函数中。

在注入连接字符串的许多方法中，您应该选择哪一种？就个人而言，我使用静态属性，因为它简单，直接，易于理解和弄清楚。我的第二个最喜欢的方法是将配置注入DbContext。至于将完全配置的DbContext注入DI系统，这让我感到害怕，因为它可能导致开发人员在他们可能需要的任何地方调用DbContext，从而无法分离任何关注点。

#### 自动创建数据库

对数据库建模并将其映射到类的整个过程与EF6略有不同;创建数据库所需的代码（如果它还不存在）也是。在EF Core中，必须明确请求此步骤，而不是数据库初始化程序组件基础的结果。如果要创建数据库，请在Configure方法的启动类中放置以下两行代码：

```c#
var db = new YourDatabase();
db.Database.EnsureCreated();
```

如果数据库不存在，则EnsureCreated方法创建数据库（否则跳过）。将初始数据加载到数据库也在您的完全程序控制之下。一个常见的模式是公开一个公共方法 - 这个名称由DbContext类决定，并在EnsureCreated之后立即调用它。

```c#
db.Database.SeedTables();
```

在初始化程序内部，您可以直接调用EF Core方法，也可以调用存储库（如果已定义它们）。

注意可以通过许多命令行工具控制脚手架任务，例如对现有数据库进行反向工程或将更改从类迁移到数据库。更多细节可以在这里找到：http：//docs.microsoft.com/en-us/ef/core/get-started/aspnetcore/existing-db。

### 使用表数据

使用EF Core读取和写入数据在很大程度上与EF6中的相同。从现有数据库正确创建或反向设计数据库后，查询和更新的工作方式相同。 EF6和EF Core API存在一些差异，但总的来说，我认为最好的方法是尝试做与EF6相同的事情，并且只有当它们发生时才关注异常。

#### 获取记录

以下代码显示如何通过其主键获取记录。实际上，这种方法更为通用，并展示了如何通过条件获取记录。

```c#
public Customer FindById(int id)
{
    using (var db = new YourDatabase())
    {
        var customer = (from c in db.Customers
                        where c.Id == id
                        select c).FirstOrDefault();
        return customer;
    }
}
```

有两件事比代码本身更有意义。

- 首先，代码封装在由存储库类公开的方法中。存储库类是一个包装类，它使用DbContext的新实例或注入的副本（由您决定）来公开特定于数据库的操作。
- 第二个相关的事情是上面的代码是一种整体。它打开与数据库的连接，检索其数据并关闭连接。这一切都发生在单个透明数据库事务的上下文中。如果需要运行两个不同的查询，请考虑对存储库方法的两次调用将打开/关闭与数据库的连接两次。

如果您编写的业务流程需要来自数据库的两个或更多查询，您可能希望尝试在单个透明事务中连接它们。 DbContext实例的范围确定系统创建的数据库事务的范围。

```c#
public Customer[] FindAdminAndSupervisor()
{
    using (var db = new YourDatabase())
    {
        var admin = (from c in db.Customers
                        where c.Id == ADMIN
                        select c).FirstOrDefault();
        var supervisor = (from c in db.Customers
                        where c.Id == SUPERVISOR
                        select c).FirstOrDefault();
        return new[] {admin, supervisor};
    }
}
```

在这种情况下，两个记录是通过不同的查询检索的，但是在同一个事务中并通过相同的连接。另一个有趣的用例是整个查询是零敲碎打的。假设一个方法获取一块记录，然后将输出传递给另一个方法，以根据运行时条件进一步限制结果集。这是一些示例代码：

```c#
// Opens a connection and returns all EU customers
var customers = FindByContinent("EU");

// Runs an in-memory query to select only those from EAST EU
if (someConditionsApply())
{
    customers = (from c in customers where c.Area.Is("EAST") select c).ToList();
}
```

最后，您可以得到您所需要的，但内存的使用并不是最佳的。这是一个更好的方法。

```c#
public IQueryable<Customer> FindByContinent(string continent)
{
    var customers = (from c in db.Customers 
                     where c.Continent == continent
                     select c);


    // No query is actually run at this point! Only the formal 
    // definition of the query is returned.
    return customers;
}
```

在查询表达式的末尾不调用FirstOrDefault或ToList实际上不会运行查询;相反，它只是返回它的形式描述。

```c#
// Opens a connection and returns all EU customers
var query = FindByContinent("EU");

// Runs an in-memory query to select only those from EAST EU
if (someConditionsApply())
{
    query = (from c in query where c.Area.Is("EAST") select c;
}
var customers = query.ToList();
```

第二个过滤器现在只是编辑查询添加一个额外的WHERE子句。接下来，当调用ToList时，查询将运行一次，并从欧洲获得所有也位于东部的客户。

#### 处理关系

以下代码定义了两个表之间的一对一关系。 Customer对象引用Countries表中的Country对象。

```c#
public class Customer : EntityBase
{
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    [ForeignKey]
    public int CountryId { get; set; }
    public Country Country { get; set; }
}
```

这足以让数据库定义表之间的外键关系。查询客户记录时，可以轻松地通过基础JOIN语句扩展Country属性。

````c#
var customer = (from c in db.Customers.Include("Country")
                where c.Id == id
                select c).FirstOrDefault();
````

由于包含调用，现在返回的对象在配置的外键上填充了带有JOIN语句的Country属性。传递给Include的字符串是外键属性的名称。从技术上讲，在查询语句中，您可以根据需要包含多个Include调用。但是，您拥有的内容越多，返回的对象图表以及随后的额外内存消耗也会增长。

#### 添加记录

添加新记录需要一些代码在内存中添加对象，然后将集合持久保存到磁盘。

```c#
public void Add(Customer customer)
{
    if (customer == null)
      return;
    using (var db = new YourDatabase())
    {
        db.Customers.Add(customer);
        try 
        {
           db.SaveChanges();
        } 
        catch(Exception exception) 
        {
           // Recover in some way or expand the way
           // it works for you, For example, only catching
           // some exceptions.
        }
    }
}
```

只要传递的对象已完全配置并填充在所有必填字段中，只需执行此操作即可。数据访问层的一个好方法是从应用程序层（在从控制器调用的服务类中）的业务角度验证对象，并假设存储库中的一切正常，或者如果出现任何问题则抛出异常。或者，在存储库方法中，您还可以重复一些检查以确保一切正常。

#### 更新记录

在EF Core更新中，记录是两步操作。首先，查询要更新的记录，然后在相同的DbContext上下文中，更新其在内存中的状态并保持更改。

```c#
public void Update(Customer updatedCustomer)
{
   using (var db = new YourDatabase())
   {
       // Retrieve the record to update
       var customer = (from c in db.Customers
                       where c.Id == updatedCustomer.Id
                       select c).FirstOrDefault();
       if (customer == null)
           return;
           
       // Make changes   
       customer.FirstName = updatedCustomer.FirstName;
       customer.LastName = updatedCustomer.LastName;
       customer.Modified = DateTime.UtcNow;
       ...

       // Persist
       try 
       {
           db.SaveChanges();
       } 
       catch(Exception exception) 
       {
           // Recover in some way or expand the way
           // it works for you, For example, only catching
           // some exceptions.
       }
    }
}
```

使用发布的记录更新获取的记录可能是无聊的代码。虽然没有什么比手动复制场到场更快，但像AutoMapper这样的反射或高级工具可以节省时间。此外，使用单行代码克隆对象也很有帮助。尽管如此，考虑到更新记录主要是业务操作而不是普通的数据库操作，这两件事只在简单的应用程序中重合。这里的要点是，根据业务条件，某些字段永远不应更新或应获得系统计算的值。更多，有时更新记录是不够的，其他操作应该在同一业务事务的上下文中执行。这就是说，使用单一的更新方法，您可以将属性从源对象盲目地复制到目标对象，这种情况比最初看起来要少得多。我马上回到这里谈论交易。

#### 删除记录

删除记录就像更新记录一样。此外，在这种情况下，您必须检索要删除的记录，将其从数据库的内存中集合中删除，然后更新物理表。

```c#
public void Delete(int id)
{
    using (var db = new YourDatabase())
    {
       // Retrieve the record to delete
       var customer = (from c in db.Customers
                       where c.Id == id
                       select c).FirstOrDefault();
       if (customer == null)
           return;
       db.Customers.Remove(customer);
       
       // Persist
       try 
       {
           db.SaveChanges();
       } 
       catch(Exception exception) 
       {
           // Recover in some way or expand the way
           // it works for you, For example, only catching
           // some exceptions.
       }
    }
}
```

关于删除有两个备注。首先，删除也是业务操作，很少，业务操作需要销毁数据。通常，删除记录是逻辑上删除记录的问题，这会将删除操作变为更新。在EF6和EF Core中执行删除操作可能看起来势不可挡，但它为应用任何所需逻辑留下了空间。

如果您确实需要从数据库中物理删除记录，无论是否在数据库级别配置了级联选项，您都可以使用纯SQL语句。

```c#
db.Database.ExecuteSqlCommand(sql);
```

一般情况下，我鼓励您（和您的客户）仔细考虑实际删除记录。发展的未来在事件采购中眨眼，事件采购的支柱之一是数据库是仅附加结构。

### 处理交易

在实际应用程序中，大多数数据库操作都是事务的一部分，有时，它们是分布式事务的一部分。默认情况下，如果基础数据库提供程序支持事务，则在事务中处理随单次调用SaveChanges所做的所有更改。这意味着如果任何更改失败，则回滚整个事务，以便所有尝试的更改都不会物理应用于数据库。换句话说，SaveChanges要么完成它所做的所有工作，要么做任何事情。

#### 交易的显式控制

如果您无法通过单次调用SaveChanges来驱动所有更改，则可以通过DbContext类上的ad hoc方法定义显式事务。

```c#
using (var db = new YourDatabase())
{
   using (var tx = db.Database.BeginTransaction())
   {
       try 
       {
            // All database calls including multiple SaveChanges calls
            ...
            
            // Commit
            tx.Commit();
       }
       catch(Exception exception)
       {
           // Recover in some way or expand the way
           // it works for you, For example, only catching
           // some exceptions.
       }
   }
}
```

同样，请注意并非所有数据库提供程序都支持事务。但是，对于SQL Server等流行数据库的提供商来说情况并非如此。当提供者不支持事务时会发生什么取决于提供者本身 - 它可以抛出异常或者什么也不做。

#### 共享连接和事务

在EF Core中，在创建DbContext对象的实例时，可以注入数据库连接和/或事务对象。两个对象的基类是DbConnection和DbTransaction。

如果将相同的连接和事务注入到两个不同的DbContext对象中，则效果是跨这些上下文的所有操作都将在同一事务中和同一数据库连接上发生。以下代码段显示了如何在DbContext中注入连接。

```c#
public class YourDatabase : DbContext
{
    private DbConnection _connection;
    public YourDatabase(DbConnection connection)
    {
       _connection = connection;
    }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connection);
    }
}
```

相反，要注入事务范围，请按以下步骤操作：

```c#
context.Database.UseTransaction(transaction);
```

要从正在运行的事务中获取事务对象，请使用GetDbTransaction方法。有关更多信息，请查看http://docs.microsoft.com/en-us/ef/core/saving/transactions。

注意事项某些对TransactionScope的支持已添加到.NET Core 2.0中，但我建议您仔细检查以确保它适用于您打算在开始认真开发之前使用它的方案。这个类就在那里，但是暂时，行为似乎与完全.NET Framework中的版本不一样，顺便说一句，允许你将关系事务和文件系统和/或Web服务操作。

### 关于异步数据处理的一个词

EF Core中触发数据库操作的整个方法集也具有异步版本：SaveChangesAsync，FirstOrDefaultAsync和ToListAsync，仅列出最常用的方法。你应该使用它们吗？他们真正提供了什么样的好处？那么ASP.NET Core应用程序中的异步处理有什么意义呢？

异步处理本身不比同步处理快。相反，异步调用具有比同步调用更复杂的执行流程。在Web应用程序的上下文中，异步处理主要是在等待同步调用返回而不是处理下一个请求时没有阻塞线程。因此，整个应用程序响应速度更快，因为它可以接受并提供更多请求。从这里可以看出速度提升，更重要的是提高了可扩展性。

C＃语言使用async / await关键字以非常简单的方式将任何明显的同步代码转换为异步代码。然而，强大的功能带来了巨大的责任：始终意识到产生额外线程以处理可能不需要此异步性的工作负载的成本。请记住，您不是在处理并行性，而是将工作负载转发到另一个线程，而当前线程返回池以处理更多传入请求。您可以获得更高的可扩展性，但可能会受到轻微的速度影响。

#### ASP.NET核心应用程序中的异步处理

假设您将控制器方法标记为异步。代码从网站下载内容并跟踪异步操作之前和之后的线程ID。

```c#
public async Task<IActionResult> Test()
{
    var t1 = Thread.CurrentThread.ManagedThreadId.ToString();
    var client = new HttpClient();
    await client.GetStringAsync("http://www.google.com");
    var t2 = Thread.CurrentThread.ManagedThreadId.ToString();
    return Content(string.Concat(t1, " / ", t2));
}
```

您获得的净效果可以总结如图9-5所示。

![9_10](assets/9_10.jpg)

如图所示，请求由异步断点之前和之后的不同线程提供。对特定页面的请求并没有真正受益于异步实现，但该站点的其余部分将享受它。原因是没有ASP.NET线程忙于等待I / O操作完成。在请求.NET线程池调用GetStringAsync异步操作之后，线程＃9立即返回到ASP.NET池以提供任何新的传入请求。完成此异步方法后，将拾取池中的第一个可用线程。它可能是第9号线，但不一定。在高度流量的站点中，在长时间操作完成所需的秒数内可以到达的请求数量可以保持较高或降低站点的响应水平。

#### 数据访问中的异步处理

要使线程返回到池并准备好提供另一个请求，线程必须等待异步操作。这种语法的措辞可能会令人困惑：当您看到等待MethodAsync出现时，这意味着当前线程将对MethodAsync的调用推送到.NET线程池并返回。 MethodAsync调用之后的代码将在方法返回后的任何可用线程上发生。如下面的代码片段中那样调用Web服务是可能的。另一种可能性是通过EF Core异步调用某个数据库。

让我们考虑一个常见的场景。假设您有一个由一些静态内容组成的Web应用程序，视图渲染速度相对较快，并且由于需要长时间的数据库操作，一些视图运行速度明显变慢。

想象一下，你得到了许多耗尽线程池的并发请求。许多请求需要访问数据库，随后，所有这些线程都用于处理请求但实际上是空闲的，等待数据库查询返回。您的系统无法提供更多请求，CPU几乎达到0％！将数据库访问转换为异步代码似乎可以解决问题。再次，这取决于。

首先，我们讨论的是重构大部分数据访问层。无论你怎么看，它都不是在公园散步。但是我们假设你这样做了。其次，你真正实现的是更多线程回到池中准备好接受其他传入请求。如果这些请求需要访问要处理的数据库怎么办？通过将您的数据访问代码转换为异步模式，您只能获得阻塞数据库的能力！您转向异步，因为您的数据库太慢而无法响应传入的请求，您所做的就是向数据库发送更多查询。这不是解决问题的方法。在Web服务器和数据库之间添加缓存将是一个更好的解决方案。再次，花时间在负载下测量分布式应用程序性能，并在需要时更新代码和体系结构。

另一方面，这不是唯一的情况。甚至可能是，通过转向异步可以为静态资源或快速页面提供的请求越多。在这种情况下，您的站点将为用户提供更具响应性的体验并提供更好的可伸缩性。

#### 你想减速哪个服务器？

在某些方面，在我看来，当站点由于长时间运行（而不是CPU绑定）操作而响应缓慢时，您应该决定哪个服务器可以放慢速度 - Web服务器或数据库服务器。

一般来说，ASP.NET线程池可以处理比数据库服务器更多的并发请求。性能计数器将告诉您问题是对于IIS配置来说实际的HTTP流量是否过高，或者Web服务器是否正常，但是数据库是很难的。 IIS / ASP.NET配置中的设置可以增加每个CPU的请求和线程数。如果数字显示队列中牺牲了快速请求，那么只需提高该数字就可以比将代码转换为异步更快。

如果数字告诉您瓶颈是数据库获取的查询请求太多而需要太长时间才能完成，那么您需要查看后端的整体架构，或者只是设法使用缓存或者只是尝试提高查询效率。

例如，后端的体系结构更改可能意味着将请求卸载到外部队列，并让队列在完成后回拨您。长时间运行的查询 - 对您的应用程序而言“长”意味着什么 - 更好地被视为即发即弃操作。我意识到这种方法可能需要一个完全不同的基于消息的架构。然而，这是扩大规模的真正关键。 Asynching all不是超级性能的保证，但它也不是性能杀手。不要欺骗自己认为它只是起作用并修复一切。



## 概要

ASP.NET Core应用程序有许多方法可以访问数据。 EF Core不是唯一的选择，但它是专为.NET Core平台设计的O / RM，可与ASP.NET Core配合使用。正如我们在本章中看到的，您可以使用ADO.NET以及微O / RM来创建数据访问层。我最好的建议是将数据访问层视为一个单独的层，而不是直接来自表示，而是来自您集中所有工作流的应用层。









