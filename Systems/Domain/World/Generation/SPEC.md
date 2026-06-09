# World Generation

Procedural cave-style world map generation. Produces tile grids consumed by `IWorldDataSource`.

## Responsibility

- Cellular-automata cave layout from seed
- Start and goal placement on ground cells
- Ground-only connectivity validation (BFS)
- Deterministic retry when a seed produces an unplayable map

## Ports

| Port | Role |
|------|------|
| `IWorldGenerator` | `Generate(config) → GeneratedWorldMap` |

## Algorithm

1. **Initialize** — each cell is `wall` with probability `FillProbability` (default 0.48).
2. **Border** — force outer ring to `wall`.
3. **Smooth** — repeat `CellularAutomataIterations` times: cell becomes `wall` if 8-neighbor wall count >= 5, else `ground`. Borders stay `wall`.
4. **Place spawns** — Start = ground cell with minimum `x + y`; Goal = ground cell with maximum `x + y`.
5. **Validate** — BFS from Start through 4-connected `ground` cells only; accept iff Goal is reached.
6. **Water** — carve small water pools without breaking the start→goal path.
7. **Cave carving** — find wall blobs (including border-touching blobs), collect interior components from each eligible blob, order components for **even geographic spread** (deterministic farthest-point sampling on chamber centroids — no start/goal bias), then carve up to `MaxCaveCount` (backfill on failure): for each component (up to `MaxCavesPerBlob` per blob) hollow a connected subset capped by `MaxCaveAreaSize` (default 49; seed cell chosen deterministically at random within the component), leave map border tiles as wall, bore entrance tunnels through blob walls (`MinEntranceDepth`–`MaxEntranceDepth`, default 8; `MinEntranceWidth`–`MaxEntranceWidth` for parallel mouth width) to start-reachable ground, mark carved floor in `CaveRegionIndex`; reject carves where any marked floor is not start-reachable.
8. **Ceiling** — place `ceiling-solid` on carved cave floor (`CaveRegionIndex >= 0`); add clustered extra wall stacks on wall runs.
9. **Retry** — on rejection, try `Seed + attempt` up to `MaxAttempts` (ground/path validation only).

## Tile representation

- Grid cells use `TileId` string identities (`ground`, `wall`, `water`).
- Movement and pathfinding use this layer via `ToDataSource()`.

## Invariants

- Same `WorldGenerationConfig` (including seed) → identical ground layer.
- Every accepted map has `Tiles[Start] == Ground` and `Tiles[Goal] == Ground`.
- Every accepted map has a verified ground-only path from Start to Goal.
- Border cells are always `wall`.
- Every carved cave floor cell is reachable from Start via ground.

## Integration

```
GeneratedWorldMap.ToDataSource() → InMemoryWorldDataSource → WorldSystem
GeneratedWorldMap → TerrainComposer → Heightmap (actor Y positioning)
GeneratedWorldMap → UnityTerrainPresenter → per-tile cubes (Unity only)
```
