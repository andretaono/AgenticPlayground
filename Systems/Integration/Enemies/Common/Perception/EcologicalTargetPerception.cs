using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Integration.Enemies.Common.Perception;

public sealed class EcologicalTargetPerception : ITargetTrackingState
{
	public bool IsTracking { get; private set; }

	public Vector2 LastKnownTargetPosition { get; private set; }

	public float LastDetectionStrength { get; private set; }

	public void Update(
		IWorldCognitionController cognition,
		Vector2 agentPosition,
		Vector2 targetPosition,
		PerceptionConfig config)
	{
		var distanceToTarget = Distance(agentPosition, targetPosition);

		if (distanceToTarget <= config.DirectSightRange)
		{
			IsTracking = true;
			LastKnownTargetPosition = targetPosition;
			LastDetectionStrength = 100f;
			return;
		}

		if (TryAcquireScent(cognition, agentPosition, config, out var scentPosition, out var scentStrength))
		{
			IsTracking = true;
			LastKnownTargetPosition = scentPosition;
			LastDetectionStrength = scentStrength;
			return;
		}

		if (distanceToTarget <= config.LongRangeScentRadius)
		{
			var awareness = cognition.GetAwareness(targetPosition);
			if (awareness >= AwarenessState.Noticed)
			{
				IsTracking = true;
				LastKnownTargetPosition = targetPosition;
				LastDetectionStrength = (float)awareness;
				return;
			}
		}

		IsTracking = false;
		LastDetectionStrength = 0f;
	}

	private static bool TryAcquireScent(
		IWorldCognitionController cognition,
		Vector2 agentPosition,
		PerceptionConfig config,
		out Vector2 scentPosition,
		out float scentStrength)
	{
		scentPosition = agentPosition;
		scentStrength = 0f;

		var agentCellX = (int)MathF.Floor(agentPosition.X / config.CognitionCellSize);
		var agentCellY = (int)MathF.Floor(agentPosition.Y / config.CognitionCellSize);
		var scentRadiusCells = (int)MathF.Ceiling(config.LongRangeScentRadius / config.CognitionCellSize);

		for (var dx = -scentRadiusCells; dx <= scentRadiusCells; dx++)
		{
			for (var dy = -scentRadiusCells; dy <= scentRadiusCells; dy++)
			{
				var cellX = agentCellX + dx;
				var cellY = agentCellY + dy;

				if (cellX < 0 || cellY < 0 ||
				    cellX >= config.CognitionGridWidth ||
				    cellY >= config.CognitionGridHeight)
					continue;

				var cellCenter = new Vector2(
					(cellX + 0.5f) * config.CognitionCellSize,
					(cellY + 0.5f) * config.CognitionCellSize);

				if (Distance(agentPosition, cellCenter) > config.LongRangeScentRadius)
					continue;

				WorldCell cell;
				try
				{
					cell = cognition.GetCell(cellCenter);
				}
				catch (ArgumentOutOfRangeException)
				{
					continue;
				}

				var strength = cell.Presence + cell.Disturbance * 0.35f;
				if (strength <= scentStrength)
					continue;

				scentStrength = strength;
				scentPosition = cellCenter;
			}
		}

		return scentStrength >= config.ScentDetectionThreshold;
	}

	private static float Distance(Vector2 a, Vector2 b)
	{
		var dx = a.X - b.X;
		var dy = a.Y - b.Y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}
}
