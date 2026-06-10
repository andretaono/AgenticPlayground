# AgenticPlayground

## Purpose
AgenticPlayground is an experimental sandbox for exploring AI-assisted game system architecture and development workflows. The goal is to investigate how agentic AI can reduce the complexity cost of ambitious game development projects while maintaining clean, scalable system design.

## Architectural Decisions
Several constraints were intentionally enforced throughout development:

- Strict separation of concerns
- No game-engine dependencies outside Presentation
- No event-driven architecture
- No static state
- Deterministic simulation
- Composition over inheritance
- Explicit contracts through interfaces

The goal was to maximize testability, maintainability and portability while minimizing coupling between gameplay systems.

## Architecture
The project is structured into five layers:

Presentation  
↓  
Runtime  
↓  
Integration  
↓  
Domain  
↓  
Foundation  

Dependencies flow downward only.

### Foundation
Contains reusable primitives and abstractions with no game-specific concepts.

### Domain
Contains pure simulation systems implementing game rules and state.

Each Domain system:
- Is self-contained
- Owns its own state and logic
- Exposes functionality through ports (interfaces)
- Has no dependencies on other Domain systems

### Integration
Connects Domain systems through adapters and composes gameplay features.
Cross-system communication occurs exclusively through adapters.

### Runtime
Provides orchestration and deterministic tick execution.

### Presentation
Unity-specific rendering, input and scene management.
The Presentation layer never contains gameplay logic.

## Development Process
- The project was developed iteratively using AI-assisted code generation.
- The process began by creating small, isolated Domain systems with strict architectural constraints.
- System correctness was validated through unit tests focused on business logic.
- As the number of systems grew, integration tests were introduced to verify cross-system behaviour through adapters.
- The first integration tests acted as "walking skeletons" and required manual verification. Over time these were replaced by fully automated integration tests.
- Only after a playable simulation existed entirely in the Foundation, Domain and Integration layers was a Unity Presentation layer introduced.
- Because gameplay logic was already isolated from engine concerns, integrating Unity required only a thin presentation layer and a runtime tick bridge.

## Conclusion
This project demonstrated that agentic AI can be used to develop complex game projects at scale when guided by a strict architecture and supported by comprehensive testing. The combination of isolated Domain systems, automated tests, and engine-agnostic gameplay logic made Unity integration straightforward and reduced the risk of code entanglement as the project evolved.
