using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCommand;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Behaviours;

namespace Game.Tests.Integration.Runners;

public sealed class AgentBehaviourIntegrationRunner
{
	public AgentBehaviourChaseResult RunChaseThenAttack()
	{
		var agentId = new AgentId(1);
		var target = new EntityId(99);
		const float attackRange = 2f;
		var agentPosition = new Vector2(0f, 0f);
		var targetPosition = new Vector2(6f, 0f);
		var lastDistance = float.PositiveInfinity;
		var sawChase = false;
		var sawAttack = false;

		var contextProvider = new ScriptedContextProvider(agentId, () =>
		{
			var delta = new Vector2(targetPosition.X - agentPosition.X, targetPosition.Y - agentPosition.Y);
			lastDistance = delta.Magnitude();

			return new BehaviourContext
			{
				Agent = agentId,
				Position = agentPosition,
				TargetEntity = target,
				TargetDirection = lastDistance <= 1e-6f ? Vector2.Zero : delta.Normalized(),
				TargetInAttackRange = lastDistance <= attackRange
			};
		});

		var behaviourSystem = new AgentBehaviourSystem(contextProvider, new IdleBehaviour());
		behaviourSystem.Behaviour.AddBehaviour(agentId, new ChaseBehaviour(priority: 10));
		behaviourSystem.Behaviour.AddBehaviour(agentId, new AttackBehaviour(priority: 20));

		for (var i = 0; i < 6; i++)
		{
			behaviourSystem.Simulation.Tick(1f / 20f);
			var active = behaviourSystem.Output.GetActiveBehaviour(agentId)?.Id.ToString();

			if (active == "chase")
				sawChase = true;

			if (active == "attack")
				sawAttack = true;

			agentPosition = new Vector2(agentPosition.X + 1.5f, agentPosition.Y);
		}

		var finalActive = behaviourSystem.Output.GetActiveBehaviour(agentId)?.Id.ToString() ?? "none";
		var finalIntents = behaviourSystem.Output.GetEmittedIntents(agentId);

		return new AgentBehaviourChaseResult(
			SawChase: sawChase,
			SawAttack: sawAttack,
			FinalActiveBehaviour: finalActive,
			FinalDistance: lastDistance,
			FinalIntentCount: finalIntents.Count);
	}

	public AgentBehaviourIdleResult RunIdleFallback()
	{
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

		var active = behaviourSystem.Output.GetActiveBehaviour(agentId)?.Id.ToString() ?? "none";

		return new AgentBehaviourIdleResult(ActiveBehaviour: active);
	}

	public AgentBehaviourCommandPipelineResult RunCommandPipeline()
	{
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

		var commands = commandSystem.GetCommands();
		var commandTypes = commands.Select(command => command.GetType().Name).ToList();

		return new AgentBehaviourCommandPipelineResult(
			CommandCount: commands.Count,
			CommandTypes: commandTypes);
	}
}

public sealed record AgentBehaviourChaseResult(
	bool SawChase,
	bool SawAttack,
	string FinalActiveBehaviour,
	float FinalDistance,
	int FinalIntentCount);

public sealed record AgentBehaviourIdleResult(string ActiveBehaviour);

public sealed record AgentBehaviourCommandPipelineResult(
	int CommandCount,
	IReadOnlyList<string> CommandTypes);
