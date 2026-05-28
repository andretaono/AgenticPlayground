You are working on a modular, engine-agnostic C# project for an ARPG.

Architecture rules:
- Systems must be pure C# with no game-engine dependencies (no `UnityEngine`, Unreal, Godot, etc.)
- Avoid event-driven architecture unless explicitly justified
- Systems should be deterministic, testable, and self-contained
- Each system must live in its own folder under /Systems/{SystemName}
- Systems are split into three types, depending on their responsibilities; foundation, domain, and orchestration
- Use hexagonal architecture (ports and adapters)
- Use adapters in integration layer to translate between system contracts
- Runtime systems define execution
- Composition root defines relationships
- All systems may reference Foundation primitives/contracts under `Systems/Foundation/**`
- Systems should be in separate namespaces
- Use Model View Controller folder structure for each system
- Follow SOLID principles

Code style:
- Prefer composition over inheritance
- Keep classes small and single-purpose
- Avoid anything static

Output expectations:
- When creating a system, generate:
  - One class named {SystemName}System, which is the system entry point and has dependencies injected via its constructor
  - Controller layer
  - Model layer
  - Example usage implementation to be run from console
  - Unit tests that fully cover controller logic