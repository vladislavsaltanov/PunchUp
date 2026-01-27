using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public BaseEntity owner;
    [SerializeField] List<ItemData> items = new();

    Dictionary<ItemData, List<(StatType type, StatModifier mod)>> activeModifiers = new();

    public bool AddItem(ItemData item)
    {
        if (item == null) return false;

        if (item is StatItemData statItem)
        {
            var appliedMods = new List<(StatType, StatModifier)>();

            foreach (var data in statItem.modifiers)
            {
                var mod = owner.Stats.AddModifier(
                    data.statType,
                    data.flat,
                    data.percent,
                    source: item
                );
                appliedMods.Add((data.statType, mod));
            }

            activeModifiers[item] = appliedMods;
        }
        else if (item is AbilityItemData abilityItem)
        {
            if (abilityItem.ability != null)
                owner.SetSpecialAbility(abilityItem.ability);
        }

        items.Add(item);
        return true;
    }

    public bool RemoveItem(ItemData item)
    {
        if (!items.Remove(item))
            return false;

        if (item is StatItemData)
        {
            if (activeModifiers.TryGetValue(item, out var mods))
            {
                foreach (var (type, mod) in mods)
                {
                    owner.Stats.RemoveModifier(type, mod);
                }
                activeModifiers.Remove(item);
            }
        }
        else if (item is AbilityItemData)
        {
            owner.SetSpecialAbility(null);
        }

        return true;
    }

    public bool HasItem(ItemData item) => items.Contains(item);

    public void Clear()
    {
        owner.Stats.ClearModifiers();
        owner.SetSpecialAbility(null);

        items.Clear();
        activeModifiers.Clear();
    }

    public IReadOnlyList<ItemData> GetItems() => items;
}