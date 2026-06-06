using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Enemies.Common.Behaviours;
using Game.Systems.Integration.Enemies.Common.Perception;

namespace Game.Systems.Integration.Enemies.Raven;

public sealed class RavenAgentFactory
{
	public RavenAgentHandle Register(
		AgentId ravenAgentId,
		EntityId ravenEntityId,
		EntityId targetEntityId,
		RavenConfig config,
		EcologicalTargetPerception perception,
		IBehaviourController behaviourController)
	{
		var tacticalConfig = config.ToTacticalConfig();

		behaviourController.AddBehaviour(ravenAgentId, new PatrolBehaviour(tacticalConfig));
		behaviourController.AddBehaviour(
			ravenAgentId,
			new ObserveTargetBehaviour(perception, tacticalConfig, config.ObserveDistance));

		return new RavenAgentHandle(ravenAgentId, ravenEntityId, targetEntityId, perception, config);
	}
}

public sealed class RavenAgentHandle
{
	public RavenAgentHandle(
		AgentId agentId,
		EntityId entityId,
		EntityId targetEntityId,
		EcologicalTargetPerception perception,
		RavenConfig config)
	{
		AgentId = agentId;
		EntityId = entityId;
		TargetEntityId = targetEntityId;
		Perception = perception;
		Config = config;
	}

	public AgentId AgentId { get; }
	public EntityId EntityId { get; }
	public EntityId TargetEntityId { get; }
	public EcologicalTargetPerception Perception { get; }
	public RavenConfig Config { get; }
}
