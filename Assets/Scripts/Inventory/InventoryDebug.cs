using System;
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

        Debug.Log("=== Current Stats ===");

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            float value = stats.Get(type);
            float baseValue = stats.GetBase(type);

            if (Math.Abs(value - baseValue) > 0.001f)
            {
                // Показываем только изменённые статы
                Debug.Log($"{type}: {baseValue:F1} → {value:F1}");
            }
            else if (baseValue > 0)
            {
                Debug.Log($"{type}: {value:F1}");
            }
        }
    }

    [ContextMenu("Print Quick Stats")]
    void PrintQuickStats()
    {
        if (inventory?.owner == null) return;

        var stats = inventory.owner.Stats;
        Debug.Log($"Speed: {stats[StatType.Speed]:F1} | ATK: {stats[StatType.AttackPower]:F1} | DEF: {stats[StatType.Defense]:F1} | HP: {stats[StatType.MaxHealth]:F0}");
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