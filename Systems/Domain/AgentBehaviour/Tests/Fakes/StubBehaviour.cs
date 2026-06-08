using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;

namespace Game.Systems.Domain.AgentBehaviour.Tests.Fakes;

public sealed class StubBehaviour : IBehaviour
{
	private readonly Func<BehaviourContext, bool>? _canExecute;
	private readonly Func<BehaviourContext, IReadOnlyList<IBehaviourIntent>>? _execute;

	public StubBehaviour(
		BehaviourId id,
		int priority,
		Func<BehaviourContext, bool>? canExecute = null,
		Func<BehaviourContext, IReadOnlyList<IBehaviourIntent>>? execute = null)
	{
		Id = id;
		Priority = priority;
		_canExecute = canExecute;
		_execute = execute;
	}

	public BehaviourId Id { get; }
	public int Priority { get; }

	public bool CanExecute(BehaviourContext context) =>
		_canExecute?.Invoke(context) ?? true;

	public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context) =>
		_execute?.Invoke(context) ?? Array.Empty<IBehaviourIntent>();
}
