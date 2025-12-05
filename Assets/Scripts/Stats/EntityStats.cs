using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityStats
{
    [Header("Base Stats")]
    public float speed = 5f;
    public float attackPower = 1f;
    public float defense = 0f;
    public float maxHealth = 100f;
    public float knockbackMultiplier = 1f;

    // Runtime
    Dictionary<string, float> baseStats;
    Dictionary<string, float> flatMods = new Dictionary<string, float>();
    Dictionary<string, float> percentMods = new Dictionary<string, float>();
    Dictionary<string, float> cached = new Dictionary<string, float>();
    bool isDirty = true;
    bool initialized = false;

    public float this[string statName]
    {
        get
        {
            Initialize();

            if (isDirty)
                Recalculate();

            if (cached.TryGetValue(statName, out float val))
                return val;

            Debug.LogWarning($"[EntityStats] Unknown stat '{statName}'");
            return 0f;
        }
    }

    /// <summary>
    /// flat: +5 урона
    /// percent: +20 означает +20%
    /// </summary>
    public bool AddModifier(string statName, float flat = 0f, float percent = 0f)
    {
        Initialize();

        if (!baseStats.ContainsKey(statName))
        {
            Debug.LogWarning($"[EntityStats] Unknown stat '{statName}'");
            return false;
        }

        if (flat != 0f)
        {
            if (!flatMods.ContainsKey(statName))
                flatMods[statName] = 0f;
            flatMods[statName] += flat;
        }

        if (percent != 0f)
        {
            if (!percentMods.ContainsKey(statName))
                percentMods[statName] = 0f;
            percentMods[statName] += percent;
        }

        isDirty = true;
        return true;
    }

    public void RemoveModifier(string statName, float flat = 0f, float percent = 0f)
    {
        if (flat != 0f && flatMods.ContainsKey(statName))
        {
            flatMods[statName] -= flat;
            isDirty = true;
        }

        if (percent != 0f && percentMods.ContainsKey(statName))
        {
            percentMods[statName] -= percent;
            isDirty = true;
        }
    }

    public void ResetModifiers()
    {
        flatMods.Clear();
        percentMods.Clear();
        isDirty = true;
    }

    public bool HasStat(string statName)
    {
        Initialize();
        return baseStats.ContainsKey(statName);
    }

    void Initialize()
    {
        if (initialized) return;

        baseStats = new Dictionary<string, float>
        {
            ["speed"] = speed,
            ["attackPower"] = attackPower,
            ["defense"] = defense,
            ["maxHealth"] = maxHealth,
            ["knockbackMultiplier"] = knockbackMultiplier
        };

        initialized = true;
    }

    void Recalculate()
    {
        cached.Clear();

        foreach (var stat in baseStats)
        {
            float baseVal = stat.Value;
            float flat = flatMods.TryGetValue(stat.Key, out float f) ? f : 0f;
            float percent = percentMods.TryGetValue(stat.Key, out float p) ? p : 0f;

            // Формула: (base + flat) * (1 + percent / 100)
            cached[stat.Key] = (baseVal + flat) * (1f + percent / 100f);
        }

        isDirty = false;
    }
}