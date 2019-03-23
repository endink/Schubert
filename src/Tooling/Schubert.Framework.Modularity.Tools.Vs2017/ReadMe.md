Schubert.Framework 模块化工具 For VS2017
================
<h2>概述</h2>
在VS2017升级后，.NET Core项目组织方式发生了极大变化：过去通过该`project.json`组织项目的方式变更为`.csproj`项目文件组织方式。为了适应这一变更，在VS2017下需要使用针对VS2017的模块化工具进行项目发布。

<h2>使用方式：</h2>

打开需要发布的Web项目文件`*.csproj`，添加以配置下节：
>因为VS2017.1版本下的Nuget无法正常引用CliTool

>详见：

>https://github.com/maartenba/dotnetcli-init/issues/1

>https://github.com/NuGet/Home/issues/4190 
>
>所以需要在在在`<Project>`节下手工加入该项目

```xml
<project Sdk="Microsoft.NET.Sdk">
    ...
  <ItemGroup>
    <DotNetCliToolReference Include="Schubert.Framework.Modularity.Tools.Vs2017" Version="*" />
  </ItemGroup>

  <Target Name="Modularity" AfterTargets="AfterPublish">
    <Message Text="publishUrl=$(publishUrl)"></Message>
    <Exec Command="dotnet modularity --dest $(publishUrl)"></Exec>
  </Target>
</project>
```
	
		  


引入完成后，Nuget将自动进行包还原。
之后打包将自动进行模块化处理