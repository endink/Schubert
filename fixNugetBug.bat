
set project1=%userprofile%\.nuget
set project2=%localappdata%\nuget
echo "清理本地 NUGET 缓存，以修复依赖解析失败错误："

:delete
rd /s/Q %project1%\packages
rd /s/Q %project2%\Cache
echo.

:exit
set /p input=清理完成，按任意键退出。