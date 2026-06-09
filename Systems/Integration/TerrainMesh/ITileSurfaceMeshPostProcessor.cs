namespace Game.Systems.Integration.TerrainMesh;

public interface ITileSurfaceMeshPostProcessor
{
	TileSurfaceMeshResult Process(
		TileSurfaceMeshResult mesh,
		TileSurfaceMeshSettings settings,
		float cellSize);
}
