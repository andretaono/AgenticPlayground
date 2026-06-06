using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Enemies.Common.Config;

namespace Game.Systems.Integration.Enemies.Common.Behaviours;

public sealed class PatrolBehaviour : IBehaviour
{
	private readonly EnemyTacticalConfig _config;
	private Vector2 _direction = new(0f, 1f);
	private float _distanceTraveled;

	public PatrolBehaviour(EnemyTacticalConfig config)
	{
		_config = config ?? throw new ArgumentNullException(nameof(config));
	}

	public BehaviourId Id => new($"{_config.IdPrefix}-patrol");
	public int Priority => _config.PatrolPriority;

	public bool CanExecute(BehaviourContext context) => !context.HasTarget;

	public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context)
	{
		_distanceTraveled += 1f;
		if (_distanceTraveled >= _config.PatrolTurnDistance)
		{
			_direction = new Vector2(-_direction.Y, _direction.X);
			_distanceTraveled = 0f;
		}

		return new IBehaviourIntent[]
		{
			new MoveBehaviourIntent(context.Agent, _direction)
		};
	}
}
