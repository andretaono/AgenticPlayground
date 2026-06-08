# TerrainMesh

Engine-agnostic heightmap generation and terrain mesh building. Independent of the World tile system.

## Responsibility

- Procedural heightmap generation from seed
- Height sampling (nearest and bilinear)
- Regular-grid mesh construction (vertices, indices, normals)

## Ports

| Port | Role |
|------|------|
| `IHeightmapGenerator` | `Generate(seed, width, height, config) → Heightmap` |
| `IHeightmapSampler` | `Sample`, `SampleBilinear` on a heightmap |
| `ITerrainMeshBuilder` | `Build(heightmap, config) → TerrainMeshData` |

## Invariants

- Same seed + config + dimensions → identical heightmap
- Samples stay within `[MinHeight, MaxHeight]`
- Mesh vertex count = `width × height` (one vertex per heightmap sample)
- Index count = `(width - 1) × (height - 1) × 6` for a gridded quad mesh
- No NaN or infinity in generated samples or mesh vertices
- No dependency on World or Integration layers

## Future (Integration)

World tile data will be composed with TerrainMesh output in Integration (`WorldTerrainMeshComposer`) — not in this system.
