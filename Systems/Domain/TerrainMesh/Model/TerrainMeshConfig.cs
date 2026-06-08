namespace Game.Systems.Domain.TerrainMesh.Model;

public sealed class TerrainMeshConfig
{
	public float CellSize { get; init; } = 1f;
	public float HeightScale { get; init; } = 1f;
	public float MinHeight { get; init; } = 0f;
	public float MaxHeight { get; init; } = 10f;
	public float NoiseFrequency { get; init; } = 0.08f;
	public int NoiseOctaves { get; init; } = 4;
	public float NoisePersistence { get; init; } = 0.5f;
	public float NoiseLacunarity { get; init; } = 2f;
}
