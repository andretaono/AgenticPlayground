using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Presentation.Ports;

namespace Game.Tests.Integration.Runners;

public sealed class ScriptedInputSource : IInputSource
{
	private readonly AgentId _boundAgentId;
	private readonly Func<int, Vector2> _directionFactory;
	private int _pollCount;

	public ScriptedInputSource(AgentId boundAgentId, Vector2 constantDirection)
		: this(boundAgentId, _ => constantDirection)
	{
	}

	public ScriptedInputSource(AgentId boundAgentId, Func<int, Vector2> directionFactory)
	{
		_boundAgentId = boundAgentId;
		_directionFactory = directionFactory ?? throw new ArgumentNullException(nameof(directionFactory));
	}

	public int PollCount => _pollCount;

	public Vector2 PollMovementInput(AgentId agentId)
	{
		if (!agentId.Equals(_boundAgentId))
			return Vector2.Zero;

		_pollCount++;
		return _directionFactory(_pollCount);
	}

	public bool PollAttackInput(AgentId agentId) => false;
}
