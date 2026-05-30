using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCommand;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Behaviours;

namespace Game.Scenarios;

public sealed class AgentBehaviourDemo : IScenario
{
	public string Name => "agent-behaviour";

	public void Run()
	{
		RunChaseThenAttackCase();
		Console.WriteLine();
		RunIdleFallbackCase();
		Console.WriteLine();
		RunCommandPipelineCase();
	}

	private static void RunChaseThenAttackCase()
	{
		Console.WriteLine("=== Case 1: Chase, then attack when in range ===");

		var agentId = new AgentId(1);
		var target = new EntityId(99);
		const float attackRange = 2f;
		var agentPosition = new Vector2(0f, 0f);
		var targetPosition = new Vector2(6f, 0f);
		var lastDistance = float.PositiveInfinity;

		var contextProvider = new ScriptedContextProvider(agentId, () =>
		{
			var delta = new Vector2(targetPosition.X - agentPosition.X, targetPosition.Y - agentPosition.Y);
			lastDistance = delta.Magnitude();
			var inRange = lastDistance <= attackRange;

			return new BehaviourContext
			{
				Agent = agentId,
				Position = agentPosition,
				TargetEntity = target,
				TargetDirection = delta.Magnitude() <= 1e-6f ? Vector2.Zero : delta.Normalized(),
				TargetInAttackRange = inRange
			};
		});

		var behaviourSystem = new AgentBehaviourSystem(contextProvider, new IdleBehaviour());
		behaviourSystem.Behaviour.AddBehaviour(agentId, new ChaseBehaviour(priority: 10));
		behaviourSystem.Behaviour.AddBehaviour(agentId, new AttackBehaviour(priority: 20));

		Console.WriteLine($"Attack range required: {attackRange:F1}");

		for (var i = 0; i < 4; i++)
		{
			behaviourSystem.Simulation.Tick(1f / 20f);
			PrintTickResult(behaviourSystem, agentId, i + 1, lastDistance, attackRange);

			// Simulate closing distance between ticks.
			agentPosition = new Vector2(agentPosition.X + 1.5f, agentPosition.Y);
		}
	}

	private static void RunIdleFallbackCase()
	{
		Console.WriteLine("=== Case 2: Idle when no behaviour is valid ===");

		var agentId = new AgentId(2);
		var contextProvider = new ScriptedContextProvider(agentId, () => new BehaviourContext
		{
			Agent = agentId,
			Position = Vector2.Zero,
			TargetEntity = null,
			TargetDirection = Vector2.Zero,
			TargetInAttackRange = false
		});

		var behaviourSystem = new AgentBehaviourSystem(contextProvider, new IdleBehaviour());
		behaviourSystem.Behaviour.AddBehaviour(agentId, new ChaseBehaviour());
		behaviourSystem.Behaviour.AddBehaviour(agentId, new AttackBehaviour());

		behaviourSystem.Simulation.Tick(1f / 20f);
		PrintTickResult(behaviourSystem, agentId, 1);
	}

	private static void RunCommandPipelineCase()
	{
		Console.WriteLine("=== Case 3: Intents converted into AgentCommands ===");

		var agentId = new AgentId(3);
		var target = new EntityId(42);
		var contextProvider = new ScriptedContextProvider(agentId, () => new BehaviourContext
		{
			Agent = agentId,
			Position = new Vector2(2f, 2f),
			TargetEntity = target,
			TargetDirection = new Vector2(0f, 1f),
			TargetInAttackRange = false
		});

		var behaviourSystem = new AgentBehaviourSystem(contextProvider, new IdleBehaviour());
		var commandSystem = new AgentCommandSystem();
		commandSystem.RegisterAgent(agentId);

		behaviourSystem.Behaviour.AddBehaviour(agentId, new ChaseBehaviour());
		behaviourSystem.Simulation.Tick(1f / 20f);

		var intentAdapter = new BehaviourIntentToCommandAdapter(behaviourSystem.Output, commandSystem);
		intentAdapter.SubmitEmittedIntents(agentId);

		Console.WriteLine($"Commands submitted: {commandSystem.GetCommands().Count}");
		foreach (var command in commandSystem.GetCommands())
			Console.WriteLine($"- {command.GetType().Name} for agent {command.Agent}");
	}

	private static void PrintTickResult(
		AgentBehaviourSystem behaviourSystem,
		AgentId agentId,
		int tickNumber,
		float? distanceToTarget = null,
		float? attackRange = null)
	{
		var active = behaviourSystem.Output.GetActiveBehaviour(agentId);
		var intents = behaviourSystem.Output.GetEmittedIntents(agentId);

		var rangeInfo = distanceToTarget.HasValue && attackRange.HasValue
			? $", distance={distanceToTarget.Value:F1}, attackRange={attackRange.Value:F1}"
			: string.Empty;

		Console.WriteLine($"Tick {tickNumber}: active={active?.Id.ToString() ?? "none"}, intents={intents.Count}{rangeInfo}");
		foreach (var intent in intents)
			Console.WriteLine($"  - {DescribeIntent(intent)}");
	}

	private static string DescribeIntent(IBehaviourIntent intent) => intent switch
	{
		MoveBehaviourIntent move => $"Move ({move.Direction.X:F1}, {move.Direction.Y:F1})",
		AttackBehaviourIntent => "Attack",
		_ => intent.GetType().Name
	};

	private sealed class ScriptedContextProvider : IBehaviourContextProvider
	{
		private readonly AgentId _agentId;
		private readonly Func<BehaviourContext> _factory;

		public ScriptedContextProvider(AgentId agentId, Func<BehaviourContext> factory)
		{
			_agentId = agentId;
			_factory = factory;
		}

		public BehaviourContext GetContext(AgentId agentId)
		{
			if (!agentId.Equals(_agentId))
				throw new KeyNotFoundException($"No scripted context for agent '{agentId}'.");

			return _factory();
		}
	}
}
