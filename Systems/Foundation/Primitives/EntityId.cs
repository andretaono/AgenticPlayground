namespace Game.Foundation.Primitives;

public readonly record struct EntityId(int Value)
{
    public override string ToString() => Value.ToString();
}

