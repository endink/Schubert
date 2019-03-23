@echo off  

echo 开始清理nuget 

for /d %%a in (C:\Users\%USERNAME%\.nuget\packages\schubert.*) do (  
  @echo "%%a" 
  rd /s /q "%%a"  
)

echo 清理完成

echo 开始还原
dotnet restore --no-cache
echo 还原成功
 