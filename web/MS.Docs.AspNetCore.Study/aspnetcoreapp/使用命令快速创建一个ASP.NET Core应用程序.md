# 使用命令快速创建一个ASP.NET Core应用程序

进入到要创建项目文件的目录中，然后输入以下命令：

```shell
dotnet new webapp -o aspnetcoreapp
```

启用本地HTTPS，用于信任HTTPS开发证书：

```
dotnet dev-certs https --trust
```

执行命令后，将会弹出是否同意信任开发证书的对话框，点击“是”即可。

运行创建的应用程序：

```
cd aspnetcoreapp
dotnet run
```

根据提示的信息，浏览对应的url即可。