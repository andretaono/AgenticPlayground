using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Integration.TerrainMesh;

public sealed record TileSurfaceMeshGroup(
	SurfaceMaterialId Material,
	int CaveRegionId,
	TerrainMeshData Mesh);

public sealed record TileSurfaceMeshResult(IReadOnlyList<TileSurfaceMeshGroup> Groups);

public sealed class VisibleSurfaceMeshBuilder
{
	private const float Epsilon = 1e-4f;

	public TileSurfaceMeshResult Build(IReadOnlyList<TileBlock> blocks)
	{
		if (blocks is null)
			throw new ArgumentNullException(nameof(blocks));

		var blocksByCell = IndexBlocksByCell(blocks);
		var accumulators = new Dictionary<(SurfaceMaterialId Material, int CaveRegionId), MeshAccumulator>();

		foreach (var block in blocks)
		{
			TryAddFace(block, FaceDirection.PosX, blocksByCell, accumulators);
			TryAddFace(block, FaceDirection.NegX, blocksByCell, accumulators);
			TryAddFace(block, FaceDirection.PosY, blocksByCell, accumulators);
			TryAddFace(block, FaceDirection.NegY, blocksByCell, accumulators);
			TryAddFace(block, FaceDirection.PosZ, blocksByCell, accumulators);
			TryAddFace(block, FaceDirection.NegZ, blocksByCell, accumulators);
		}

		var groups = new List<TileSurfaceMeshGroup>(accumulators.Count);
		foreach (var kvp in accumulators)
		{
			var (material, caveRegionId) = kvp.Key;
			var mesh = kvp.Value.ToMeshData();
			if (mesh.Indices.Count == 0)
				continue;

			groups.Add(new TileSurfaceMeshGroup(material, caveRegionId, mesh));
		}

		return new TileSurfaceMeshResult(groups);
	}

	private static void TryAddFace(
		TileBlock block,
		FaceDirection face,
		Dictionary<(int X, int Z), List<TileBlock>> blocksByCell,
		Dictionary<(SurfaceMaterialId Material, int CaveRegionId), MeshAccumulator> accumulators)
	{
		if (IsFaceOccluded(block, face, blocksByCell))
			return;

		var groupKey = (block.Material, GroupCaveRegionId(block));
		if (!accumulators.TryGetValue(groupKey, out var accumulator))
		{
			accumulator = new MeshAccumulator();
			accumulators[groupKey] = accumulator;
		}

		accumulator.AddQuad(block, face);
	}

	private static int GroupCaveRegionId(TileBlock block) =>
		block.Material == SurfaceMaterialId.CeilingCap ? block.CaveRegionId : -1;

	private static bool IsFaceOccluded(
		TileBlock block,
		FaceDirection face,
		Dictionary<(int X, int Z), List<TileBlock>> blocksByCell)
	{
		foreach (var neighbor in GetOcclusionCandidates(block, face, blocksByCell))
		{
			if (neighbor.Material != block.Material)
				continue;

			if (!RangesOverlapForFace(face, block, neighbor))
				continue;

			if (face switch
			    {
				    FaceDirection.PosX => ApproxEqual(neighbor.MinX, block.MaxX),
				    FaceDirection.NegX => ApproxEqual(neighbor.MaxX, block.MinX),
				    FaceDirection.PosY => ApproxEqual(neighbor.MinY, block.MaxY),
				    FaceDirection.NegY => ApproxEqual(neighbor.MaxY, block.MinY),
				    FaceDirection.PosZ => ApproxEqual(neighbor.MinZ, block.MaxZ),
				    FaceDirection.NegZ => ApproxEqual(neighbor.MaxZ, block.MinZ),
				    _ => false
			    })
			{
				return true;
			}
		}

		return false;
	}

	private static IEnumerable<TileBlock> GetOcclusionCandidates(
		TileBlock block,
		FaceDirection face,
		Dictionary<(int X, int Z), List<TileBlock>> blocksByCell)
	{
		return face switch
		{
			FaceDirection.PosX => GetCellBlocks(blocksByCell, block.CellX + 1, block.CellZ),
			FaceDirection.NegX => GetCellBlocks(blocksByCell, block.CellX - 1, block.CellZ),
			FaceDirection.PosZ => GetCellBlocks(blocksByCell, block.CellX, block.CellZ + 1),
			FaceDirection.NegZ => GetCellBlocks(blocksByCell, block.CellX, block.CellZ - 1),
			FaceDirection.PosY or FaceDirection.NegY => GetCellBlocks(blocksByCell, block.CellX, block.CellZ),
			_ => Array.Empty<TileBlock>()
		};
	}

	private static IEnumerable<TileBlock> GetCellBlocks(
		Dictionary<(int X, int Z), List<TileBlock>> blocksByCell,
		int cellX,
		int cellZ)
	{
		return blocksByCell.TryGetValue((cellX, cellZ), out var blocks)
			? blocks
			: Array.Empty<TileBlock>();
	}

	private static Dictionary<(int X, int Z), List<TileBlock>> IndexBlocksByCell(IReadOnlyList<TileBlock> blocks)
	{
		var index = new Dictionary<(int X, int Z), List<TileBlock>>();
		foreach (var block in blocks)
		{
			var key = (block.CellX, block.CellZ);
			if (!index.TryGetValue(key, out var list))
			{
				list = new List<TileBlock>();
				index[key] = list;
			}

			list.Add(block);
		}

		return index;
	}

	private static bool RangesOverlapForFace(FaceDirection face, TileBlock block, TileBlock neighbor) =>
		face switch
		{
			FaceDirection.PosX or FaceDirection.NegX =>
				RangesOverlap(block.MinY, block.MaxY, neighbor.MinY, neighbor.MaxY) &&
				RangesOverlap(block.MinZ, block.MaxZ, neighbor.MinZ, neighbor.MaxZ),
			FaceDirection.PosY or FaceDirection.NegY =>
				RangesOverlap(block.MinX, block.MaxX, neighbor.MinX, neighbor.MaxX) &&
				RangesOverlap(block.MinZ, block.MaxZ, neighbor.MinZ, neighbor.MaxZ),
			FaceDirection.PosZ or FaceDirection.NegZ =>
				RangesOverlap(block.MinX, block.MaxX, neighbor.MinX, neighbor.MaxX) &&
				RangesOverlap(block.MinY, block.MaxY, neighbor.MinY, neighbor.MaxY),
			_ => false
		};

	private static bool RangesOverlap(float minA, float maxA, float minB, float maxB) =>
		minA < maxB - Epsilon && minB < maxA - Epsilon;

	private static bool ApproxEqual(float a, float b) => MathF.Abs(a - b) <= Epsilon;

	private enum FaceDirection
	{
		PosX,
		NegX,
		PosY,
		NegY,
		PosZ,
		NegZ
	}

	private sealed class MeshAccumulator
	{
		private readonly List<Vector3> _vertices = new();
		private readonly List<int> _indices = new();
		private readonly List<Vector3> _normals = new();

		public void AddQuad(TileBlock block, FaceDirection face)
		{
			var start = _vertices.Count;
			var normal = FaceNormal(face);

			foreach (var corner in FaceCorners(block, face))
			{
				_vertices.Add(corner);
				_normals.Add(normal);
			}

			_indices.Add(start);
			_indices.Add(start + 1);
			_indices.Add(start + 2);
			_indices.Add(start);
			_indices.Add(start + 2);
			_indices.Add(start + 3);
		}

		public TerrainMeshData ToMeshData() =>
			TerrainMeshData.Create(_vertices, _indices, _normals);

		private static Vector3 FaceNormal(FaceDirection face) =>
			face switch
			{
				FaceDirection.PosX => new Vector3(1f, 0f, 0f),
				FaceDirection.NegX => new Vector3(-1f, 0f, 0f),
				FaceDirection.PosY => new Vector3(0f, 1f, 0f),
				FaceDirection.NegY => new Vector3(0f, -1f, 0f),
				FaceDirection.PosZ => new Vector3(0f, 0f, 1f),
				FaceDirection.NegZ => new Vector3(0f, 0f, -1f),
				_ => new Vector3(0f, 1f, 0f)
			};

		private static IEnumerable<Vector3> FaceCorners(TileBlock block, FaceDirection face)
		{
			// CCW when viewed from outside so triangle cross products match face normals.
			return face switch
			{
				FaceDirection.PosX =>
				[
					new Vector3(block.MaxX, block.MinY, block.MinZ),
					new Vector3(block.MaxX, block.MaxY, block.MinZ),
					new Vector3(block.MaxX, block.MaxY, block.MaxZ),
					new Vector3(block.MaxX, block.MinY, block.MaxZ)
				],
				FaceDirection.NegX =>
				[
					new Vector3(block.MinX, block.MinY, block.MaxZ),
					new Vector3(block.MinX, block.MaxY, block.MaxZ),
					new Vector3(block.MinX, block.MaxY, block.MinZ),
					new Vector3(block.MinX, block.MinY, block.MinZ)
				],
				FaceDirection.PosY =>
				[
					new Vector3(block.MinX, block.MaxY, block.MinZ),
					new Vector3(block.MinX, block.MaxY, block.MaxZ),
					new Vector3(block.MaxX, block.MaxY, block.MaxZ),
					new Vector3(block.MaxX, block.MaxY, block.MinZ)
				],
				FaceDirection.NegY =>
				[
					new Vector3(block.MinX, block.MinY, block.MinZ),
					new Vector3(block.MaxX, block.MinY, block.MinZ),
					new Vector3(block.MaxX, block.MinY, block.MaxZ),
					new Vector3(block.MinX, block.MinY, block.MaxZ)
				],
				FaceDirection.PosZ =>
				[
					new Vector3(block.MinX, block.MinY, block.MaxZ),
					new Vector3(block.MaxX, block.MinY, block.MaxZ),
					new Vector3(block.MaxX, block.MaxY, block.MaxZ),
					new Vector3(block.MinX, block.MaxY, block.MaxZ)
				],
				FaceDirection.NegZ =>
				[
					new Vector3(block.MaxX, block.MinY, block.MinZ),
					new Vector3(block.MinX, block.MinY, block.MinZ),
					new Vector3(block.MinX, block.MaxY, block.MinZ),
					new Vector3(block.MaxX, block.MaxY, block.MinZ)
				],
				_ => Array.Empty<Vector3>()
			};
		}
	}
}
