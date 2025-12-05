using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public BaseEntity owner;
    [SerializeField] List<ItemSO> items = new List<ItemSO>();

    public bool AddItem(ItemSO item)
    {
        foreach (var mod in item.modifiers)
        {
            if (!owner.Stats.AddModifier(mod.statName, mod.flat, mod.percent))
            {
                Debug.LogWarning($"[Inventory] Item '{item.itemName}' ignored");
                return false;
            }
        }

        items.Add(item);
        return true;
    }

    public void RemoveItem(ItemSO item)
    {
        if (!items.Remove(item)) return;

        foreach (var mod in item.modifiers)
            owner.Stats.RemoveModifier(mod.statName, mod.flat, mod.percent);
    }
    public void Clear()
    {
        items.Clear();
        owner.Stats.ResetModifiers();
    }
}