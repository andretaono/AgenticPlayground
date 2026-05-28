# AgentMovement

## Summary
ARPG-style top-down movement used for agent movement and interaction with the world.

The actual input system is abstracted away, so this system focuses on movement logic and state management.

## States
- Grounded
- Swimming
- Airborne

## Core data per agent
- Position (`IVector3`)
- Velocity (`IVector3`)
- Pending input (`IVector3`)
- MovementState (`AgentMovementState`)

## Invariants
- Position cannot contain NaN or Infinity values.

## Ports (Interfaces)
- `IAgentMovementSystem`
- exposes `Registry`, `Input`, and `Simulation` ports
- `IAgentMovementRegistryPort`
  - `CreateAgent(entityId, initialPosition)`
  - `RemoveAgent(entityId)`
- `IAgentMovementInputPort`
  - `GetMovementState(entityId)`
  - `SetMovementState(entityId, state)`
  - `ApplyMovement(entityId, input)`
  - `GetPosition(entityId)`
  - `GetVelocity(entityId)`
- `IAgentMovementSimulationPort`
  - `AdvanceSimulation(deltaTime)` (port)

## Dependencies
- Foundation.GameMath (`IVector3`, `IGameMath`)
- None (runtime integration via adapter in composition root)

## Folder structure
- `Interfaces/` (ports)
- `Model/` (state + configuration)
- `Controller/` (use-case implementation + simulation controller)
- `Example/` (console demo)

