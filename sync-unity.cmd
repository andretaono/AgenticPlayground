@echo off
setlocal

set "CONFIG=Debug"
if not "%~1"=="" set "CONFIG=%~1"

echo Syncing Game.dll to Unity (%CONFIG%, netstandard2.1)...
dotnet build "%~dp0AgenticPlayGround.csproj" -c %CONFIG% -f netstandard2.1
if errorlevel 1 exit /b 1

endlocal
