using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public BaseEntity owner;
    [SerializeField] List<ItemData> items = new List<ItemData>();

    public bool AddItem(ItemData item)
    {
        if (item is StatItemData statItem)
        {
            foreach (var mod in statItem.modifiers)
            {
                if (!owner.Stats.AddModifier(mod.statName, mod.flat, mod.percent))
                    return false;
            }
        }
        else if (item is AbilityItemData abilityItem)
        {
            if (abilityItem.ability != null)
            {
                owner.SetSpecialAbility(abilityItem.ability);
            }
        }

        items.Add(item);
        return true;
    }

    public void RemoveItem(ItemData item)
    {
        if (!items.Remove(item)) return;

        if (item is StatItemData statItem)
        {
            foreach (var mod in statItem.modifiers)
            {
                owner.Stats.RemoveModifier(mod.statName, mod.flat, mod.percent);
            }
        }
        else owner.SetSpecialAbility(null);

    }
    public void Clear()
    {
        items.Clear();
        owner.Stats.ResetModifiers();
    }
}