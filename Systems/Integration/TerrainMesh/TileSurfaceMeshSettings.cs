namespace Game.Systems.Integration.TerrainMesh;

public sealed class TileSurfaceMeshSettings
{
	public bool EnableNormalSmoothing { get; init; } = true;

	/// <summary>Smooth wall and ceiling meshes together so shared corners match.</summary>
	public bool EnableStructuralNormalSmoothing { get; init; } = true;

	/// <summary>Triangles with face normal Y above this value keep flat hard shading.</summary>
	public float UpHardNormalThreshold { get; init; } = 0.9f;

	/// <summary>
	/// Minimum dot(smoothedNormal, faceNormal) after soft averaging (0 disables).
	/// Pulls corner normals back toward their face plane to reduce overly dark lighting.
	/// </summary>
	public float SoftNormalMinFaceDot { get; init; } = 0.6f;

	public float WeldEpsilon { get; init; } = 1e-4f;

	public bool EnableGeometrySmoothing { get; init; } = false;

	/// <summary>Grid splits per face edge. One split yields 2×2 quads; two yields 3×3.</summary>
	public int GeometrySmoothDivisions { get; init; } = 1;

	/// <summary>Blend toward neighbor-average position (0–1). Upward vertices move in XZ only.</summary>
	public float GeometrySmoothStrength { get; init; } = 0.35f;

	/// <summary>Relax outdoor ground and cave ground together at shared positions.</summary>
	public bool EnableGroundGeometrySmoothing { get; init; } = true;

	/// <summary>Relax wall and ceiling geometry together at shared positions.</summary>
	public bool EnableStructuralGeometrySmoothing { get; init; } = true;
}
