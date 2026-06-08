# AgenticPlayGround.Unity

Unity 6 host for the engine-agnostic `Game.dll` library. Unity code lives only under `Assets/GameBridge/` and implements **host-side adapters** for Integration presentation ports defined in `Game.dll`.

See also: [`docs/ARCHITECTURE.md`](../docs/ARCHITECTURE.md) (portable layers) and [`docs/actors.md`](../docs/actors.md) (actor identity).

## How GameBridge relates to Integration

| Layer | Location | Role |
|-------|----------|------|
| Domain + portable Integration | `Game.dll` (`Systems/Domain`, `Systems/Integration`) | Simulation, game content, `GameRuntimeBuilder`, port **interfaces** |
| Unity host (GameBridge) | `Assets/GameBridge/` | Port **implementations**, scene wiring, camera/input UX |

`Game.dll` never references `UnityEngine`. Unity scripts call into `Game.dll`; they do not reimplement simulation logic.

## Demo pipeline

1. **World** — `WorldGenerationSystem` produces a tile grid (`GeneratedWorldMap`)
2. **Terrain** — `TerrainComposer` builds a per-tile heightmap for actor positioning
3. **Render** — `UnityTerrainPresenter` (`ITerrainPresenter`) spawns per-tile cubes: wall on tile, ground offset below, water skipped
4. **Runtime** — `TerrainDemoComposition` builds `GameRuntime` via `GameRuntimeBuilder` (same pipeline as `TestRunner` integration tests)
5. **Play** — `GameLoopHost` ticks simulation; `CameraFollowHost` syncs the over-shoulder camera

## Controls (TerrainDemo)

- **W / S** — move forward / backward along facing
- **A / D** — turn left / right
- Camera follows behind the player (over-shoulder)

## Debug controls (TerrainDemo play mode)

Layer debug is enabled by default (`TerrainDemoSettings.EnableLayerDebug`). A HUD in the top-left shows the tile under the player and ground tile type.

| Key | Action |
|-----|--------|
| **1** | Ground layer overlay (tan = ground, blue = water, gray = wall) |
| **0** | Turn overlay off |

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
3. Console logs seed, start, goal, and tile counts
4. Terrain renders as per-tile cubes: tan = ground (below surface), gray = wall, water tiles have no geometry

## GameBridge layout

| Path | Role |
|------|------|
| `Assets/Plugins/Game/Game.dll` | Prebuilt core library |
| `Bootstrap/` | Thin scene entry (`TerrainDemoBootstrap`), settings, composition factory, session context |
| `Runtime/` | Per-frame hosts (`GameLoopHost`, `CameraFollowHost`) added at compose time |
| `Input/` | `IInputSource` implementation + facing (tank controls) |
| `Presentation/` | `IWorldPresenter`, camera follow |
| `Terrain/` | `ITerrainPresenter`, per-tile cube visuals, material helper |
| `Debug/` | Ground tile debug overlay |
| `Shaders/` | `GameBridge/VertexColorUnlit` shader (legacy; cubes use Lit materials) |

### Organization conventions

**Bootstrap** only holds inspector config and calls `TerrainDemoComposition.Build()` (mirrors `GameRuntimeBuilder` on the portable side). Per-frame logic lives in `Runtime/` host components attached during composition.

| Concern | Belongs in GameBridge | Belongs in `Game.dll` |
|---------|----------------------|------------------------|
| Tile collision, movement speed | — | `AgentMovementPolicy`, `AgentMovementConfig` |
| WASD → facing + forward/back input | `PlayerFacingController`, `UnityInputSource` | `IInputSource`, `InputToCommandAdapter` |
| Capsule position on terrain | `UnityWorldPresenter` | `WorldPresentationAdapter` |
| Camera smoothing / over-shoulder offset | `OverShoulderCameraFollow` | — |
| Swimming state from tile type | — | `AgentMovementStateAdapter` |
| World generation, heightmap compose | Called from bootstrap/composition | `WorldGenerationSystem`, `TerrainComposer` |

When adding a feature, ask: *is this simulation or presentation?* Simulation stays in `Game.dll`; Unity only adapts ports and scene-specific UX.

### Port implementations

| Port (`Game.dll`) | GameBridge implementation |
|-------------------|---------------------------|
| `IInputSource` | `UnityInputSource` |
| `IWorldPresenter` | `UnityWorldPresenter` |
| `ITerrainPresenter` | `UnityTerrainPresenter` |

Headless counterparts (`ConsoleInputSource`, `NullWorldPresenter`) live inside `Game.dll` for tests and console demos.

### Coordinate mapping

Movement simulation uses 2D `(X, Y)`. Unity terrain uses `(X, Z)` horizontal with `Y` as height:

```
unityX = simX * worldUnitsPerTile
unityZ = simY * worldUnitsPerTile
unityY = heightmap sample at (unityX, unityZ)
```

## Architecture rules

- No `UnityEngine` references in `Game.dll`
- No simulation logic in Unity scripts — wiring and presentation only
- Headless tests remain in `TestRunner`; Unity is visual verification and host UX

### Naming / Unity API (GameBridge)

GameBridge scripts mix `Game.dll` types with Unity APIs. Avoid ambiguous references:

- **Always qualify** static Unity APIs: `UnityEngine.Input`, `UnityEngine.Debug`, `UnityEngine.Time`, `UnityEngine.Object`, `UnityEngine.Shader`, `UnityEngine.GUILayout`, `UnityEngine.GUI`, `UnityEngine.Mathf`, `UnityEngine.Camera.main`
- **Use type aliases** for game vs Unity math:
  - `GameVector2` / `GameVector3` — simulation coordinates from `Game.Systems.Foundation.GameMath.Core.Model`
  - `UnityVector3` / `UnityQuaternion` — scene geometry (`UnityEngine.Vector3`, etc.)
- **Use `UnityEngine.Color`** for mesh tinting and materials; never bare `Color` when game types are in scope
- Keep `using UnityEngine;` for inheritance and components (`MonoBehaviour`, `Transform`, `GameObject`, `MeshRenderer`, `Camera` parameters)

## Troubleshooting

**Unity cannot load Game.dll / TypeCache backing-field errors:** Run `sync-unity.cmd` from the repo root — it builds and copies the `netstandard2.1` `Game.dll` (Unity-compatible). `TestRunner` still uses the `net8.0` build. `Game.dll` excludes test code so the plugin does not reference `System.Security.Cryptography`.

**Out of memory on Play:** Close the Unity Profiler window when not actively profiling — it reserves memory even when not recording.

**File-scoped namespace / C# version errors:** GameBridge scripts use classic block namespaces and `Assets/csc.rsp` sets `-langversion:latest` for Unity's compiler.
