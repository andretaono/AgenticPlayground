@echo off
setlocal

set "CONFIG=Debug"
if not "%~1"=="" set "CONFIG=%~1"

echo Watching Game.dll changes — rebuilds and syncs to Unity (%CONFIG%, netstandard2.1)...
echo Press Ctrl+C to stop.
dotnet watch build "%~dp0AgenticPlayGround.csproj" -c %CONFIG% -f netstandard2.1

endlocal
