using Game.Systems.Domain.AgentBehaviour.Model;

namespace Game.Systems.Domain.AgentBehaviour.Ports;

public interface IBehaviour
{
	BehaviourId Id { get; }
	int Priority { get; }

	bool CanExecute(BehaviourContext context);
	IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context);
}
