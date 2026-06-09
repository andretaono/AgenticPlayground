using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Generation.Controller;

internal static class WallBlobCaveCarver
{
	private const int SpreadFirstPickSalt = 0xCAF2;
	private const int SpreadTieSalt = 0xCAF3;
	private const int InteriorSeedSalt = 0xCAF4;
	private const int EntranceCountSalt = 0xE001;
	private const int EntranceWidthSalt = 0xE002;
	private const int EntrancePickSalt = 0xE003;

	private static readonly (int Dx, int Dy)[] Directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };

	public static CaveCarveDiagnostic Apply(
		TileId[,] groundLayer,
		WorldPosition start,
		WorldPosition goal,
		int seed,
		WorldGenerationConfig config,
		int[,] caveRegionIndex)
	{
		if (groundLayer is null)
			throw new ArgumentNullException(nameof(groundLayer));
		if (config is null)
			throw new ArgumentNullException(nameof(config));
		if (caveRegionIndex is null)
			throw new ArgumentNullException(nameof(caveRegionIndex));

		var width = groundLayer.GetLength(0);
		var height = groundLayer.GetLength(1);

		var eligible = FindEligibleBlobs(groundLayer, config, width, height);
		var chamberCandidates = CollectChamberCandidates(eligible, groundLayer, config, width, height);
		var orderedCandidates = OrderCandidatesForEvenSpread(chamberCandidates, seed);

		var nextRegionId = 0;
		var successfulCarves = 0;
		var attemptedCarves = 0;
		var carvedCaves = new List<CarvedCaveInfo>();
		var chambersCarvedFromBlob = new Dictionary<WallBlob, int>();

		foreach (var candidate in orderedCandidates)
		{
			if (successfulCarves >= config.MaxCaveCount)
				break;

			chambersCarvedFromBlob.TryGetValue(candidate.Blob, out var chambersInBlob);
			if (chambersInBlob >= config.MaxCavesPerBlob)
				continue;

			if (!candidate.InteriorComponent.Any(cell => groundLayer[cell.X, cell.Y] == TileIds.Wall))
				continue;

			attemptedCarves++;

			if (TryCarveChamber(
				    groundLayer,
				    caveRegionIndex,
				    candidate.Blob,
				    candidate.InteriorComponent,
				    start,
				    goal,
				    seed,
				    config,
				    nextRegionId,
				    chambersInBlob,
				    width,
				    height,
				    out var carvedCave))
			{
				carvedCaves.Add(carvedCave);
				nextRegionId++;
				successfulCarves++;
				chambersCarvedFromBlob[candidate.Blob] = chambersInBlob + 1;
			}
		}

		return new CaveCarveDiagnostic(attemptedCarves, successfulCarves, carvedCaves);
	}

	private sealed class ChamberCandidate
	{
		public ChamberCandidate(WallBlob blob, HashSet<WorldPosition> interiorComponent, WorldPosition centroid)
		{
			Blob = blob;
			InteriorComponent = interiorComponent;
			Centroid = centroid;
		}

		public WallBlob Blob { get; }
		public HashSet<WorldPosition> InteriorComponent { get; }
		public WorldPosition Centroid { get; }
	}

	private static List<ChamberCandidate> CollectChamberCandidates(
		IReadOnlyList<WallBlob> eligible,
		TileId[,] groundLayer,
		WorldGenerationConfig config,
		int width,
		int height)
	{
		var candidates = new List<ChamberCandidate>();

		foreach (var blob in eligible)
		{
			var fullInterior = ComputeInterior(blob.Cells, groundLayer, width, height);
			var interiorComponents = PartitionInteriorComponents(fullInterior)
				.Where(component => component.Count >= config.MinCaveAreaSize);

			foreach (var component in interiorComponents)
			{
				candidates.Add(new ChamberCandidate(blob, component, ComputeCentroid(component)));
			}
		}

		return candidates;
	}

	private static WorldPosition ComputeCentroid(HashSet<WorldPosition> cells)
	{
		var sumX = 0;
		var sumY = 0;

		foreach (var cell in cells)
		{
			sumX += cell.X;
			sumY += cell.Y;
		}

		return new WorldPosition(sumX / cells.Count, sumY / cells.Count);
	}

	private static List<ChamberCandidate> OrderCandidatesForEvenSpread(
		IReadOnlyList<ChamberCandidate> candidates,
		int seed)
	{
		if (candidates.Count <= 1)
			return candidates.ToList();

		var ordered = new List<ChamberCandidate>(candidates.Count);
		var remaining = candidates.ToList();
		var selectedCentroids = new List<WorldPosition>();

		for (var pickIndex = 0; remaining.Count > 0; pickIndex++)
		{
			ChamberCandidate pick;

			if (selectedCentroids.Count == 0)
			{
				var index = RandomIntInclusive(seed, pickIndex, 0, SpreadFirstPickSalt, 0, remaining.Count - 1);
				pick = remaining[index];
			}
			else
			{
				pick = remaining
					.OrderByDescending(candidate => MinDistanceToAny(candidate.Centroid, selectedCentroids))
					.ThenByDescending(candidate =>
						DeterministicCellRandom.Roll(seed, candidate.Centroid.X, candidate.Centroid.Y, SpreadTieSalt))
					.First();
			}

			ordered.Add(pick);
			remaining.Remove(pick);
			selectedCentroids.Add(pick.Centroid);
		}

		return ordered;
	}

	private static int MinDistanceToAny(WorldPosition position, IReadOnlyList<WorldPosition> others)
	{
		var nearest = int.MaxValue;

		foreach (var other in others)
		{
			var distance = Math.Abs(position.X - other.X) + Math.Abs(position.Y - other.Y);
			if (distance < nearest)
				nearest = distance;
		}

		return nearest;
	}

	private sealed class WallBlob
	{
		public WallBlob(HashSet<WorldPosition> cells, WorldPosition anchor)
		{
			Cells = cells;
			Anchor = anchor;
		}

		public HashSet<WorldPosition> Cells { get; }
		public WorldPosition Anchor { get; }
	}

	private sealed class TunnelCandidate
	{
		public TunnelCandidate(IReadOnlyList<IReadOnlyList<WorldPosition>> rays)
		{
			Rays = rays;
		}

		public IReadOnlyList<IReadOnlyList<WorldPosition>> Rays { get; }

		public int Width => Rays.Count;

		public IEnumerable<WorldPosition> AllCells
		{
			get
			{
				foreach (var ray in Rays)
				foreach (var cell in ray)
					yield return cell;
			}
		}
	}

	private static List<WallBlob> FindEligibleBlobs(
		TileId[,] groundLayer,
		WorldGenerationConfig config,
		int width,
		int height)
	{
		var visited = new bool[width, height];
		var eligible = new List<WallBlob>();

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				if (visited[x, y] || groundLayer[x, y] != TileIds.Wall)
					continue;

				var cells = CollectWallBlob(groundLayer, visited, x, y, width, height);

				if (cells.Count <= config.MinWallBlobSize)
					continue;

				var interior = ComputeInterior(cells, groundLayer, width, height);
				if (interior.Count < config.MinCaveAreaSize)
					continue;

				eligible.Add(new WallBlob(cells, FindAnchor(cells)));
			}
		}

		return eligible;
	}

	private static HashSet<WorldPosition> CollectWallBlob(
		TileId[,] groundLayer,
		bool[,] visited,
		int startX,
		int startY,
		int width,
		int height)
	{
		var cells = new HashSet<WorldPosition>();
		var queue = new Queue<WorldPosition>();
		queue.Enqueue(new WorldPosition(startX, startY));
		visited[startX, startY] = true;

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			cells.Add(current);

			foreach (var neighbor in FloorTraversal.GetNeighbors(current))
			{
				if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
					continue;

				if (visited[neighbor.X, neighbor.Y])
					continue;

				if (groundLayer[neighbor.X, neighbor.Y] != TileIds.Wall)
					continue;

				visited[neighbor.X, neighbor.Y] = true;
				queue.Enqueue(neighbor);
			}
		}

		return cells;
	}

	private static bool IsMapBorderCell(int x, int y, int width, int height) =>
		x == 0 || y == 0 || x == width - 1 || y == height - 1;

	private static WorldPosition FindAnchor(HashSet<WorldPosition> cells)
	{
		var anchor = new WorldPosition(int.MaxValue, int.MaxValue);

		foreach (var cell in cells)
		{
			if (cell.X < anchor.X || (cell.X == anchor.X && cell.Y < anchor.Y))
				anchor = cell;
		}

		return anchor;
	}

	private static WorldPosition FindAnchor(HashSet<WorldPosition> cells, WorldPosition fallback)
	{
		if (cells.Count == 0)
			return fallback;

		return FindAnchor(cells);
	}

	private static HashSet<WorldPosition> ComputeInterior(
		HashSet<WorldPosition> blob,
		TileId[,] groundLayer,
		int width,
		int height)
	{
		var interior = new HashSet<WorldPosition>();

		foreach (var cell in blob)
		{
			if (IsMapBorderCell(cell.X, cell.Y, width, height))
				continue;

			if (IsInteriorCell(cell, groundLayer, width, height))
				interior.Add(cell);
		}

		return interior;
	}

	private static bool IsInteriorCell(WorldPosition cell, TileId[,] groundLayer, int width, int height)
	{
		foreach (var neighbor in FloorTraversal.GetNeighbors(cell))
		{
			if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
				return false;

			if (groundLayer[neighbor.X, neighbor.Y] != TileIds.Wall)
				return false;
		}

		return true;
	}

	private static List<HashSet<WorldPosition>> PartitionInteriorComponents(HashSet<WorldPosition> interior)
	{
		var remaining = new HashSet<WorldPosition>(interior);
		var components = new List<HashSet<WorldPosition>>();

		while (remaining.Count > 0)
		{
			var seedCell = remaining
				.OrderBy(cell => cell.X + cell.Y)
				.ThenBy(cell => cell.X)
				.ThenBy(cell => cell.Y)
				.First();

			var component = new HashSet<WorldPosition>();
			var queue = new Queue<WorldPosition>();
			queue.Enqueue(seedCell);
			remaining.Remove(seedCell);
			component.Add(seedCell);

			while (queue.Count > 0)
			{
				var current = queue.Dequeue();

				foreach (var neighbor in FloorTraversal.GetNeighbors(current))
				{
					if (!remaining.Remove(neighbor))
						continue;

					component.Add(neighbor);
					queue.Enqueue(neighbor);
				}
			}

			components.Add(component);
		}

		return components;
	}

	private static HashSet<WorldPosition> SelectCarvableInterior(
		HashSet<WorldPosition> interior,
		int maxCaveAreaSize,
		int seed,
		WorldPosition anchor)
	{
		if (interior.Count <= maxCaveAreaSize)
			return interior;

		var orderedCells = interior
			.OrderBy(cell => cell.X + cell.Y)
			.ThenBy(cell => cell.X)
			.ThenBy(cell => cell.Y)
			.ToList();
		var seedIndex = RandomIntInclusive(seed, anchor.X, anchor.Y, InteriorSeedSalt, 0, orderedCells.Count - 1);
		var seedCell = orderedCells[seedIndex];

		var selected = new HashSet<WorldPosition>();
		var queue = new Queue<WorldPosition>();
		queue.Enqueue(seedCell);
		selected.Add(seedCell);

		while (queue.Count > 0 && selected.Count < maxCaveAreaSize)
		{
			var current = queue.Dequeue();

			foreach (var neighbor in FloorTraversal.GetNeighbors(current))
			{
				if (!interior.Contains(neighbor) || selected.Contains(neighbor))
					continue;

				selected.Add(neighbor);
				queue.Enqueue(neighbor);

				if (selected.Count >= maxCaveAreaSize)
					break;
			}
		}

		return selected;
	}

	private static bool TryCarveChamber(
		TileId[,] groundLayer,
		int[,] caveRegionIndex,
		WallBlob blob,
		HashSet<WorldPosition> interiorComponent,
		WorldPosition start,
		WorldPosition goal,
		int seed,
		WorldGenerationConfig config,
		int regionId,
		int chamberIndex,
		int width,
		int height,
		out CarvedCaveInfo carvedCave)
	{
		carvedCave = null!;
		var chamberAnchor = FindAnchor(interiorComponent, blob.Anchor);
		var interior = SelectCarvableInterior(
			interiorComponent,
			config.MaxCaveAreaSize,
			seed,
			chamberAnchor);
		var snapshot = SnapshotCells(groundLayer, caveRegionIndex, blob.Cells);

		foreach (var cell in interior)
		{
			if (IsMapBorderCell(cell.X, cell.Y, width, height))
				continue;

			groundLayer[cell.X, cell.Y] = TileIds.Ground;
			caveRegionIndex[cell.X, cell.Y] = regionId;
		}

		var accessibleExterior = MarkAccessibleGround(groundLayer, start, width, height);
		var entranceCount = RandomIntInclusive(
			seed,
			chamberAnchor.X,
			chamberAnchor.Y,
			EntranceCountSalt + chamberIndex,
			config.MinCaveEntrances,
			config.MaxCaveEntrances);

		var usedEntrances = new HashSet<WorldPosition>();
		var outerEntranceCells = new List<WorldPosition>();

		for (var entranceIndex = 0; entranceIndex < entranceCount; entranceIndex++)
		{
			var tunnelCandidates = FindTunnelCandidates(
				blob.Cells,
				interior,
				groundLayer,
				accessibleExterior,
				config,
				width,
				height);

			if (tunnelCandidates.Count == 0)
			{
				RestoreSnapshot(groundLayer, caveRegionIndex, snapshot);
				return false;
			}

			var widthRoll = RandomIntInclusive(
				seed,
				chamberAnchor.X,
				chamberAnchor.Y,
				EntranceWidthSalt + (chamberIndex * 10) + entranceIndex,
				config.MinEntranceWidth,
				config.MaxEntranceWidth);

			if (!TryPlaceTunnelEntrance(
				    tunnelCandidates,
				    usedEntrances,
				    widthRoll,
				    seed,
				    chamberAnchor,
				    (chamberIndex * 10) + entranceIndex,
				    groundLayer,
				    caveRegionIndex,
				    regionId,
				    width,
				    height,
				    out var placedOuterCells))
			{
				RestoreSnapshot(groundLayer, caveRegionIndex, snapshot);
				return false;
			}

			outerEntranceCells.AddRange(placedOuterCells);
			accessibleExterior = MarkAccessibleGround(groundLayer, start, width, height);
		}

		if (!GroundConnectivity.HasGroundPath(groundLayer, start, goal))
		{
			RestoreSnapshot(groundLayer, caveRegionIndex, snapshot);
			return false;
		}

		if (!IsAllCaveFloorReachable(groundLayer, caveRegionIndex, start, regionId, width, height))
		{
			RestoreSnapshot(groundLayer, caveRegionIndex, snapshot);
			return false;
		}

		carvedCave = new CarvedCaveInfo(
			regionId,
			interior.Count,
			SelectOutermostEntrance(outerEntranceCells, interior));

		return true;
	}

	private static WorldPosition SelectOutermostEntrance(
		IReadOnlyList<WorldPosition> outerEntranceCells,
		HashSet<WorldPosition> interior)
	{
		var best = outerEntranceCells[0];
		var bestDistance = DistanceToNearestInteriorCell(best, interior);

		for (var i = 1; i < outerEntranceCells.Count; i++)
		{
			var candidate = outerEntranceCells[i];
			var distance = DistanceToNearestInteriorCell(candidate, interior);
			if (distance > bestDistance)
			{
				best = candidate;
				bestDistance = distance;
			}
		}

		return best;
	}

	private static int DistanceToNearestInteriorCell(
		WorldPosition entrance,
		HashSet<WorldPosition> interior)
	{
		var nearest = int.MaxValue;

		foreach (var cell in interior)
		{
			var distance = Math.Abs(entrance.X - cell.X) + Math.Abs(entrance.Y - cell.Y);
			if (distance < nearest)
				nearest = distance;
		}

		return nearest;
	}

	private static bool IsAllCaveFloorReachable(
		TileId[,] groundLayer,
		int[,] caveRegionIndex,
		WorldPosition start,
		int regionId,
		int width,
		int height)
	{
		var accessible = MarkAccessibleGround(groundLayer, start, width, height);

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				if (caveRegionIndex[x, y] != regionId)
					continue;

				if (!accessible[x, y])
					return false;
			}
		}

		return true;
	}

	private sealed class CellSnapshot
	{
		public CellSnapshot(TileId tile, int regionIndex)
		{
			Tile = tile;
			RegionIndex = regionIndex;
		}

		public TileId Tile { get; }
		public int RegionIndex { get; }
	}

	private static Dictionary<WorldPosition, CellSnapshot> SnapshotCells(
		TileId[,] groundLayer,
		int[,] caveRegionIndex,
		HashSet<WorldPosition> cells)
	{
		var snapshot = new Dictionary<WorldPosition, CellSnapshot>();

		foreach (var cell in cells)
		{
			snapshot[cell] = new CellSnapshot(
				groundLayer[cell.X, cell.Y],
				caveRegionIndex[cell.X, cell.Y]);
		}

		return snapshot;
	}

	private static void RestoreSnapshot(
		TileId[,] groundLayer,
		int[,] caveRegionIndex,
		Dictionary<WorldPosition, CellSnapshot> snapshot)
	{
		foreach (var (cell, state) in snapshot)
		{
			groundLayer[cell.X, cell.Y] = state.Tile;
			caveRegionIndex[cell.X, cell.Y] = state.RegionIndex;
		}
	}

	private static bool[,] MarkAccessibleGround(TileId[,] groundLayer, WorldPosition start, int width, int height)
	{
		var accessible = new bool[width, height];

		if (start.X < 0 || start.Y < 0 || start.X >= width || start.Y >= height)
			return accessible;

		if (groundLayer[start.X, start.Y] != TileIds.Ground)
			return accessible;

		var queue = new Queue<WorldPosition>();
		queue.Enqueue(start);
		accessible[start.X, start.Y] = true;

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();

			foreach (var neighbor in FloorTraversal.GetNeighbors(current))
			{
				if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
					continue;

				if (accessible[neighbor.X, neighbor.Y])
					continue;

				if (groundLayer[neighbor.X, neighbor.Y] != TileIds.Ground)
					continue;

				accessible[neighbor.X, neighbor.Y] = true;
				queue.Enqueue(neighbor);
			}
		}

		return accessible;
	}

	private static List<TunnelCandidate> FindTunnelCandidates(
		HashSet<WorldPosition> blob,
		HashSet<WorldPosition> interior,
		TileId[,] groundLayer,
		bool[,] accessibleExterior,
		WorldGenerationConfig config,
		int width,
		int height)
	{
		var candidates = new List<TunnelCandidate>();
		var seen = new HashSet<string>();

		foreach (var interiorCell in interior)
		{
			foreach (var (dx, dy) in Directions)
			{
				for (var tunnelWidth = config.MinEntranceWidth; tunnelWidth <= config.MaxEntranceWidth; tunnelWidth++)
				{
					var (pdx, pdy) = Perpendicular(dx, dy);
					var rays = new List<IReadOnlyList<WorldPosition>>();

					for (var offset = 0; offset < tunnelWidth; offset++)
					{
						var startCell = new WorldPosition(
							interiorCell.X + (offset * pdx),
							interiorCell.Y + (offset * pdy));

						if (!interior.Contains(startCell))
							break;

						var ray = TryBuildTunnelRay(
							startCell,
							dx,
							dy,
							blob,
							groundLayer,
							accessibleExterior,
							config,
							width,
							height);

						if (ray is null)
							break;

						rays.Add(ray);
					}

					if (rays.Count != tunnelWidth)
						continue;

					var key = string.Join("|", rays.SelectMany(ray => ray).OrderBy(c => c.X).ThenBy(c => c.Y).Select(c => $"{c.X},{c.Y}"));
					if (!seen.Add(key))
						continue;

					candidates.Add(new TunnelCandidate(rays));
				}
			}
		}

		return candidates;
	}

	private static (int Pdx, int Pdy) Perpendicular(int dx, int dy) =>
		dx == 0 ? (1, 0) : (0, 1);

	private static List<WorldPosition>? TryBuildTunnelRay(
		WorldPosition interiorCell,
		int dx,
		int dy,
		HashSet<WorldPosition> blob,
		TileId[,] groundLayer,
		bool[,] accessibleExterior,
		WorldGenerationConfig config,
		int width,
		int height)
	{
		var current = new WorldPosition(interiorCell.X + dx, interiorCell.Y + dy);
		var path = new List<WorldPosition>();

		for (var depth = 0; depth < config.MaxEntranceDepth; depth++)
		{
			if (current.X < 0 || current.Y < 0 || current.X >= width || current.Y >= height)
				return null;

			if (!blob.Contains(current) || groundLayer[current.X, current.Y] != TileIds.Wall)
				return null;

			if (IsMapBorderCell(current.X, current.Y, width, height))
				return null;

			path.Add(current);

			if (TouchesAccessibleExterior(current, accessibleExterior, width, height) &&
			    path.Count >= config.MinEntranceDepth)
			{
				return path;
			}

			current = new WorldPosition(current.X + dx, current.Y + dy);
		}

		return null;
	}

	private static bool TouchesAccessibleExterior(
		WorldPosition cell,
		bool[,] accessibleExterior,
		int width,
		int height)
	{
		foreach (var neighbor in FloorTraversal.GetNeighbors(cell))
		{
			if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
				continue;

			if (accessibleExterior[neighbor.X, neighbor.Y])
				return true;
		}

		return false;
	}

	private static bool TryPlaceTunnelEntrance(
		IReadOnlyList<TunnelCandidate> tunnelCandidates,
		HashSet<WorldPosition> usedEntrances,
		int entranceWidth,
		int seed,
		WorldPosition anchor,
		int entranceIndex,
		TileId[,] groundLayer,
		int[,] caveRegionIndex,
		int regionId,
		int width,
		int height,
		out IReadOnlyList<WorldPosition> outerEntranceCells)
	{
		outerEntranceCells = Array.Empty<WorldPosition>();

		var available = tunnelCandidates
			.Where(candidate => candidate.Width == entranceWidth)
			.Where(candidate => candidate.AllCells.All(cell => !usedEntrances.Contains(cell)))
			.OrderBy(candidate => candidate.Rays[0][0].X)
			.ThenBy(candidate => candidate.Rays[0][0].Y)
			.ToList();

		if (available.Count == 0)
		{
			available = tunnelCandidates
				.Where(candidate => candidate.AllCells.All(cell => !usedEntrances.Contains(cell)))
				.OrderBy(candidate => Math.Abs(candidate.Width - entranceWidth))
				.ThenBy(candidate => candidate.Rays[0][0].X)
				.ThenBy(candidate => candidate.Rays[0][0].Y)
				.ToList();
		}

		if (available.Count == 0)
			return false;

		var pick = RandomIntInclusive(
			seed,
			anchor.X,
			anchor.Y,
			EntrancePickSalt + entranceIndex,
			0,
			available.Count - 1);

		var chosen = available[pick];
		var outerCells = new List<WorldPosition>();

		foreach (var ray in chosen.Rays)
		{
			if (ray.Count == 0)
				return false;

			outerCells.Add(ray[ray.Count - 1]);
		}

		foreach (var cell in chosen.AllCells)
		{
			if (IsMapBorderCell(cell.X, cell.Y, width, height))
				return false;

			groundLayer[cell.X, cell.Y] = TileIds.Ground;
			caveRegionIndex[cell.X, cell.Y] = regionId;
			usedEntrances.Add(cell);
		}

		outerEntranceCells = outerCells;
		return true;
	}

	private static int RandomIntInclusive(int seed, int x, int y, int salt, int min, int max)
	{
		if (max <= min)
			return min;

		var roll = DeterministicCellRandom.Roll(seed, x, y, salt);
		return min + (int)(roll * (max - min + 1));
	}
}
