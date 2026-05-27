System: CharacterMovement
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
