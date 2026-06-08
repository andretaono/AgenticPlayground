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
6. **Retry** — on rejection, try `Seed + attempt` up to `MaxAttempts`.

## Tile representation

- Grid cells use `TileId` string identities (`ground`, `wall`, `water`).
- Well-known ids live in `TileIds`.
- Start and goal are `WorldPosition` metadata on `GeneratedWorldMap`, not special tile types.

## Invariants

- Same `WorldGenerationConfig` (including seed) → identical tiles, start, and goal.
- Every accepted map has `Tiles[Start] == Ground` and `Tiles[Goal] == Ground`.
- Every accepted map has a verified ground-only path from Start to Goal.
- Border cells are always `wall`.
- No dependency on Integration `TileRules` or TerrainMesh.

## Non-goals (v1)

- Water placement pass
- Room or maze dungeon layouts
- Integration with movement or terrain composition

## Integration

Generated maps feed existing World query flow:

```
GeneratedWorldMap.ToDataSource() → InMemoryWorldDataSource → WorldSystem
```
