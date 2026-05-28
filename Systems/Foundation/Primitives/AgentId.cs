namespace Game.Systems.Foundation.Primitives;

/// <summary>
/// Identifier for simulation agents.
/// </summary>
public readonly record struct AgentId(int Value)
{
    public override string ToString() => Value.ToString();
}
