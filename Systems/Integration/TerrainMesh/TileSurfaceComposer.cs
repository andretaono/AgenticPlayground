using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.Adapters;

namespace Game.Systems.Integration.TerrainMesh;

public sealed class TileSurfaceComposer
{
	private readonly ITileRulesProvider _tileRules;
	private readonly TileBlockLayoutBuilder _layoutBuilder = new();
	private readonly VisibleSurfaceMeshBuilder _meshBuilder = new();
	private readonly IReadOnlyList<ITileSurfaceMeshPostProcessor> _postProcessors;

	public TileSurfaceComposer(
		ITileRulesProvider tileRules,
		IEnumerable<ITileSurfaceMeshPostProcessor>? postProcessors = null)
	{
		_tileRules = tileRules ?? throw new ArgumentNullException(nameof(tileRules));
		_postProcessors = (postProcessors ?? DefaultPostProcessors()).ToList();
	}

	private static IEnumerable<ITileSurfaceMeshPostProcessor> DefaultPostProcessors()
	{
		yield return new FaceSubdivisionSmoothingPostProcessor();
		yield return new NormalSmoothingPostProcessor();
	}

	public TileSurfaceMeshResult Compose(
		GeneratedWorldMap map,
		WorldTerrainMapping mapping,
		TileHeightModifierSettings? modifierSettings = null)
	{
		if (map is null)
			throw new ArgumentNullException(nameof(map));
		if (mapping is null)
			throw new ArgumentNullException(nameof(mapping));

		var settings = mapping.ModifierSettings ?? modifierSettings ?? new TileHeightModifierSettings();
		var surfaceSettings = mapping.SurfaceSettings ?? new TileSurfaceMeshSettings();
		var heightScale = mapping.TerrainConfig?.HeightScale ?? 1f;
		var blocks = _layoutBuilder.Build(
			map,
			_tileRules,
			mapping.WorldUnitsPerTile,
			heightScale,
			settings);

		var result = _meshBuilder.Build(blocks);
		foreach (var postProcessor in _postProcessors)
			result = postProcessor.Process(result, surfaceSettings, mapping.WorldUnitsPerTile);

		return result;
	}
}
