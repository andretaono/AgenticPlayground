System: Inventory
Summary: Simple item storage and lookup used by player and shops.

Responsibilities:
- Store IItem instances up to Capacity
- Raise ItemAdded/ItemRemoved events
- Provide GetItem / ListItems read API

Public contracts (interfaces / events):
- IInventory
  - int Capacity { get; }
  - IReadOnlyList<IItem> Items { get; }
  - bool AddItem(IItem item);
  - bool RemoveItem(string itemId);
  - IItem? GetItem(string itemId);
  - event EventHandler<ItemEventArgs> ItemAdded;
  - event EventHandler<ItemEventArgs> ItemRemoved;

Core data:
- IItem: Id, Name, Description, ItemType, Value

Scenarios / acceptance criteria:
- Add when below capacity -> returns true and fires ItemAdded
- Add when at capacity -> returns false
- Remove existing item -> returns true and fires ItemRemoved
- Remove nonexistent -> returns false

Constraints:
- Core logic must be plain C# (no UnityEngine)
- Unity glue only in Systems/Inventory/Unity guarded by #if UNITY

Example usage:
- short pseudo-code showing AddItem/GetItem