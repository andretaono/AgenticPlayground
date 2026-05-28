namespace Game.Systems.Foundation.GameMath.Interfaces;

/// <summary>
/// Engine-agnostic 3D vector contract used by gameplay systems.
/// </summary>
public interface IVector3
{
    float X { get; }
    float Y { get; }
    float Z { get; }
}

