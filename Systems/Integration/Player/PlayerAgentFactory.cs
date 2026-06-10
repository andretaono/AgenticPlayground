using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Navigation;

namespace Game.Systems.Integration.Player;

public sealed class PlayerAgentFactory
{
	public PlayerAgentHandle Register(
		AgentMovementSystem movement,
		AgentCombatSystem combat,
		EntityResourceSystem resources,
		CombatRuntimeServices combatServices,
		ActorHandle player,
		PlayerConfig config,
		ArcAttackAbilityDefinition? attackAbility = null)
	{
		if (movement is null)
			throw new ArgumentNullException(nameof(movement));
		if (combat is null)
			throw new ArgumentNullException(nameof(combat));
		if (resources is null)
			throw new ArgumentNullException(nameof(resources));
		if (combatServices is null)
			throw new ArgumentNullException(nameof(combatServices));

		var ability = attackAbility ?? ArcAttackAbilityDefinition.Default;
		var getPosition = MovementPositionQuery.Create(movement);

		CombatEntityRegistrar.RegisterArcAttacker(
			combat,
			resources,
			combatServices,
			player.EntityId,
			ability,
			config.MaxHealth,
			getPosition);

		return new PlayerAgentHandle(player, config);
	}
}

public sealed class PlayerAgentHandle
{
	public PlayerAgentHandle(ActorHandle player, PlayerConfig config)
	{
		Player = player;
		Config = config;
	}

	public ActorHandle Player { get; }
	public PlayerConfig Config { get; }
	public AgentId AgentId => Player.AgentId;
	public EntityId EntityId => Player.EntityId;
}
