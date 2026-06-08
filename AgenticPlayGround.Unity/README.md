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

From the **repo root** (recommended):

```cmd
sync-unity.cmd
```

Then open the project in Unity Hub (`AgenticPlayGround.Unity`).

## Syncing after core library changes

| When | Command |
|------|---------|
| One-shot sync | `sync-unity.cmd` (repo root) |
| Hands-free while coding | `watch-unity.cmd` (repo root; leave running in a terminal) |
| From Cursor / VS Code | **Terminal → Run Build Task** (Ctrl+Shift+B) — runs "Sync Unity DLL" |
| From Unity folder | `copy-game-dll.cmd` (alias for the same build) |

`dotnet build -f netstandard2.1` also auto-copies `Game.dll` into `Assets/Plugins/Game/` via an MSBuild target in `AgenticPlayGround.csproj`.

Unity refreshes the plugin automatically when the DLL changes.

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

**Unity cannot load Game.dll / TypeCache backing-field errors:** Run `sync-unity.cmd` from the repo root — it builds and copies the `netstandard2.1` `Game.dll` (Unity-compatible). `TestRunner` still uses the `net8.0` build. `Game.dll` excludes test code so the plugin does not reference `System.Security.Cryptography`.

**Out of memory on Play:** Close the Unity Profiler window when not actively profiling — it reserves memory even when not recording.

**File-scoped namespace / C# version errors:** GameBridge scripts use classic block namespaces and `Assets/csc.rsp` sets `-langversion:latest` for Unity's compiler.

**Pink terrain material:** Assign a material using the `GameBridge/VertexColorUnlit` shader, or let `TerrainDemoBootstrap` create one at runtime.
