using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.WorldCognition;
using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Presentation;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime.Core;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Runtime;

public sealed class GameRuntimeBuilder
{
	private readonly IGameMath _math;
	private readonly List<TickEntry> _extraTickables = new();

	private AgentMovementSystem? _movement;
	private AgentCommandSystem? _command;
	private AgentBehaviourSystem? _behaviour;
	private AgentCombatSystem? _combat;
	private EntityResourceSystem? _resources;
	private WorldCognitionSystem? _cognition;
	private IReadOnlyList<AgentId> _intentAgentIds = Array.Empty<AgentId>();
	private IInputSource? _inputSource;
	private AgentId? _inputAgentId;
	private EntityId? _inputEntityId;
	private EntityId? _playerEntityId;
	private IWorldPresenter? _presenter;
	private IActorRegistry? _actorRegistry;
	private CombatRuntimeServices? _combatServices;
	private GameSessionState? _sessionState;
	private ArcAttackAbilityDefinition? _playerAttackAbility;
	private IAgentFacingProvider? _facingProvider;
	private IReadOnlyDictionary<EntityId, EntityId>? _faceTargetByEntity;
	private VitalityCleanupServices? _vitalityCleanup;
	private float _worldUnitsPerTile = 1f;

	public GameRuntimeBuilder(IGameMath math)
	{
		_math = math ?? throw new ArgumentNullException(nameof(math));
	}

	public GameRuntimeBuilder WithMovement(
		IAgentMovementPolicy? policy = null,
		AgentMovementConfig? config = null)
	{
		_movement = new AgentMovementSystem(_math, policy ?? new PermissiveMovementPolicy(), config);
		return this;
	}

	public GameRuntimeBuilder WithExistingMovement(AgentMovementSystem movement)
	{
		_movement = movement ?? throw new ArgumentNullException(nameof(movement));
		return this;
	}

	public GameRuntimeBuilder WithCommand()
	{
		_command = new AgentCommandSystem();
		return this;
	}

	public GameRuntimeBuilder WithExistingCommand(AgentCommandSystem command)
	{
		_command = command ?? throw new ArgumentNullException(nameof(command));
		return this;
	}

	public GameRuntimeBuilder WithExistingCombat(AgentCombatSystem combat)
	{
		_combat = combat ?? throw new ArgumentNullException(nameof(combat));
		return this;
	}

	public GameRuntimeBuilder WithExistingResources(EntityResourceSystem resources)
	{
		_resources = resources ?? throw new ArgumentNullException(nameof(resources));
		return this;
	}

	public GameRuntimeBuilder WithBehaviour(AgentBehaviourSystem behaviourSystem)
	{
		_behaviour = behaviourSystem ?? throw new ArgumentNullException(nameof(behaviourSystem));
		return this;
	}

	public GameRuntimeBuilder WithCombat(IAbilityExecutor? executor = null)
	{
		_combat = new AgentCombatSystem(executor ?? new AbilityExecutor());
		return this;
	}

	public GameRuntimeBuilder WithResources()
	{
		_resources = new EntityResourceSystem();
		return this;
	}

	public GameRuntimeBuilder WithWorldCognition(WorldCognitionConfig config)
	{
		_cognition = new WorldCognitionSystem(config ?? throw new ArgumentNullException(nameof(config)));
		return this;
	}

	public GameRuntimeBuilder WithExistingCognition(WorldCognitionSystem cognition)
	{
		_cognition = cognition ?? throw new ArgumentNullException(nameof(cognition));
		return this;
	}

	public GameRuntimeBuilder WithIntentAgents(params AgentId[] agentIds)
	{
		_intentAgentIds = agentIds ?? throw new ArgumentNullException(nameof(agentIds));
		return this;
	}

	public GameRuntimeBuilder WithInput(IInputSource inputSource, AgentId agentId, EntityId entityId)
	{
		_inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
		_inputAgentId = agentId;
		_inputEntityId = entityId;
		return this;
	}

	public GameRuntimeBuilder WithPresenter(
		IWorldPresenter presenter,
		IActorRegistry actorRegistry,
		float worldUnitsPerTile = 1f)
	{
		_presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
		_actorRegistry = actorRegistry ?? throw new ArgumentNullException(nameof(actorRegistry));
		_worldUnitsPerTile = worldUnitsPerTile;
		return this;
	}

	public GameRuntimeBuilder WithCombatRuntime(CombatRuntimeServices combatServices)
	{
		_combatServices = combatServices ?? throw new ArgumentNullException(nameof(combatServices));
		return this;
	}

	public GameRuntimeBuilder WithSessionState(GameSessionState sessionState)
	{
		_sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
		return this;
	}

	public GameRuntimeBuilder WithPlayerEntity(EntityId playerEntityId)
	{
		_playerEntityId = playerEntityId;
		return this;
	}

	public GameRuntimeBuilder WithPlayerAttackAbility(ArcAttackAbilityDefinition attackAbility)
	{
		_playerAttackAbility = attackAbility ?? throw new ArgumentNullException(nameof(attackAbility));
		return this;
	}

	public GameRuntimeBuilder WithFacingProvider(IAgentFacingProvider facingProvider)
	{
		_facingProvider = facingProvider ?? throw new ArgumentNullException(nameof(facingProvider));
		return this;
	}

	public GameRuntimeBuilder WithFaceTargets(IReadOnlyDictionary<EntityId, EntityId> faceTargetByEntity)
	{
		_faceTargetByEntity = faceTargetByEntity ??
		                       throw new ArgumentNullException(nameof(faceTargetByEntity));
		return this;
	}

	public GameRuntimeBuilder WithVitalityCleanup(VitalityCleanupServices vitalityCleanup)
	{
		_vitalityCleanup = vitalityCleanup ?? throw new ArgumentNullException(nameof(vitalityCleanup));
		return this;
	}

	public GameRuntimeBuilder WithExtraTickable(ITickable tickable, int order)
	{
		_extraTickables.Add(new TickEntry(tickable, order));
		return this;
	}

	public GameRuntime Build()
	{
		if (_movement is null)
			throw new InvalidOperationException("Movement is required. Call WithMovement().");
		if (_command is null)
			throw new InvalidOperationException("Command is required. Call WithCommand().");

		var entries = new List<TickEntry>(_extraTickables);

		if (_combatServices is not null)
			entries.Add(new TickEntry(new CombatClockAdapter(_combatServices), StandardTickOrder.PreCognition));

		if (_cognition is not null)
		{
			entries.Add(new TickEntry(
				new WorldCognitionSimulationAdapter(_cognition.Simulation),
				StandardTickOrder.WorldCognition));
		}

		if (_behaviour is not null)
		{
			entries.Add(new TickEntry(
				new AgentBehaviourSimulationAdapter(_behaviour.Simulation),
				StandardTickOrder.AgentBehaviour));

			if (_intentAgentIds.Count > 0)
			{
				var intentAdapter = new BehaviourIntentToCommandAdapter(_behaviour.Output, _command);
				entries.Add(new TickEntry(
					new BehaviourIntentSubmissionAdapter(intentAdapter, _intentAgentIds),
					StandardTickOrder.BehaviourIntentSubmission));
			}
		}

		if (_combatServices is not null &&
		    _actorRegistry is not null &&
		    _movement is not null)
		{
			entries.Add(new TickEntry(
				new AgentOrientationSyncAdapter(
					_combatServices.Orientation,
					_movement,
					_actorRegistry,
					_facingProvider,
					_faceTargetByEntity),
				StandardTickOrder.AgentOrientation));
		}

		if (_inputSource is not null && _inputAgentId is not null && _inputEntityId is not null)
		{
			entries.Add(new TickEntry(
				new InputToCommandAdapter(
					_inputSource,
					_command,
					_inputAgentId.Value,
					_inputEntityId.Value,
					_sessionState,
					_combatServices?.CooldownTracker,
					_playerAttackAbility ?? ArcAttackAbilityDefinition.Default,
					_combatServices),
				StandardTickOrder.Input));
		}

		var commandExecution = new AgentCommandExecutionAdapter(
			_command,
			_movement.Input,
			_math,
			_combat?.Registry);

		entries.Add(new TickEntry(commandExecution, StandardTickOrder.CommandExecution));

		if (_combat is not null)
		{
			entries.Add(new TickEntry(
				new AgentCombatSimulationAdapter(_combat.Simulation),
				StandardTickOrder.AgentCombat));
		}

		if (_resources is not null &&
		    _sessionState is not null &&
		    _actorRegistry is not null)
		{
			entries.Add(new TickEntry(
				new VitalityMonitorAdapter(
					_resources,
					_sessionState,
					_playerEntityId ?? _inputEntityId ?? default,
					_actorRegistry,
					_vitalityCleanup),
				StandardTickOrder.Vitality));
		}

		entries.Add(new TickEntry(
			new AgentMovementSimulationAdapter(_movement.Simulation),
			StandardTickOrder.AgentMovement));

		if (_presenter is not null && _actorRegistry is not null)
		{
			entries.Add(new TickEntry(
				new WorldPresentationAdapter(
					_presenter,
					_actorRegistry,
					_movement,
					_sessionState),
				StandardTickOrder.WorldPresentation));

			if (_resources is not null && _combatServices is not null && _sessionState is not null)
			{
				entries.Add(new TickEntry(
					new CombatPresentationAdapter(
						_presenter,
						_actorRegistry,
						_resources,
						_combatServices,
						_sessionState),
					StandardTickOrder.CombatPresentation));
			}
		}

		if (_resources is not null)
		{
			entries.Add(new TickEntry(
				new EntityResourceSimulationAdapter(_resources.Simulation),
				StandardTickOrder.EntityResource));
		}

		var systems = new GameSystems(_math)
		{
			Movement = _movement,
			Command = _command,
			Behaviour = _behaviour,
			Combat = _combat,
			Resources = _resources,
			Cognition = _cognition
		};

		var runtime = new RuntimeSystem(new DefaultTickSchedule(entries));
		return new GameRuntime(runtime, systems);
	}
}
