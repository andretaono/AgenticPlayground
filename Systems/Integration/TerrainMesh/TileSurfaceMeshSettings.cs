namespace Game.Systems.Integration.TerrainMesh;

public sealed class TileSurfaceMeshSettings
{
	public bool EnableNormalSmoothing { get; init; } = true;

	/// <summary>Smooth wall and ceiling meshes together so shared corners match.</summary>
	public bool EnableStructuralNormalSmoothing { get; init; } = true;

	/// <summary>Triangles with face normal Y above this value keep flat hard shading.</summary>
	public float UpHardNormalThreshold { get; init; } = 0.9f;

	public float WeldEpsilon { get; init; } = 1e-4f;
}
