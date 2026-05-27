You are working on a modular, engine-agnostic C# project for an ARPG.

Architecture rules:
- Systems must be pure C# with no game-engine dependencies (no `UnityEngine`, Unreal, Godot, etc.)
- Do not add engine glue code (no `MonoBehaviour`, no `#if UNITY` adapters, no `/Unity` folders) unless explicitly requested
- Engine integration (rendering, input, scene objects) lives outside this repository for now
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
  - Core logic layer
  - Interface layer
  - Example usage implementation to be run from console
  - Unit tests that fully cover core logic layer
- Do not generate engine glue folders or adapter code unless explicitly requested