@setlocal enabledelayedexpansion
@set PREVPROMPT=%PROMPT%
@prompt $E[1A
@set MAKE=make.bat
@echo on

@if "%1" == "" (set TARGET=build
) else (set TARGET=%1 && shift)

@call :%TARGET% %1 %2 %3 %4 %5 %6 %7 %8 %9
@prompt %PREVPROMPT%
@exit /b %ERRORLEVEL%

:build
	dotnet build --nologo -v q --clp:NoSummary
	@exit /b %ERRORLEVEL%

:clean
	dotnet clean --nologo -v q
	@exit /b %ERRORLEVEL%

:distclean
	@call :clean
	rmdir /S /Q src\bin  2>nul
	rmdir /S /Q src\obj  2>nul
	rmdir /S /Q test\bin 2>nul
	rmdir /S /Q test\obj 2>nul
	@exit /b %ERRORLEVEL%

:release
	dotnet publish src --nologo -v q --clp:NoSummary -c Release -o .tmp
	git archive HEAD --output=Yanp-%DATE:/=%.zip
	powershell -NoProfile Compress-Archive -Force -Path .tmp\*, README.md, LICENSE -DestinationPath Yanp-bin-%DATE:/=%.zip
	rmdir /S /Q .tmp 2>nul
	@exit /b %ERRORLEVEL%

:test
	dotnet test --nologo -v q
	@exit /b %ERRORLEVEL%
