using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Integration.Enemies.Common.Perception;

public interface ITargetTrackingState
{
	bool IsTracking { get; }

	Vector2 LastKnownTargetPosition { get; }

	float LastDetectionStrength { get; }
}
