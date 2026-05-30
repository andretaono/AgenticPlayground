## Foundation.GameMath

**Status:** Implemented  
**Path:** `Systems/Foundation/GameMath/`  
**Spec:** `Systems/Foundation/GameMath/SPEC.md`  
**Ticks:** No  
**Depends on:** None

### Summary
Provide commonly used math types and operations through engine-agnostic abstractions.

### Public API
- `IVector3` — `X`, `Y`, `Z`
- `IGameMath` — `Create`, `Add`, `Subtract`, `Scale`, `Dot`, `Magnitude`, `Distance`, `Normalize`, `IsFinite`
- `GameMathSystem` — default implementation of `IGameMath`

## Runtime
Summary: Engine-agnostic runtime orchestrator that ticks registered systems each frame.

API:
- ITickSchedule.Entries (ordered TickEntry list)
- Tick(float deltaTime)

##AgentMovement
Summary: ARPG-style top-down movement used for agent movement and interaction with the world.

The actual input system is abstracted away, so this system focuses on movement logic and state management.

Use movement states to determine behavior:
- Grounded
- Swimming
- Airborne

Variables:
- Position (IVector3)
- Velocity (IVector3)
- MovementState (enum)

Invariants:
- Position cannot contain NaN or Infinity values.

API:
- GetMovementState(): returns current movement state
- ApplyMovement(IVector3 input): applies movement input based on current state
- AdvanceSimulation(float deltaTime): advances simulation (port)
- GetPosition(): returns current position

## AgentCommand
### Summary
The AgentCommandSystem is responsible for receiving, buffering, and exposing agent-issued simulation commands in a deterministic and source-agnostic manner. The system acts as the boundary between command producers and simulation systems. The AgentCommandSystem does not interpret gameplay intent, mutate gameplay state, or depend on device-specific input implementations.

The system is designed to support:
- player input
- AI agents
- replay systems
- networking
- automated tests
- scripted sequences

through a unified command pipeline.

### Notes
- Commands represent intent, not completed actions.
- The system is agent-agnostic and device-agnostic.
- Commands are immutable once submitted.
- Simulation systems must not know command origin.
- Command submission and command execution are separate phases.
- Runtime orchestration controls when commands are consumed.
- The system is deterministic under identical command sequences.
- The system does not directly depend on Unity APIs.
- The system does not perform gameplay simulation.
- The system does not own entity state.

### Invariants
- Deterministic Processing
- Identical command sequences must produce identical simulation results.
- Immutable Commands
- Commands must not be modified after submission.
- Explicit Ownership
- Every command must reference a valid AgentId

### API
#### SubmitCommand
Submits a command for future simulation execution.
void SubmitCommand(IAgentCommand command);
#### ClearCommands
Clears the active command buffer after simulation execution completes.
void ClearCommands();
#### RegisterAgent
Registers a valid simulation agent capable of issuing commands.
void RegisterAgent(AgentId agentId);
#### UnregisterAgent
Removes an agent from the command system.
void UnregisterAgent(AgentId agentId);
#### HasCommands
Returns whether commands currently exist in the active buffer.
bool HasCommands();

### Initial command types
public readonly struct MoveCommand : IAgentCommand
{
    public AgentId Agent;
    public Vector2 Direction;
}
public readonly struct JumpCommand : IAgentCommand
{
    public AgentId Agent;
}
public readonly struct AttackCommand : IAgentCommand
{
    public AgentId Agent;
    public EntityId Target;
}

## World

### Summary
Provides a simulation-oriented representation of world space independent of rendering or engine-specific concerns.

### Notes
- Presentation must be handled through adapters.
- World data sources should be replaceable.
- Initial implementation should prioritize simplicity and readability.

### Variables

#### WorldMap
Represents world state.

#### WorldTile
Represents a single tile.

#### TileType
Semantic tile category.

Examples:
- Ground
- Wall
- Water

#### WorldPosition
Spatial location within the world.

### Invariants

- Presentation must not mutate world state.
- World queries must be deterministic.
- Tile access must be bounds-safe.

### API

#### IWorldSystem
Provides world queries and tile access.

#### IWorldDataSource
Loads or generates world data.

#### IWorldPresenter
Visualizes world state.

## EntityResource

### Summary
Manages resources associated with entities, such as Health, Mana, Stamina, Energy, Hunger, etc.

### Notes
- Resources are attached to entities
- Supports passive regeneration and depletion through simulation ticks
- Supports multiple resource types per entity
- Does not communicate via events
- Resource state is queried explicitly by other systems
- Full and depleted states are derived from current values

### Variables

#### ResourceId
Unique identifier for a resource type.

#### EntityId
Owner of the resource.

#### Name
Human-readable resource name.

#### CurrentAmount
Current resource value.

#### MaximumAmount
Maximum allowed value.

#### RegenerationRate
Amount restored per second.

#### DepletionRate
Amount consumed per second.

### Invariants

- CurrentAmount >= 0
- CurrentAmount <= MaximumAmount
- MaximumAmount > 0
- An Entity can have no more than one resource of the same type

### API

#### ResourceController

- IncreaseResource(EntityId, ResourceId, amount)
- DecreaseResource(EntityId, ResourceId, amount)
- SetResource(EntityId, ResourceId, amount)
- GetResource(EntityId, ResourceId)
- IsDepleted(EntityId, ResourceId)
- IsFull(EntityId, ResourceId)

#### SimulationController

- AdvanceSimulation(float deltaTime)

#### RegistryController

- AddResource(EntityId, ResourceDefinition)
- RemoveResource(EntityId, ResourceId)
- HasResource(EntityId, ResourceId)

## AgentCombat

### Summary
Ability-based combat framework. Entities carry ability triggers; when a trigger fires, the system executes targeting, conditions, modifiers, and effects in a deterministic pipeline.

### Notes
- Domain defines ports and execution framework only
- Concrete triggers, targeting rules, conditions, and effects live in `Systems/Integration/Combat/`
- Commands arm `PendingAttackTarget` on combat entities; simulation tick executes abilities then clears pending state
- Damage is applied via Integration `IEffect` adapters (e.g. `ResourceDamageEffect` → EntityResource)

### Ports
- `ICombatEntity` — entity identity, pending attack target, ability triggers
- `ICombatEntityRegistry` — register/lookup combat entities by `EntityId`
- `IAbilityExecutor` — executes a single ability
- `IAbilityTrigger`, `ITargetingRule`, `ICondition`, `IEffect`, `IEffectModifier` — ability pipeline contracts
- `IAgentCombatSimulation` — `Tick(float deltaTime)`

### Recommended tick order
1. AgentBehaviour simulation
2. Behaviour intent → command submission
3. AgentCommand execution (move + arm attacks)
4. AgentCombat simulation
5. AgentMovement simulation
6. EntityResource simulation

### Integration adapters
- `AgentCombatSimulationAdapter` — runtime tick bridge
- `AgentCommandExecutionAdapter` — processes `MoveCommand` and arms `AttackCommand`
- `LoggingAbilityExecutor` — wraps executor with console output
- `MeleeAttackAbilityFactory`, `PendingTargetTrigger`, `ExplicitTargetTargetingRule`, `ResourceDamageEffect` — concrete melee attack wiring