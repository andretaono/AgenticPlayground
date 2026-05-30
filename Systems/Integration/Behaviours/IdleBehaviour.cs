using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;

namespace Game.Systems.Integration.Behaviours;

public sealed class IdleBehaviour : IBehaviour
{
	public BehaviourId Id => new("idle");
	public int Priority => int.MinValue;

	public bool CanExecute(BehaviourContext context) => true;

	public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context) =>
		Array.Empty<IBehaviourIntent>();
}
