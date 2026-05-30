using System;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Flags describing gameplay semantics for a tile id.
/// Included here to keep the rule definition colocated with the default provider.
/// </summary>
[Flags]
public enum TileRules
{
    None = 0,
    Walkable = 1,
    BlocksMovement = 2,
    Swimable = 4
}

/// <summary>
/// Example mapping from tile id to gameplay rules.
/// Combine flags to represent composite behavior.
/// </summary>
public sealed class DefaultTileRulesProvider : ITileRulesProvider
{
    public TileRules GetRules(TileId id)
    {
        return id.Id switch
        {
            "wall" => TileRules.BlocksMovement,
            "ground" => TileRules.Walkable,
            "water" => TileRules.Swimable,
            "" => TileRules.None,
            _ => TileRules.None
        };
    }
}