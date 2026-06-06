using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Actors;

public sealed class ActorRegistry : IActorRegistry
{
	private readonly AgentCommandSystem _commandSystem;
	private readonly AgentMovementSystem _movement;
	private readonly List<ActorHandle> _actors = new();
	private int _nextId = 1;

	public ActorRegistry(AgentCommandSystem commandSystem, AgentMovementSystem movement)
	{
		_commandSystem = commandSystem ?? throw new ArgumentNullException(nameof(commandSystem));
		_movement = movement ?? throw new ArgumentNullException(nameof(movement));
	}

	public IReadOnlyList<ActorHandle> Actors => _actors;

	public ActorHandle RegisterActor(IVector3 position)
	{
		var id = _nextId++;
		var agentId = new AgentId(id);
		var entityId = new EntityId(id);

		_commandSystem.RegisterAgent(agentId);
		_movement.Registry.CreateAgent(entityId, position);

		var handle = new ActorHandle(agentId, entityId);
		_actors.Add(handle);
		return handle;
	}

	public EntityId RegisterEntity(IVector3 position)
	{
		var id = _nextId++;
		var entityId = new EntityId(id);
		_movement.Registry.CreateAgent(entityId, position);
		return entityId;
	}

	public bool TryGetActor(EntityId entityId, out ActorHandle handle)
	{
		foreach (var actor in _actors)
		{
			if (actor.EntityId.Equals(entityId))
			{
				handle = actor;
				return true;
			}
		}

		handle = default;
		return false;
	}
}
