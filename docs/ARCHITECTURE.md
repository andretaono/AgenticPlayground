# Architecture Rules

You are working on a modular, engine-agnostic C# ARPG simulation project.

---

## Core Rules

- Strict layering: Foundation → Domain → Integration
- Hexagonal architecture (ports & adapters)
- No engine dependencies (no Unity, Unreal, Godot)
- No engine glue code (no MonoBehaviour, no engine folders, no #if UNITY)
- No static classes, functions, or variables
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

---

## Integration Layer

Location: `/Systems/Integration/Adapters`

### Responsibility

- Implements Domain ports (adapters)
- Runs runtime simulation via ticks
- Runs Scenario execution

### Rules

- Adapters only
- No domain logic
- No state ownership
- No simulation rules or decisions
- Only orchestration and wiring

---

## Runtime Model

- Tick-based deterministic simulation
- ScenarioRunner executes isolated scenarios
- Systems are explicitly orchestrated only via Runtime

---

## Output Requirements

When generating a Domain system:

- `{SystemName}System.cs`
- `/Model`
- `/Controller`
- `/Ports`
- One console scenario under `/Scenarios`
- Register scenario in `ScenarioRunner.cs`

---

## Hard Constraints

- No statics
- No events
- No engine dependencies
- No cross-layer violations
- No logic inside Integration layer
- No orchestration inside Controllers
- No domain logic outside Domain layer