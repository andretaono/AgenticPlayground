# Architecture Rules

You are working on a modular, engine-agnostic C# ARPG simulation project.

---

## Core Rules

- Strict layering: Foundation → Domain → Integration
- Hexagonal architecture (ports & adapters)
- No engine dependencies (no Unity, Unreal, Godot)
- No engine glue code (no MonoBehaviour, no engine folders, no #if UNITY)
- No static classes, functions, or variables (composition roots and dev harnesses may use static entry points until replaced)
- No event-based architecture
- Fully deterministic and testable systems
- All communication via interfaces (ports)
- Composition over inheritance

---

## Foundation Layer

Location: `/Systems/Foundation/{SystemName}`

- Primitives and engine-agnostic utilities only
- No game/domain concepts
- May be referenced by Domain

---

## Domain Layer

Location: `/Systems/Domain/{SystemName}`

### Responsibility

- Owns state, logic, and contracts (ports)
- Fully self-contained simulation systems
- May only depend on Foundation

### Required Structure

Each system MUST include:
- `{SystemName}System.cs` (root orchestrator)
- `/Model` (state, structs, enums)
- `/Controller` (logic units)
- `/Ports` (interfaces)

### Rules

- Root System is the ONLY entry point
- Root System orchestrates Controllers
- Controllers contain single-responsibility logic only
- Controllers must NOT orchestrate other controllers
- Domain defines all interfaces (ports)
- No cross-system coupling inside Domain

### TerrainMesh (separate from World)

`Systems/Domain/TerrainMesh/` owns procedural heightmaps and engine-agnostic mesh data. The World tile system is unchanged; composing World data into terrain meshes is an Integration concern (later phase). See [`Systems/Domain/TerrainMesh/SPEC.md`](Systems/Domain/TerrainMesh/SPEC.md).

---

## Integration Layer

Location: `/Systems/Integration/`

Integration has two documented sub-roles:

### Adapters (`Integration/Adapters`, `Integration/Runtime`)

- Implements domain ports for cross-system wiring
- Tick adapters bridge domain simulations into `RuntimeSystem`
- Presentation adapters bridge input and render hooks
- No simulation state ownership
- No game-content decisions (which enemy attacks when, item modifiers, etc.)

### Game content (`Integration/Enemies`, `Integration/Items`, `Integration/Combat`, `Integration/Behaviours`, `Integration/Resources`)

- Implements domain ports with game-specific rules (`IBehaviour`, combat effects, resource definitions)
- Holds configuration and profiles (enemy tactics, advantage thresholds)
- Does **not** own tick-scheduled simulation state — state lives in domain systems
- May compose domain systems via factories and assemblers at the composition boundary

### What goes where

| Concern | Layer | Example |
|---------|-------|---------|
| Behaviour selection contract | Domain port | `IBehaviour`, `BehaviourContext` |
| Patrol / stalk AI implementation | Integration game content | `PatrolBehaviour`, `PolarBearConfig` |
| Combat damage resolution | Domain | `AgentCombatSystem` |
| Melee ability wiring | Integration game content | `MeleeAttackAbilityFactory` |
| Tick ordering | Integration runtime | `GameRuntimeBuilder`, `RuntimeSystem` |
| WCS decay rules | Domain | `WorldCognitionSimulationController` |
| Actor ID registration | Integration composition | `ActorRegistry` |
| Keyboard → move command | Integration presentation | `IInputSource`, `InputToCommandAdapter` |

### Rules

- Integration must not own domain simulation state
- Game content implements ports; domain systems execute simulation
- Orchestration and wiring live in runtime builders and adapters

---

## Actor identity

See [`docs/actors.md`](actors.md) for the `AgentId` / `EntityId` model and `ActorRegistry` usage.

---

## Runtime Model

- Tick-based deterministic simulation
- `GameRuntimeBuilder` composes domain systems and standard tick adapters
- `TestRunner` console project is the sole entry point: `dotnet run --project TestRunner` (exit code 1 on failure)
- Main `Game` project is a library referenced by `TestRunner`

### Tests

| Layer | Location | What they exercise |
|-------|----------|-------------------|
| **Domain unit** | `Systems/Domain/{System}/Tests/` | Single `XxxSystem`, public ports, domain fakes — no Integration |
| **Integration** | `Tests/Integration/` | Multi-system harnesses in `Tests/Integration/Runners/` |

Filter examples: `dotnet run --project TestRunner -- unit` (domain unit suites), `dotnet run --project TestRunner -- polar-bear`, `dotnet run --project TestRunner -- item-assembly`.

Domain unit tests may use `Tests/Fakes/` under the same system folder. Shared domain fakes belong in `Systems/Domain/Common/Tests/Fakes/` if needed.

---

## Output Requirements

When generating a Domain system:

- `{SystemName}System.cs`
- `/Model`
- `/Controller`
- `/Ports`
- `/Tests` (optional) — `ITestSuite` for domain unit tests; register in `Tests/Core/UnitTestRunner.cs`
- Integration coverage (optional) — `ITestSuite` in `Tests/Integration/` + `*IntegrationRunner` in `Tests/Integration/Runners/`; register in `UnitTestRunner`

---

## Hard Constraints

- No statics (except dev entry points being phased out)
- No events
- No engine dependencies
- No cross-layer violations
- No simulation state ownership inside Integration
- No orchestration inside Controllers
- No domain logic outside Domain layer

