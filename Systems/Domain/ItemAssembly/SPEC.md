## ItemAssemblySystem

### Summary

Core itemization system that enables gameplay configuration through socketed assemblies of items containing modifiers.

This system defines how items are created, placed into assemblies, and resolved into a deterministic set of gameplay modifiers consumed by all simulation systems.

It is the foundation for all build expression in the game.

---

### Notes

- Items are immutable containers of Modifiers
- Assemblies are socketed structures that hold Items
- Factories are pure construction utilities (no logic beyond instantiation)
- All gameplay interpretation happens in the Resolver
- System is deterministic and order-stable
- No gameplay systems (movement, combat, etc.) exist in this layer

---

### Variables

#### Modifier
Atomic rule contribution to gameplay.

Examples:
- +Damage
- +MovementSpeed
- +CanSwim
- +AttackRange

---

#### Item
Immutable container of Modifiers.

- Id
- List<Modifier>

---

#### Assembly
Socketed structure containing Items.

- SocketCount
- Sockets[] (Item or empty)

---

#### ResolvedModifierSet
Final aggregated result used by gameplay systems.

- FlatValues
- PercentValues
- Flags
- RawModifiers

---

### Invariants

- Items are immutable after creation
- Assemblies only store and organize Items, they do not interpret them
- Factories do not apply rules or resolve modifiers
- Resolver is the only system allowed to interpret Modifier behavior
- Same Assembly state must always produce identical resolved output
- Modifier resolution is deterministic (order-independent or explicitly ordered)
- No runtime or simulation logic exists in this system

---

### API

---

## AssemblyFactory

Responsible only for creating Assembly instances.

### Methods

- Create(int socketCount) → Assembly

### Responsibilities
- Allocate empty Assembly
- Initialize socket structure
- Define capacity only

### Restrictions
- Must NOT create or modify Items
- Must NOT resolve Modifiers
- Must NOT apply gameplay rules

---

## ItemFactory

Responsible only for creating Item instances.

### Methods

- Create(IEnumerable<Modifier> modifiers) → Item

### Responsibilities
- Instantiate Item
- Attach provided Modifiers
- Enforce immutability (recommended)

### Restrictions
- Must NOT interact with Assemblies
- Must NOT resolve or interpret Modifiers
- Must NOT apply gameplay rules or balancing logic

---

## AssemblyController

Manages mutation of Assembly state.

### Methods

- AddItem(Assembly assembly, int socketIndex, Item item)
- RemoveItem(Assembly assembly, int socketIndex)
- GetItem(Assembly assembly, int socketIndex)

### Responsibilities
- Manage socket occupancy
- Maintain structural integrity of Assembly

---

## ResolverController

Core computation engine of the system.

### Methods

- Resolve(Assembly assembly) → ResolvedModifierSet

### Responsibilities
1. Collect all Items from Assembly
2. Extract all Modifiers
3. Group Modifiers by type/category
4. Apply deterministic resolution rules
5. Output final resolved modifier state

### Restrictions
- Must NOT modify Assembly or Items
- Must NOT depend on gameplay systems
- Must remain pure and deterministic

---

### Resolution Rules

Modifiers are combined using category-based logic:

- Flat values → summed
- Percent values → accumulated or multiplied (defined per category)
- Boolean flags → OR aggregation
- Conflicts → resolved via explicit rule priority ordering

---

### Ports

#### IModifierSource
Exposes modifiers from an item.

#### IAssemblyStorage
Provides access to socketed item layout.

#### IModifierResolver
Defines how raw modifiers are transformed into final values.

---

### Design Intent

This system exists to:

- Provide a deterministic rule composition engine
- Decouple itemization from gameplay simulation
- Enable scalable build complexity through composition
- Support future systems such as:
  - shape/morph systems
  - ability construction systems
  - procedural build generation
  - AI-driven item creation

All higher-level systems must consume only the ResolvedModifierSet output.

---

### Folder structure
- `ItemAssemblySystem.cs`
- `Ports/`
- `Controller/`
- `Model/`