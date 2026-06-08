param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$gameProject = Join-Path $repoRoot "AgenticPlayGround.csproj"

Write-Host "Syncing Game.dll to Unity ($Configuration, netstandard2.1)..."
dotnet build $gameProject -c $Configuration -f netstandard2.1
if ($LASTEXITCODE -ne 0) {
    throw "Game.dll sync failed."
}
