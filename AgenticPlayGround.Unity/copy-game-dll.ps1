param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$unityRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $unityRoot
$gameProject = Join-Path $repoRoot "AgenticPlayGround.csproj"
$dllSource = Join-Path $repoRoot "bin\$Configuration\netstandard2.1\Game.dll"
$dllTargetDir = Join-Path $unityRoot "Assets\Plugins\Game"
$dllTarget = Join-Path $dllTargetDir "Game.dll"

Write-Host "Building $gameProject ($Configuration)..."

dotnet build $gameProject -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "Game.dll build failed."
}
if (-not (Test-Path $dllSource)) {
    throw "Game.dll not found at $dllSource"
}

New-Item -ItemType Directory -Force -Path $dllTargetDir | Out-Null
Copy-Item $dllSource $dllTarget -Force
Write-Host "Copied Game.dll to $dllTarget"
