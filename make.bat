@echo off

if "%1" == "" (set TARGET=build
) else (set TARGET=%1 && shift)

call :%TARGET% %*
exit /b %ERRORLEVEL%

:build
	dotnet build --nologo --noconlog
	exit /b %ERRORLEVEL%

:test
	dotnet test --noconlog
	exit /b %ERRORLEVEL%
