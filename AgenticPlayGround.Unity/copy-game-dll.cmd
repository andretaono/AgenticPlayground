@echo off
setlocal

set "CONFIG=Debug"
if not "%~1"=="" set "CONFIG=%~1"

set "REPO_ROOT=%~dp0.."
set "GAME_PROJECT=%REPO_ROOT%\AgenticPlayGround.csproj"

echo Building and syncing Game.dll to Unity (%CONFIG%, netstandard2.1)...
dotnet build "%GAME_PROJECT%" -c %CONFIG% -f netstandard2.1
if errorlevel 1 (
    echo Game.dll sync failed.
    exit /b 1
)

endlocal
