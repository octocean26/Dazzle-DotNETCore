using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Docs.Configuration
{
    public class Program
    {
        //键值对的快速赋值语法
        public static Dictionary<string, string> arrayDict = new Dictionary<string, string> {
            { "array:entries:0","value0"},
            { "array:entries:1","value1"},
            { "array:entries:2","value2"},
            { "array:entries:3","value3"},
            { "array:entries:4","value4"},
            { "array:entries:5","value5"}

        };


        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            //指定应用的配置提供程序
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                //内存配置提供程序
                config.AddInMemoryCollection(arrayDict);
                //JSON配置文件提供程序
                config.AddJsonFile("json_array.json", optional: false, reloadOnChange: false);
                config.AddJsonFile("startship.json", optional: false, reloadOnChange: false);
                //XML配置文件提供程序
                config.AddXmlFile("tvshow.xml", optional: false, reloadOnChange: false);
                config.AddEFConfiguration(options => options.UseInMemoryDatabase("InMemoryDb"));
                
                //使用 CreateDefaultBuilder 初始化新的 WebHostBuilder 时会自动调用 AddCommandLine
                //如果需要在其他配置之后，仍然能够使用命令行参数覆盖该配置，需要在最后调用AddCommandLine方法
                config.AddCommandLine(args);

                //设置其他前缀的环境变量的调用
                config.AddEnvironmentVariables(prefix: "My_");

            })
                .UseStartup<Startup>();
    }
}

