@echo off  
echo 工作目录   %~dp0

if {%1} neq {} echo 版本后缀： %~1

:clear
set WHAT_SHOULD_BE_DELETED=bin 
set IN_LOOP=no 

:del
for /r %~dp0 %%a in (!WHAT_SHOULD_BE_DELETED!) do (  
  if exist %%a (   
  rd /s /q "%%a"  
 )  
)
if %IN_LOOP%  == yes goto build

set WHAT_SHOULD_BE_DELETED=obj 
set IN_LOOP=yes 
goto del


:build

set sln=%~dp0Schubert.sln

echo 清理完成，开始包还原
@REM dotnet restore --no-cache
dotnet restore --no-cache "%sln%"

echo 包还原完成，开始编译

dotnet build --no-restore -c Release "%sln%"

:pack
echo 包还原完成，开始打包
if {%1} neq {} (
	  dotnet pack "%sln%" -c Release --no-restore --no-build --version-suffix %~1 --output ..\..\..\Output
	  ) else (
	  echo 打包 %sln%
	  dotnet pack "%sln%" -c Release --no-restore --no-build --output ..\..\..\Output
	  )

:quit 
