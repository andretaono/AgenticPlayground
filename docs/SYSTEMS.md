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

##Runtime
Summary: Engine-agnostic runtime orchestrator that ticks registered systems each frame.

API:
- Register(ITickable)
- Unregister(ITickable)
- Tick(float deltaTime)

##CharacterMovement
Summary: ARPG-style top-down movement used for agent movement and interaction with the world.

The actual input system is abstracted away, so this system focuses on movement logic and state management.

Use movement states to determine behavior:
- Grounded
- Swimming
- Airborne

Variables:
- Position (Vector3)
- Velocity (Vector3)
- MovementState (enum)

Invariants:
- Position cannot contain NaN or Infinity values.

API:
- GetMovementState(): returns current movement state
- ApplyMovement(Vector3 input): applies movement input based on current state
- Tick(float deltaTime): updates position and velocity based on current state and input
- GetPosition(): returns current position