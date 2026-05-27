#if UNITY
using UnityEngine;
using Game.Inventory.Core.Controller;
using Game.Inventory.Interfaces;

namespace Game.Inventory.Unity;

/// <summary>
/// Unity glue layer (composition only). Guarded by #if UNITY so .NET builds are unaffected.
/// </summary>
[DisallowMultipleComponent]
public class InventoryBehaviour : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int capacity = 20;

    private InventoryController _inventory = null!;

    public InventoryController Inventory => _inventory;

    void Awake()
    {
        _inventory = new InventoryController(capacity);
        _inventory.ItemAdded += (_, e) => Debug.Log($"[Inventory] Item added: {e.Item.Name}");
        _inventory.ItemRemoved += (_, e) => Debug.Log($"[Inventory] Item removed: {e.Item.Name}");
    }

    public bool TryAdd(IItem item) => _inventory.AddItem(item);
    public bool TryRemove(string itemId) => _inventory.RemoveItem(itemId);
}
#endif