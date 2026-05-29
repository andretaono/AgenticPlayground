using Game.Systems.Domain.World.Model;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Minimal visual mapping: maps common ids to console characters/labels.
/// Extend or replace in integration layer for engine-specific visuals.
/// </summary>
public sealed class DefaultTileVisualMapper
{
    public char MapToChar(TileId id)
    {
        return id.Id switch
        {
            "wall" => 'W',
            "ground" => '.',
            "water" => '~',
            "" => '?',
            _ => '?'
        };
    }

    public string MapToLabel(TileId id)
    {
        return id.Id switch
        {
            "wall" => "Wall",
            "ground" => "Ground",
            "water" => "Water",
            "" => "Unknown",
            _ => id.Id
        };
    }
}