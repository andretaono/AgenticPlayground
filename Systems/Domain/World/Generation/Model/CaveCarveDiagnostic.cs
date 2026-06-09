namespace Game.Systems.Domain.World.Generation.Model;

public sealed class CaveCarveDiagnostic
{
	public CaveCarveDiagnostic(int attemptedCount, int createdCount, IReadOnlyList<CarvedCaveInfo> caves)
	{
		AttemptedCount = attemptedCount;
		CreatedCount = createdCount;
		Caves = caves ?? throw new ArgumentNullException(nameof(caves));
	}

	public static CaveCarveDiagnostic Empty { get; } = new(0, 0, Array.Empty<CarvedCaveInfo>());

	public int AttemptedCount { get; }

	public int CreatedCount { get; }

	public IReadOnlyList<CarvedCaveInfo> Caves { get; }
}
