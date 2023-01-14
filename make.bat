@echo off

if "%1" == "" (set TARGET=build
) else (set TARGET=%1 && shift)

call :%TARGET% %*
exit /b %ERRORLEVEL%

:build
	dotnet build --nologo -v q --clp:NoSummary
	exit /b %ERRORLEVEL%

:clean
	dotnet clean --nologo -v q
	exit /b %ERRORLEVEL%

:test
	dotnet test --nologo -v q
	exit /b %ERRORLEVEL%
