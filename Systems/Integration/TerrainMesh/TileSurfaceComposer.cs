using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.Adapters;

namespace Game.Systems.Integration.TerrainMesh;

public sealed class TileSurfaceComposer
{
	private readonly ITileRulesProvider _tileRules;
	private readonly TileBlockLayoutBuilder _layoutBuilder = new();
	private readonly VisibleSurfaceMeshBuilder _meshBuilder = new();

	public TileSurfaceComposer(ITileRulesProvider tileRules)
	{
		_tileRules = tileRules ?? throw new ArgumentNullException(nameof(tileRules));
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
		var heightScale = mapping.TerrainConfig?.HeightScale ?? 1f;
		var blocks = _layoutBuilder.Build(
			map,
			_tileRules,
			mapping.WorldUnitsPerTile,
			heightScale,
			settings);

		return _meshBuilder.Build(blocks);
	}
}
