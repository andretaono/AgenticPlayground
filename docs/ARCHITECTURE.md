You are working on a modular, engine-agnostic C# project for an ARPG video game.

Architecture rules:
- Strict separation between Foundation, Domain, and Integration layers
- Follow a hexagonal architecture approach, with systems exposing ports and adapters for communication
- Systems must be pure C# with no game-engine dependencies (no `UnityEngine`, Unreal, Godot, etc.)
- Do not add engine glue code (no `MonoBehaviour`, no `#if UNITY` adapters, no `/Unity` folders) unless explicitly requested
- Systems must expose explicit APIs
- Avoid event-driven architecture unless explicitly justified
- Prefer direct orchestration over publish/subscribe patterns
- Systems should be deterministic and testable
- Runtime coordination happens externally
- Each system must live in its own folder under /Systems/{SystemName}
- Gameplay systems must not reference other gameplay systems
- All systems may reference Foundation primitives/contracts under `Systems/Foundation/**`
- Communication must happen via clearly defined contracts using interfaces
- Systems should be in separate namespaces
- Use Model View Controller folder structure for each system
- Follow SOLID principles

Code style:
- Prefer composition over inheritance
- Keep classes small and single-purpose
- Avoid static global state unless explicitly requested

Output expectations:
- When creating a system, generate:
  - One class named {SystemName}System, which is the system entry point and has dependencies injected via its constructor
  - Model layer
  - Controller layer
  - Interface layer
  - Example usage implementation to be run from console, placed in the Scenarios folder, and registered in the Scenarios/ScenarioRunner.cs file