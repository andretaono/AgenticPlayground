namespace Game.Systems.Integration.Items;

public readonly record struct ModifierDomain(string Value)
{
	public static readonly ModifierDomain Ground = new("Ground");
	public static readonly ModifierDomain Aerial = new("Aerial");
	public static readonly ModifierDomain Oceanic = new("Oceanic");
	public static readonly ModifierDomain Beast = new("Beast");
	public static readonly ModifierDomain Spirit = new("Spirit");
	public static readonly ModifierDomain Stone = new("Stone");
	public static readonly ModifierDomain Shadow = new("Shadow");

	public override string ToString() => Value;
}
