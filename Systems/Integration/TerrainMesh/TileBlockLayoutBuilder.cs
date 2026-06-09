using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Adapters;

namespace Game.Systems.Integration.TerrainMesh;

public sealed class TileBlockLayoutBuilder
{
	public IReadOnlyList<TileBlock> Build(
		GeneratedWorldMap map,
		ITileRulesProvider tileRules,
		float cellSize,
		float heightScale,
		TileHeightModifierSettings settings)
	{
		if (map is null)
			throw new ArgumentNullException(nameof(map));
		if (tileRules is null)
			throw new ArgumentNullException(nameof(tileRules));
		if (settings is null)
			throw new ArgumentNullException(nameof(settings));
		if (cellSize <= 0f)
			throw new ArgumentOutOfRangeException(nameof(cellSize), "Cell size must be greater than zero.");
		if (heightScale <= 0f)
			throw new ArgumentOutOfRangeException(nameof(heightScale), "Height scale must be greater than zero.");

		var cubeHeight = cellSize * heightScale;
		var blocks = new List<TileBlock>(map.Width * map.Height * 2);
		var groundTopY = settings.GroundHeight * heightScale;
		var ceilingCenterY = groundTopY + cubeHeight * 1.5f;

		for (var z = 0; z < map.Height; z++)
		for (var x = 0; x < map.Width; x++)
		{
			blocks.Add(CreateBlock(
				x,
				z,
				cellSize,
				cubeHeight,
				settings.WaterHeight * heightScale - cubeHeight * 0.5f,
				SurfaceMaterialId.Water));

			var tile = map.GroundLayer[x, z];
			var rules = tileRules.GetRules(tile);
			if (rules.HasFlag(TileRules.Swimable))
				continue;

			var groundMaterial = map.CaveRegionIndex[x, z] >= 0
				? SurfaceMaterialId.CaveGround
				: SurfaceMaterialId.Ground;
			blocks.Add(CreateBlock(
				x,
				z,
				cellSize,
				cubeHeight,
				groundTopY - cubeHeight * 0.5f,
				groundMaterial));

			if (rules.HasFlag(TileRules.BlocksMovement))
			{
				blocks.Add(CreateBlock(
					x,
					z,
					cellSize,
					cubeHeight,
					settings.WallHeight * heightScale * 0.5f,
					SurfaceMaterialId.Wall));
			}
		}

		for (var z = 0; z < map.Height; z++)
		for (var x = 0; x < map.Width; x++)
		{
			if (map.CeilingLayer[x, z] != CeilingLayerTileIds.Solid)
				continue;

			if (map.GroundLayer[x, z] == TileIds.Wall)
			{
				blocks.Add(CreateBlock(
					x,
					z,
					cellSize,
					cubeHeight,
					ceilingCenterY,
					SurfaceMaterialId.CeilingStack));
				continue;
			}

			blocks.Add(CreateBlock(
				x,
				z,
				cellSize,
				cubeHeight,
				ceilingCenterY,
				SurfaceMaterialId.CeilingCap,
				map.CaveRegionIndex[x, z]));
		}

		return blocks;
	}

	private static TileBlock CreateBlock(
		int cellX,
		int cellZ,
		float cellSize,
		float cubeHeight,
		float centerY,
		SurfaceMaterialId material,
		int caveRegionId = -1)
	{
		var half = cellSize * 0.5f;
		var halfHeight = cubeHeight * 0.5f;
		var centerX = (cellX + 0.5f) * cellSize;
		var centerZ = (cellZ + 0.5f) * cellSize;

		return new TileBlock
		{
			CellX = cellX,
			CellZ = cellZ,
			MinX = centerX - half,
			MinY = centerY - halfHeight,
			MinZ = centerZ - half,
			MaxX = centerX + half,
			MaxY = centerY + halfHeight,
			MaxZ = centerZ + half,
			Material = material,
			CaveRegionId = caveRegionId
		};
	}
}
