# Foundation: GameMath

## Summary
Provide commonly used math types and operations through engine-agnostic abstractions that all gameplay systems may depend on.

## Responsibilities
- Expose `IVector3` for position, velocity, and direction data
- Provide vector math via `GameMathSystem` (or any `IGameMath` implementation)
- Validate finiteness for downstream invariants (e.g. movement)

## Public API
- `IVector3` — `X`, `Y`, `Z`
- `IGameMath`
  - `Create(x, y, z)`
  - `Add`, `Subtract`, `Scale`
  - `Dot`, `Magnitude`, `MagnitudeSquared`, `Distance`, `Normalize`
  - `IsFinite`
- `GameMathSystem` (default implementation)
  - `Zero`

## Constraints
- Plain C# only; no game-engine references

## Acceptance criteria
- Vectors are immutable value types implementing `IVector3`
- `Normalize` on zero vector returns `Zero` (no exception)
- `IsFinite` returns false for NaN or Infinity components

