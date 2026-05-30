namespace Game.Systems.Domain.AgentBehaviour.Model;

public readonly record struct BehaviourId(string Id)
{
	public override string ToString() => Id;
}
