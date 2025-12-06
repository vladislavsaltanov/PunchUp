using UnityEngine;

public class InventoryDebug : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] ItemData testItem;

    [ContextMenu("Add Test Item")]
    void AddTestItem()
    {
        if (!Validate()) return;

        inventory.AddItem(testItem);
        Debug.Log($"[Debug] Added '{testItem.itemName}'");
        PrintStats();
    }

    [ContextMenu("Remove Test Item")]
    void RemoveTestItem()
    {
        if (!Validate()) return;

        inventory.RemoveItem(testItem);
        Debug.Log($"[Debug] Removed '{testItem.itemName}'");
        PrintStats();
    }

    [ContextMenu("Print Stats")]
    void PrintStats()
    {
        if (inventory?.owner == null) return;

        var stats = inventory.owner.Stats;
        Debug.Log($"Speed: {stats["speed"]}");
        Debug.Log($"Attack Power: {stats["attackPower"]}");
        Debug.Log($"Defense: {stats["defense"]}");
        Debug.Log($"Max Health: {stats["maxHealth"]}");
    }

    [ContextMenu("Clear Inventory")]
    void ClearInventory()
    {
        if (inventory == null) return;

        inventory.Clear();
        Debug.Log("[Debug] Inventory cleared");
        PrintStats();
    }

    bool Validate()
    {
        if (inventory == null)
        {
            Debug.LogError("[Debug] Inventory not assigned");
            return false;
        }

        if (testItem == null)
        {
            Debug.LogError("[Debug] Test Item not assigned");
            return false;
        }

        return true;
    }
}