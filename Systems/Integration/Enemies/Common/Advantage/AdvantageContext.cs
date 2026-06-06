using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Enemies.Common.Advantage;

public sealed class AdvantageContext
{
	public AdvantageContext(
		EntityId targetEntity,
		Vector2 agentPosition,
		Vector2 lastKnownTargetPosition,
		IWorldCognitionController cognition,
		IEntityResourceRegistry resources)
	{
		TargetEntity = targetEntity;
		AgentPosition = agentPosition;
		LastKnownTargetPosition = lastKnownTargetPosition;
		Cognition = cognition ?? throw new ArgumentNullException(nameof(cognition));
		Resources = resources ?? throw new ArgumentNullException(nameof(resources));
	}

	public EntityId TargetEntity { get; }
	public Vector2 AgentPosition { get; }
	public Vector2 LastKnownTargetPosition { get; }
	public IWorldCognitionController Cognition { get; }
	public IEntityResourceRegistry Resources { get; }
}
