using Game.Systems.Domain.TerrainMesh.Model;

using Game.Systems.Domain.World.Model;

using Game.Systems.Foundation.GameMath.Core.Model;

using Game.Systems.Integration.Adapters;



namespace Game.Systems.Integration.TerrainMesh;



internal static class BeveledTileMeshBuilder

{

	public static BeveledTileMeshBuildResult Build(

		TileId[,] tiles,

		ITileRulesProvider rules,

		TileHeightModifierSettings settings,

		float cellSize,

		float heightScale)

	{

		if (tiles is null)

			throw new ArgumentNullException(nameof(tiles));

		if (rules is null)

			throw new ArgumentNullException(nameof(rules));

		if (settings is null)

			throw new ArgumentNullException(nameof(settings));

		if (cellSize <= 0f)

			throw new ArgumentOutOfRangeException(nameof(cellSize), "Cell size must be greater than zero.");



		var width = tiles.GetLength(0);

		var height = tiles.GetLength(1);

		var segments = Math.Max(1, settings.BevelSegments);

		var gridWidth = width * segments + 1;

		var gridHeight = height * segments + 1;

		var vertexCount = gridWidth * gridHeight;



		var tileHeights = BuildTileHeights(tiles, rules, settings);
		var cornerHeights = BuildCornerHeights(tileHeights, width, height);

		var heights = new float[gridWidth, gridHeight];

		for (var fz = 0; fz < gridHeight; fz++)
		{
			var gz = fz / (float)segments;

			for (var fx = 0; fx < gridWidth; fx++)
			{
				var gx = fx / (float)segments;

				heights[fx, fz] = SampleSymmetricHeightAtWorld(
					tileHeights,
					cornerHeights,
					width,
					height,
					gx,
					gz,
					settings.BevelInset);
			}
		}



		var vertices = new List<Vector3>(vertexCount);

		var normals = new List<Vector3>(vertexCount);

		var overlay = new List<TileId>(vertexCount);

		var indices = new List<int>((gridWidth - 1) * (gridHeight - 1) * 6);



		for (var fz = 0; fz < gridHeight; fz++)

		{

			var gz = fz / (float)segments;

			for (var fx = 0; fx < gridWidth; fx++)

			{

				var gx = fx / (float)segments;

				var worldX = gx * cellSize;

				var worldZ = gz * cellSize;

				var worldY = heights[fx, fz] * heightScale;



				vertices.Add(new Vector3(worldX, worldY, worldZ));

				overlay.Add(ResolveTileAtWorld(tiles, width, height, gx, gz));

				normals.Add(ComputeNormal(heights, fx, fz, gridWidth, gridHeight, cellSize, segments, heightScale));

			}

		}



		for (var fz = 0; fz < gridHeight - 1; fz++)

		{

			for (var fx = 0; fx < gridWidth - 1; fx++)

			{

				var topLeft = fz * gridWidth + fx;

				var topRight = topLeft + 1;

				var bottomLeft = topLeft + gridWidth;

				var bottomRight = bottomLeft + 1;



				indices.Add(topLeft);

				indices.Add(bottomLeft);

				indices.Add(topRight);



				indices.Add(topRight);

				indices.Add(bottomLeft);

				indices.Add(bottomRight);

			}

		}



		return new BeveledTileMeshBuildResult(

			TerrainMeshData.Create(vertices, indices, normals),

			overlay);

	}



	private static float[,] BuildTileHeights(

		TileId[,] tiles,

		ITileRulesProvider rules,

		TileHeightModifierSettings settings)

	{

		var width = tiles.GetLength(0);

		var height = tiles.GetLength(1);

		var result = new float[width, height];



		for (var z = 0; z < height; z++)

		for (var x = 0; x < width; x++)

			result[x, z] = TileHeightModifier.HeightForTile(rules.GetRules(tiles[x, z]), settings);



		return result;

	}



	private static float[,] BuildCornerHeights(float[,] tileHeights, int width, int height)
	{
		var corners = new float[width + 1, height + 1];

		for (var cz = 0; cz <= height; cz++)
		for (var cx = 0; cx <= width; cx++)
		{
			var sum = 0f;
			var count = 0;

			for (var dz = 0; dz <= 1; dz++)
			for (var dx = 0; dx <= 1; dx++)
			{
				var tileX = cx - dx;
				var tileZ = cz - dz;
				if (tileX < 0 || tileZ < 0 || tileX >= width || tileZ >= height)
					continue;

				sum += tileHeights[tileX, tileZ];
				count++;
			}

			corners[cx, cz] = count > 0 ? sum / count : 0f;
		}

		return corners;
	}

	private static float SampleSymmetricHeightAtWorld(
		float[,] tileHeights,
		float[,] cornerHeights,
		int width,
		int height,
		float gx,
		float gz,
		float bevelInset)
	{
		gx = Math.Clamp(gx, 0f, width);
		gz = Math.Clamp(gz, 0f, height);

		var bilinearHeight = SampleBilinearHeight(cornerHeights, width, height, gx, gz);
		if (bevelInset <= 0f)
			return bilinearHeight;

		var tileX = gx >= width ? width - 1 : Math.Min((int)MathF.Floor(gx), width - 1);
		var tileZ = gz >= height ? height - 1 : Math.Min((int)MathF.Floor(gz), height - 1);
		var localU = Math.Clamp(gx - tileX, 0f, 1f);
		var localV = Math.Clamp(gz - tileZ, 0f, 1f);

		var distToNearestEdge = MathF.Min(
			MathF.Min(localU, 1f - localU),
			MathF.Min(localV, 1f - localV));
		var flatWeight = SmoothStep(0f, bevelInset, distToNearestEdge);
		var flatHeight = tileHeights[tileX, tileZ];

		return flatHeight + (bilinearHeight - flatHeight) * (1f - flatWeight);
	}

	private static float SampleBilinearHeight(
		float[,] cornerHeights,
		int width,
		int height,
		float gx,
		float gz)
	{
		var cx0 = gx >= width ? width - 1 : Math.Min((int)MathF.Floor(gx), width - 1);
		var cz0 = gz >= height ? height - 1 : Math.Min((int)MathF.Floor(gz), height - 1);
		var tx = Math.Clamp(gx - cx0, 0f, 1f);
		var tz = Math.Clamp(gz - cz0, 0f, 1f);

		var h00 = cornerHeights[cx0, cz0];
		var h10 = cornerHeights[cx0 + 1, cz0];
		var h01 = cornerHeights[cx0, cz0 + 1];
		var h11 = cornerHeights[cx0 + 1, cz0 + 1];

		return (1f - tx) * (1f - tz) * h00
			+ tx * (1f - tz) * h10
			+ (1f - tx) * tz * h01
			+ tx * tz * h11;
	}



	private static TileId ResolveTileAtWorld(TileId[,] tiles, int width, int height, float gx, float gz)

	{

		var tileX = Math.Clamp((int)gx, 0, width - 1);

		var tileZ = Math.Clamp((int)gz, 0, height - 1);

		return tiles[tileX, tileZ];

	}



	private static float SmoothStep(float edge0, float edge1, float x)

	{

		var t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);

		return t * t * (3f - 2f * t);

	}



	private static Vector3 ComputeNormal(

		float[,] heights,

		int fx,

		int fz,

		int gridWidth,

		int gridHeight,

		float cellSize,

		int segments,

		float heightScale)

	{

		var step = cellSize / segments;

		var left = heights[Math.Max(fx - 1, 0), fz] * heightScale;

		var right = heights[Math.Min(fx + 1, gridWidth - 1), fz] * heightScale;

		var back = heights[fx, Math.Max(fz - 1, 0)] * heightScale;

		var forward = heights[fx, Math.Min(fz + 1, gridHeight - 1)] * heightScale;



		var tangentX = new Vector3(step * 2f, right - left, 0f);

		var tangentZ = new Vector3(0f, forward - back, step * 2f);



		return Normalize(Cross(tangentZ, tangentX));

	}



	private static Vector3 Cross(Vector3 a, Vector3 b) =>

		new(

			a.Y * b.Z - a.Z * b.Y,

			a.Z * b.X - a.X * b.Z,

			a.X * b.Y - a.Y * b.X);



	private static Vector3 Normalize(Vector3 vector)

	{

		var length = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);

		if (length <= 1e-6f)

			return new Vector3(0f, 1f, 0f);



		return new Vector3(vector.X / length, vector.Y / length, vector.Z / length);

	}

}



internal sealed class BeveledTileMeshBuildResult

{

	public BeveledTileMeshBuildResult(TerrainMeshData mesh, IReadOnlyList<TileId> vertexTileOverlay)

	{

		Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));

		VertexTileOverlay = vertexTileOverlay ?? throw new ArgumentNullException(nameof(vertexTileOverlay));

	}



	public TerrainMeshData Mesh { get; }

	public IReadOnlyList<TileId> VertexTileOverlay { get; }

}


