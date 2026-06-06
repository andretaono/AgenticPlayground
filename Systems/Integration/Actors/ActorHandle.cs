using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Actors;

public readonly record struct ActorHandle(AgentId AgentId, EntityId EntityId);
