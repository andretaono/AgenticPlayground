using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.WorldCognition;
using Game.Systems.Foundation.GameMath.Interfaces;

namespace Game.Systems.Integration.Runtime;

public sealed class GameSystems
{
	public GameSystems(IGameMath math)
	{
		Math = math ?? throw new ArgumentNullException(nameof(math));
	}

	public IGameMath Math { get; }
	public AgentMovementSystem? Movement { get; init; }
	public AgentCommandSystem? Command { get; init; }
	public AgentBehaviourSystem? Behaviour { get; init; }
	public AgentCombatSystem? Combat { get; init; }
	public EntityResourceSystem? Resources { get; init; }
	public WorldCognitionSystem? Cognition { get; init; }
}
