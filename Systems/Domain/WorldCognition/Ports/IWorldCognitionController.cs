using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Domain.WorldCognition.Ports;

public interface IWorldCognitionController
{
	void AddPresence(Vector2 position, float amount);

	void AddDisturbance(Vector2 position, float amount);

	void AddAffinity(Vector2 position, AffinityType affinityType, float amount);

	WorldCell GetCell(Vector2 position);

	AwarenessState GetAwareness(Vector2 position);

	RegionalMood GetRegionalMood(Vector2 position);

	EcologicalInterest GetEcologicalInterest(Vector2 position);
}
