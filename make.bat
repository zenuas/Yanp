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
	git archive HEAD --output=Yanp-%DATE:/=%.zip
	
	dotnet publish src --nologo -v q --clp:NoSummary -c Release -o .tmp
	mkdir .tmp\template.cs
	mkdir .tmp\template.debug
	copy template.cs\*.txt    .tmp\template.cs\    /Y >nul
	copy template.cs\*.cs     .tmp\template.cs\    /Y >nul
	copy template.debug\*.txt .tmp\template.debug\ /Y >nul
	powershell -NoProfile $ProgressPreference = 'SilentlyContinue' ; Compress-Archive -Force -Path .tmp\*, README.md, LICENSE -DestinationPath Yanp-bin-%DATE:/=%.zip
	rmdir /S /Q .tmp 2>nul
	
	@REM dotnet publish src --nologo -v q --clp:NoSummary -c Release -o .tmp -r win-x64 --sc false -p:PublishSingleFile=true
	@REM mkdir .tmp\template.cs
	@REM copy template.cs\*.txt .tmp\template.cs\ /Y >nul
	@REM copy template.cs\*.cs  .tmp\template.cs\ /Y >nul
	@REM powershell -NoProfile $ProgressPreference = 'SilentlyContinue' ; Compress-Archive -Force -Path .tmp\*, README.md, LICENSE -DestinationPath Yanp-win-x64-noruntime-%DATE:/=%.zip
	@REM rmdir /S /Q .tmp 2>nul
	
	@REM dotnet publish src --nologo -v q --clp:NoSummary -c Release -o .tmp -r win-x64 --sc true -p:PublishSingleFile=true
	@REM mkdir .tmp\template.cs
	@REM copy template.cs\*.txt .tmp\template.cs\ /Y >nul
	@REM copy template.cs\*.cs  .tmp\template.cs\ /Y >nul
	@REM powershell -NoProfile $ProgressPreference = 'SilentlyContinue' ; Compress-Archive -Force -Path .tmp\*, README.md, LICENSE -DestinationPath Yanp-win-x64-%DATE:/=%.zip
	@REM rmdir /S /Q .tmp 2>nul
	
	@exit /b %ERRORLEVEL%

:test
	dotnet test --nologo -v q
	@exit /b %ERRORLEVEL%
