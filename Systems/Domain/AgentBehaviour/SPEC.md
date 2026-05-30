## AgentBehaviourSystem

### Summary

Evaluates and selects agent behaviours to produce AgentCommands for simulation.

Supports both player-controlled and AI-controlled agents through a unified behaviour model.

Each agent may have multiple behaviours, but only one behaviour is active per tick based on a deterministic selection rule.

---

### Notes

- Behaviour is a decision unit, not a single rule
- Behaviours represent coherent strategies (e.g. FollowTarget, Attack, Idle)
- Player input is treated as a behaviour provider (not a special case)
- All behaviours operate within the same AgentCommand pipeline
- No events are used; all outputs are explicit command generation
- Behaviour selection is deterministic and tick-based
- Behaviours must not directly modify other systems

---

### Variables

#### AgentId
The entity this behaviour system is evaluating.

#### Behaviours
List of IBehaviour instances assigned to the agent.

#### ActiveBehaviour
The behaviour selected for execution during the current tick.

#### BehaviourPriority
Numeric value used to resolve conflicts between behaviours.

#### BehaviourContext
Read-only snapshot of relevant simulation state used for decision making.

Examples:
- Agent position
- Nearby entities
- Current target
- Resource state

---

### Invariants

- Each agent has zero or more behaviours assigned
- Each agent can have no more than one of the same behaviour
- At most one behaviour may be active per tick
- Behaviour selection must be deterministic
- Behaviours must not directly mutate world state
- Behaviours must only produce AgentCommands
- Behaviour selection must not depend on frame timing or external randomness (unless seeded via Foundation RNG)
- If no behaviour is valid, agent defaults to IdleBehaviour (no-op)

---

### API

#### BehaviourController

- AddBehaviour(AgentId, IBehaviour behaviour)
- RemoveBehaviour(AgentId, IBehaviour behaviour)
- SetBehaviourPriority(AgentId, IBehaviour behaviour, int priority)
- ClearBehaviours(AgentId)

---

#### SimulationController

- Tick(float deltaTime)

Executes:
1. Evaluate all behaviours for agent
2. Select highest priority valid behaviour
3. Execute selected behaviour
4. Emit resulting AgentCommands

---

#### Behaviour Interface

```csharp
public interface IBehaviour
{
    int Priority { get; }

    bool CanExecute(BehaviourContext context);

    IBehaviourIntent[] Execute(BehaviourContext context);
}
```

---

### Folder structure

- `AgentBehaviourSystem.cs`
- `Ports/` — `IBehaviour`, `IBehaviourController`, `IAgentBehaviourSimulation`, `IAgentBehaviourOutput`, `IBehaviourContextProvider`, intents
- `Controller/` — registry, simulation, output
- `Model/` — `BehaviourContext`, state store, reference behaviours (`ChaseBehaviour`, `AttackBehaviour`)

### Integration

- `AgentBehaviourSimulationAdapter` — runtime tick bridge
- `BehaviourIntentToCommandAdapter` — converts intents to `AgentCommand` submissions