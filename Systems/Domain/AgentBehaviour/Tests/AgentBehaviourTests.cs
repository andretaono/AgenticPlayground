using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Tests.Fakes;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Foundation.Testing;

namespace Game.Systems.Domain.AgentBehaviour.Tests;

public sealed class AgentBehaviourTests : ITestSuite
{
	public string Name => "unit/agent-behaviour";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "selects highest-priority executable behaviour", SelectsHighestPriority);
		registry.Add(Name, "skips behaviours that cannot execute", SkipsNonExecutable);
		registry.Add(Name, "falls back to idle when none can execute", FallsBackToIdle);
		registry.Add(Name, "priority override beats default behaviour priority", PriorityOverrideWins);
	}

	private static void SelectsHighestPriority()
	{
		var contextProvider = new FixedContextProvider();
		var idle = new StubBehaviour(new BehaviourId("idle"), priority: 0, canExecute: _ => false);
		var system = new AgentBehaviourSystem(contextProvider, idle);

		var agentId = new AgentId(1);
		var context = new BehaviourContext { Agent = agentId, Position = Vector2.Zero };
		contextProvider.Set(agentId, context);

		var low = new StubBehaviour(new BehaviourId("low"), priority: 1);
		var high = new StubBehaviour(new BehaviourId("high"), priority: 10);

		system.Behaviour.AddBehaviour(agentId, low);
		system.Behaviour.AddBehaviour(agentId, high);
		system.Simulation.Tick(0.1f);

		TestAssert.Equal(high.Id, system.Output.GetActiveBehaviour(agentId)!.Id);
	}

	private static void SkipsNonExecutable()
	{
		var contextProvider = new FixedContextProvider();
		var idle = new StubBehaviour(new BehaviourId("idle"), priority: 0, canExecute: _ => false);
		var system = new AgentBehaviourSystem(contextProvider, idle);

		var agentId = new AgentId(2);
		var context = new BehaviourContext { Agent = agentId, Position = Vector2.Zero };
		contextProvider.Set(agentId, context);

		var blocked = new StubBehaviour(new BehaviourId("blocked"), priority: 100, canExecute: _ => false);
		var fallback = new StubBehaviour(new BehaviourId("fallback"), priority: 1);

		system.Behaviour.AddBehaviour(agentId, blocked);
		system.Behaviour.AddBehaviour(agentId, fallback);
		system.Simulation.Tick(0.1f);

		TestAssert.Equal(fallback.Id, system.Output.GetActiveBehaviour(agentId)!.Id);
	}

	private static void FallsBackToIdle()
	{
		var contextProvider = new FixedContextProvider();
		var idle = new StubBehaviour(new BehaviourId("idle"), priority: 0);
		var system = new AgentBehaviourSystem(contextProvider, idle);

		var agentId = new AgentId(3);
		var context = new BehaviourContext { Agent = agentId, Position = Vector2.Zero };
		contextProvider.Set(agentId, context);

		var blocked = new StubBehaviour(new BehaviourId("blocked"), priority: 50, canExecute: _ => false);
		system.Behaviour.AddBehaviour(agentId, blocked);
		system.Simulation.Tick(0.1f);

		TestAssert.Equal(idle.Id, system.Output.GetActiveBehaviour(agentId)!.Id);
	}

	private static void PriorityOverrideWins()
	{
		var contextProvider = new FixedContextProvider();
		var idle = new StubBehaviour(new BehaviourId("idle"), priority: 0, canExecute: _ => false);
		var system = new AgentBehaviourSystem(contextProvider, idle);

		var agentId = new AgentId(4);
		var context = new BehaviourContext { Agent = agentId, Position = Vector2.Zero };
		contextProvider.Set(agentId, context);

		var defaultHigh = new StubBehaviour(new BehaviourId("default-high"), priority: 20);
		var overriddenLow = new StubBehaviour(new BehaviourId("overridden-low"), priority: 1);

		system.Behaviour.AddBehaviour(agentId, defaultHigh);
		system.Behaviour.AddBehaviour(agentId, overriddenLow);
		system.Behaviour.SetBehaviourPriority(agentId, overriddenLow, priority: 100);
		system.Simulation.Tick(0.1f);

		TestAssert.Equal(overriddenLow.Id, system.Output.GetActiveBehaviour(agentId)!.Id);
	}
}
