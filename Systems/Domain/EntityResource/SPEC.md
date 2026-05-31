# EntityResource

## Summary
Manages resources associated with entities, such as Health, Mana, Stamina, Energy, Hunger, etc.

## Notes
- Resources are attached to entities
- Supports passive regeneration and depletion through simulation ticks
- Supports multiple resource types per entity
- Does not communicate via events
- Resource instances hold runtime state (`CurrentAmount`); registry indexes `(EntityId, ResourceType) → IResourceDefinition`
- Full and depleted states are derived from current values

## Ports
- `IEntityResourceSystem` — exposes `Registry`, `Simulation`
- `IEntityResourceRegistry` — `AddResource`, `RemoveResource`, `HasResource`, `TryGetDefinition<T>`
- `IResourceDefinition` — registration contract with runtime mutations (`Increase`, `Decrease`, `Set`, `GetSnapshot`, `IsDepleted`, `IsFull`); base implementation `ResourceDefinitionBase`, default concrete `ResourceDefinition`
- Marker definition types — `IHealthResourceDefinition`, `IStaminaResourceDefinition`, `IManaResourceDefinition`, etc.
- `IEntityResourceSimulation` — `AdvanceSimulation`

Integration-layer resource types (`HealthResource`, `StaminaResource`, `ManaResource`) implement `IResourceDefinition` per agent.

## Invariants
- `CurrentAmount >= 0`
- `CurrentAmount <= MaximumAmount`
- `MaximumAmount > 0`
- An entity can have no more than one resource of the same type

## Folder structure
- `EntityResourceSystem.cs`
- `Ports/`
- `Controller/`
- `Model/`
