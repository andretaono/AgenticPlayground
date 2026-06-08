# Actor Identity

Agents in this project are referenced by two typed IDs that share one integer value:

| Type | Used by | Purpose |
|------|---------|---------|
| `AgentId` | AgentBehaviour, AgentCommand | Decision-making and command submission |
| `EntityId` | AgentMovement, AgentCombat, EntityResource | Physics, combat entities, resources |

## Rules

1. **One actor = one int, two typed views.** For controllable or AI-driven actors, `AgentId.Value == EntityId.Value`.
2. **Register via `ActorRegistry`.** Never construct `EntityId` from `AgentId` outside [`ActorRegistry`](../Systems/Integration/Actors/ActorRegistry.cs).
3. **`RegisterActor`** creates both IDs, registers command + movement.
4. **`RegisterEntity`** creates movement-only entities (passive targets, props) with no `AgentId`.

## Example

```csharp
var registry = new ActorRegistry(commandSystem, movement, math);
var player = registry.RegisterActor(math.Create(64f, 0f, 0f));
var bear = registry.RegisterActor(math.Create(220f, 0f, 0f));

// player.AgentId → behaviour / commands
// player.EntityId → movement / combat / health
```

## Future extensions

Mounts, possession, or multi-entity agents should extend `ActorRegistry` as the single mapping layer rather than scattering ID conversions across integration harnesses.
