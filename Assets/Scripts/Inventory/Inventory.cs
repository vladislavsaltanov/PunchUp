using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public BaseEntity owner;
    [SerializeField] List<ItemData> items = new();

    Dictionary<ItemData, int> stackCounts = new();
    Dictionary<ItemData, List<(StatType type, StatModifier mod)>> activeModifiers = new();

    public bool AddItem(ItemData item)
    {
        if (item == null) return false;

        if (!stackCounts.ContainsKey(item))
            stackCounts[item] = 0;
        stackCounts[item]++;

        if (item is StatItemData statItem)
        {
            RemoveModifiersForItem(item);
            
            ApplyModifiersForItem(statItem, stackCounts[item]);
        }
        else if (item is AbilityItemData abilityItem)
        {
            if (abilityItem.ability != null)
                owner.SetSpecialAbility(abilityItem.ability);
        }

        items.Add(item);
        StatisticsHandler.Instance.statisticData.items_picked++;
        return true;
    }

    public bool RemoveItem(ItemData item)
    {
        if (!items.Remove(item)) 
            return false;

        if (!stackCounts.ContainsKey(item))
            return true;

        stackCounts[item]--;

        if (item is StatItemData statItem)
        {
            RemoveModifiersForItem(item);
            
            if (stackCounts[item] > 0)
            {
                ApplyModifiersForItem(statItem, stackCounts[item]);
            }
            else
            {
                stackCounts.Remove(item);
            }
        }
        else if (item is AbilityItemData && stackCounts[item] <= 0)
        {
            owner.SetSpecialAbility(null);
            stackCounts.Remove(item);
        }

        return true;
    }

    void ApplyModifiersForItem(StatItemData statItem, int stacks)
    {
        var appliedMods = new List<(StatType, StatModifier)>();

        foreach (var data in statItem.modifiers)
        {
            float calculatedValue = data.CalculateValue(stacks);

            var mod = owner.Stats.AddModifier(
                data.statType,
                flat: data.IsFlat ? calculatedValue : 0f,
                percent: data.IsPercent ? calculatedValue : 0f,
                source: statItem
            );

            appliedMods.Add((data.statType, mod));
        }

        activeModifiers[statItem] = appliedMods;
    }

    void RemoveModifiersForItem(ItemData item)
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

    public int GetStackCount(ItemData item)
    {
        return stackCounts.TryGetValue(item, out int count) ? count : 0;
    }

    public bool HasItem(ItemData item) => stackCounts.ContainsKey(item) && stackCounts[item] > 0;

    public void Clear()
    {
        owner.Stats.ClearModifiers();
        owner.SetSpecialAbility(null);
        
        items.Clear();
        stackCounts.Clear();
        activeModifiers.Clear();
    }

    public IReadOnlyList<ItemData> GetItems() => items;

    [ContextMenu("Print Quick Stats")]
    void PrintQuickStats()
    {
        var stats = owner.Stats;
        Debug.Log($"Speed: {stats[StatType.Speed]:F1} | ATK: {stats[StatType.AttackPower]:F1} | DEF: {stats[StatType.Defense]:F1} | HP: {stats[StatType.MaxHealth]:F0}");
    }
}