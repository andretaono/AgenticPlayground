namespace Game.Systems.Domain.WorldCognition.Model;

public readonly record struct AffinityCell(float Bear, float Raven, float Seal)
{
	public static AffinityCell Zero => new(0f, 0f, 0f);

	public AffinityCell Add(AffinityType affinityType, float amount) =>
		affinityType switch
		{
			AffinityType.Bear => this with { Bear = Bear + amount },
			AffinityType.Raven => this with { Raven = Raven + amount },
			AffinityType.Seal => this with { Seal = Seal + amount },
			_ => throw new ArgumentOutOfRangeException(nameof(affinityType))
		};

	public AffinityCell Clamp(float min, float max) =>
		new(
			Math.Clamp(Bear, min, max),
			Math.Clamp(Raven, min, max),
			Math.Clamp(Seal, min, max));
}
