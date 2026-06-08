@echo off
setlocal

set "CONFIG=Debug"
if not "%~1"=="" set "CONFIG=%~1"

set "UNITY_ROOT=%~dp0"
set "REPO_ROOT=%UNITY_ROOT%.."
set "GAME_PROJECT=%REPO_ROOT%\AgenticPlayGround.csproj"
set "DLL_SOURCE=%REPO_ROOT%\bin\%CONFIG%\netstandard2.1\Game.dll"
set "DLL_TARGET_DIR=%UNITY_ROOT%Assets\Plugins\Game"
set "DLL_TARGET=%DLL_TARGET_DIR%\Game.dll"

echo Building %GAME_PROJECT% (%CONFIG%)...
dotnet build "%GAME_PROJECT%" -c %CONFIG%
if errorlevel 1 (
    echo Game.dll build failed.
    exit /b 1
)

if not exist "%DLL_SOURCE%" (
    echo Game.dll not found at %DLL_SOURCE%
    exit /b 1
)

if not exist "%DLL_TARGET_DIR%" mkdir "%DLL_TARGET_DIR%"
copy /Y "%DLL_SOURCE%" "%DLL_TARGET%" >nul
echo Copied Game.dll to %DLL_TARGET%

endlocal
