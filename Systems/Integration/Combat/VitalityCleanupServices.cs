using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Presentation.Ports;

namespace Game.Systems.Integration.Combat;

public sealed class VitalityCleanupServices : IActorLifecycleCleanup
{
	public VitalityCleanupServices(
		IActorRegistry actorRegistry,
		AgentMovementSystem movement,
		AgentCommandSystem commandSystem,
		ICombatEntityRegistry combatRegistry,
		IWorldPresenter? presenter = null,
		IBehaviourController? behaviourController = null)
	{
		ActorRegistry = actorRegistry;
		Movement = movement;
		CommandSystem = commandSystem;
		CombatRegistry = combatRegistry;
		Presenter = presenter;
		BehaviourController = behaviourController;
	}

	public IActorRegistry ActorRegistry { get; }
	public AgentMovementSystem Movement { get; }
	public AgentCommandSystem CommandSystem { get; }
	public ICombatEntityRegistry CombatRegistry { get; }
	public IWorldPresenter? Presenter { get; }
	public IBehaviourController? BehaviourController { get; }

	public void RemoveDeadActor(ActorHandle actor)
	{
		if (CombatRegistry.TryGet(actor.EntityId, out var combatEntity))
			CombatRegistry.Unregister(combatEntity);

		Movement.Registry.RemoveAgent(actor.EntityId);
		CommandSystem.UnregisterAgent(actor.AgentId);
		BehaviourController?.UnregisterAgent(actor.AgentId);
		ActorRegistry.RemoveActor(actor);
		Presenter?.RemoveActor(actor.EntityId);
	}
}
