@REM  Name: 递归删除指定的目录，请把此文件放在你希望执行的那个目录   
@echo off  
setlocal enabledelayedexpansion  
  

set dir=%~dp0Output

echo 要发布的包：
for /R %dir% %%f in (*.nupkg) do ( 
echo publish %%f
)


set /p input=确认按 y， 取消按任意键。

if /i not "%input%"=="y" goto exit

for /R %dir% %%f in (*.nupkg) do ( 
echo 开始上传 %%f
dotnet nuget push %%f -k oy2d3dtxn5whgyvozevajv2ewfchp57exab2wziy3bd7ji -s https://api.nuget.org/v3/index.json
)
set /p input=完成，按任意键退出。


:exit