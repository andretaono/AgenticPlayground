# AgenticPlayGround.Unity

Unity 6 host for the engine-agnostic `Game.dll` library. Unity code lives only under `Assets/GameBridge/` and implements Integration presentation ports.

## Pipeline

1. `WorldGenerationSystem` — procedural cave tile grid
2. `WorldTerrainMeshComposer` — heightmap + mesh from tiles
3. `UnityTerrainPresenter` — `TerrainMeshData` → `UnityEngine.Mesh`

## Prerequisites

- Unity 6 (6000.x)
- .NET SDK 8 (to build `Game.dll`)

## Setup

From the repo root (or `AgenticPlayGround.Unity`):

```cmd
AgenticPlayGround.Unity\copy-game-dll.cmd
```

Or from `AgenticPlayGround.Unity`:

```cmd
copy-game-dll.cmd
```

PowerShell alternative (if script execution is allowed):

```powershell
.\copy-game-dll.ps1
```

If PowerShell blocks scripts (`PSSecurityException`), use the `.cmd` file above or run once:

```powershell
powershell -ExecutionPolicy Bypass -File .\copy-game-dll.ps1
```

Then open the project in Unity Hub (`AgenticPlayGround.Unity`).

After changing core library code, rebuild and recopy:

```cmd
copy-game-dll.cmd Debug
```

Unity will refresh the plugin automatically.

## Running the demo

1. Open `Assets/Scenes/TerrainDemo.unity`
2. Press Play
3. Console logs seed, start, and goal positions
4. Terrain renders with vertex colors: tan = ground, blue = water, gray = wall cliffs

## Project layout

| Path | Role |
|------|------|
| `Assets/Plugins/Game/Game.dll` | Prebuilt core library (no Unity references) |
| `Assets/GameBridge/Bootstrap/` | Composition root + `ITerrainPresenter` + mesh conversion |
| `Assets/GameBridge/Shaders/` | Vertex-color unlit shader |

## Architecture rules

- No `UnityEngine` references in `Game.dll`
- No simulation logic in Unity scripts — wiring and presentation only
- Headless tests remain in `TestRunner`; Unity is visual verification

## Troubleshooting

**Unity cannot load Game.dll / TypeCache backing-field errors:** Run `copy-game-dll.cmd` — it copies the `netstandard2.1` build of `Game.dll` (Unity-compatible). `TestRunner` still uses the `net8.0` build. `Game.dll` excludes test code so the plugin does not reference `System.Security.Cryptography`.

**File-scoped namespace / C# version errors:** GameBridge scripts use classic block namespaces and `Assets/csc.rsp` sets `-langversion:latest` for Unity's compiler.

**Pink terrain material:** Assign a material using the `GameBridge/VertexColorUnlit` shader, or let `TerrainDemoBootstrap` create one at runtime.
