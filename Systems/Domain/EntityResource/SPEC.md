# EntityResource

## Summary
Manages resources associated with entities, such as Health, Mana, Stamina, Energy, Hunger, etc.

## Notes
- Resources are attached to entities
- Supports passive regeneration and depletion through simulation ticks
- Supports multiple resource types per entity
- Does not communicate via events
- Resource state is queried explicitly by other systems
- Full and depleted states are derived from current values

## Ports
- `IEntityResourceSystem` — exposes `Registry`, `Resource`, `Simulation`
- `IEntityResourceRegistry` — `AddResource`, `RemoveResource`, `HasResource`
- `IEntityResourceController` — `IncreaseResource`, `DecreaseResource`, `SetResource`, `GetResource`, `IsDepleted`, `IsFull`
- `IEntityResourceSimulation` — `AdvanceSimulation`

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
