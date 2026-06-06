# SPEC.md - World Cognition System V1

## Purpose

The World Cognition System (WCS) maintains a persistent ecological memory of player actions.

It does not control enemies directly.

It provides a shared world state that other systems can query.

The system should answer:

* Where has the player been?
* How much has the player disturbed the environment?
* What kind of creature is the player becoming?
* How aware is the ecosystem of the player?

---

# Design Principles

## 1. Data First

The WCS stores information.

It does not make gameplay decisions.

Other systems consume WCS outputs and decide what to do.

---

## 2. Decay Over Time

All information naturally fades.

The world remembers, but eventually forgets.

---

## 3. Spatial

All cognition data is stored geographically.

A disturbance in one valley should not affect a distant coastline.

---

## 4. Continuous

The system updates continuously as the player moves through the world.

---

# World Maps

The system maintains three spatial maps.

All maps use the same grid resolution.

Example:

```text
Cell Size = 32m x 32m
```

Actual size can be adjusted later.

---

# Presence Map

Represents evidence that the player exists.

Examples:

* Footprints
* Recent movement
* Camps
* Resting locations
* Temporary scent trails

Value Range:

```text
0.0 - 100.0
```

Meaning:

```text
0 = No evidence
100 = Extremely recent evidence
```

Generation:

| Event        | Value Added |
| ------------ | ----------- |
| Movement     | +0.25       |
| Sprinting    | +0.5        |
| Resting      | +5          |
| Campfire     | +15         |
| Shape Change | +10         |

Decay:

```text
Presence *= 0.995 per second
```

(Tunable)

---

# Disturbance Map

Represents ecological disruption.

Examples:

* Combat
* Kills
* Environmental destruction
* Loud abilities
* Large transformations

Value Range:

```text
0.0 - 100.0
```

Meaning:

```text
0 = Calm
100 = Extreme disturbance
```

Generation:

| Event                | Value Added |
| -------------------- | ----------- |
| Combat Hit           | +1          |
| Enemy Kill           | +10         |
| Elite Kill           | +20         |
| Boss Kill            | +50         |
| Terrain Destruction  | +5          |
| Major Transformation | +15         |

Decay:

```text
Disturbance *= 0.999 per second
```

Decay is intentionally slower than Presence.

---

# Affinity Map

Represents what the ecosystem believes the player is becoming.

Unlike Presence and Disturbance, Affinity contains multiple channels.

Example channels:

```text
Bear
Raven
Seal
```

Future channels may include:

```text
Fox
Whale
Eagle
Spirit
```

Each cell stores:

```typescript
AffinityCell {
    bear: float
    raven: float
    seal: float
}
```

Range:

```text
0.0 - 100.0
```

Generation:

### Bear

Added when:

* Melee combat
* Aggressive actions
* Territory holding
* Predatory behavior

Examples:

```text
Melee Kill +5 Bear
Heavy Attack +1 Bear
```

### Raven

Added when:

* Scouting
* High movement
* Observation
* Flight

Examples:

```text
Flight +2 Raven
Discovery +5 Raven
```

### Seal

Added when:

* Swimming
* Underwater travel
* Coastal movement
* Escape behavior

Examples:

```text
Swim +1 Seal
Underwater Traversal +5 Seal
```

Decay:

```text
Affinity *= 0.9995 per second
```

Very slow.

The world should remember affinity longer than actions.

---

# Derived Outputs

Derived outputs are recalculated periodically.

Recommended:

```text
Every 1 second
```

---

# Awareness

Represents how aware the ecosystem is of the player.

Formula:

```text
Awareness =
AveragePresence * 0.7
+
AverageDisturbance * 0.3
```

Result:

```text
0 - 100
```

State Mapping:

| Value  | State     |
| ------ | --------- |
| 0-20   | Unnoticed |
| 20-40  | Noticed   |
| 40-60  | Observed  |
| 60-80  | Tracked   |
| 80-100 | Hunted    |

Output:

```typescript
enum AwarenessState {
    Unnoticed,
    Noticed,
    Observed,
    Tracked,
    Hunted
}
```

---

# Regional Mood

Represents the ecological stability of a region.

Formula:

```text
RegionalMood = AverageDisturbance
```

State Mapping:

| Value  | State     |
| ------ | --------- |
| 0-20   | Quiet     |
| 20-40  | Restless  |
| 40-60  | Disturbed |
| 60-80  | Hostile   |
| 80-100 | Violent   |

Output:

```typescript
enum RegionalMood {
    Quiet,
    Restless,
    Disturbed,
    Hostile,
    Violent
}
```

---

# Ecological Interest

Represents which ecological force is most interested in the player.

Calculated from nearby affinity totals.

Example:

```typescript
EcologicalInterest {
    bear: float
    raven: float
    seal: float
}
```

Normalization:

```text
Bear = 45%
Raven = 35%
Seal = 20%
```

Highest value determines dominant interest.

Example:

```text
Dominant Interest = Bear
```

Used by future AI systems.

---

# Public API

## Add Presence

```typescript
addPresence(
    position: Vector2,
    amount: float
)
```

---

## Add Disturbance

```typescript
addDisturbance(
    position: Vector2,
    amount: float
)
```

---

## Add Affinity

```typescript
addAffinity(
    position: Vector2,
    affinityType: AffinityType,
    amount: float
)
```

---

## Query Cell

```typescript
getCell(position)
```

Returns:

```typescript
WorldCell {
    presence: float
    disturbance: float
    bearAffinity: float
    ravenAffinity: float
    sealAffinity: float
}
```

---

## Query Awareness

```typescript
getAwareness(position)
```

Returns:

```typescript
AwarenessState
```

---

## Query Regional Mood

```typescript
getRegionalMood(position)
```

Returns:

```typescript
RegionalMood
```

---

## Query Ecological Interest

```typescript
getEcologicalInterest(position)
```

Returns:

```typescript
EcologicalInterest
```

---

# Non-Goals (V1)

The WCS does NOT:

* Spawn enemies
* Control AI
* Generate quests
* Control weather
* Perform pathfinding
* Track factions
* Predict player behavior

These systems may consume WCS data later.

The sole purpose of V1 is to establish persistent ecological memory.

---

# Implementation

## Ports
- `IWorldCognitionSystem` — exposes `Cognition`, `Simulation`
- `IWorldCognitionController` — `AddPresence`, `AddDisturbance`, `AddAffinity`, `GetCell`, `GetAwareness`, `GetRegionalMood`, `GetEcologicalInterest`
- `IWorldCognitionSimulation` — `AdvanceSimulation` (map decay)

## Folder structure
- `WorldCognitionSystem.cs`
- `Ports/`
- `Controller/`
- `Model/`
