using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityEffectsSystem))]
[Serializable]
public class EntityStats
{
    [Header("Base Stats")]
    [SerializeField] float speed = 5f;
    [SerializeField] float attackPower = 1f;
    [SerializeField] float defense = 0f;
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float healthRegenRate = 0f;

    [Header("Critical")]
    [SerializeField] float critChance = 0f;
    [SerializeField] float critMultiplier = 1.5f;

    [Header("Resistance")]
    [SerializeField] float knockbackMultiplier = 1f;
    [SerializeField] float knockbackResistance = 0f;
    [SerializeField] float damageBlockChance = 0f;

    [Header("Abilities")]
    [SerializeField] float cooldownReduction = 0f;

    [Header("Luck")]
    [SerializeField] float luck = 0f;

    [Header("Effects")]
    [SerializeField] EntityEffectsSystem entityEffectsSystem;

    Dictionary<StatType, float> baseValues;
    Dictionary<StatType, List<StatModifier>> modifiers;
    Dictionary<StatType, float> cache;

    bool isDirty = true;
    bool initialized;

    public event Action<StatType, float, float> OnChanged;

    public float this[StatType type] => Get(type);

    public float Get(StatType type)
    {
        Init();

        if (isDirty)
            Recalculate();

        return cache.TryGetValue(type, out var val) ? val : 0f;
    }

    public float GetBase(StatType type)
    {
        Init();
        return baseValues.TryGetValue(type, out var val) ? val : 0f;
    }

    public void SetBase(StatType type, float value)
    {
        Init();

        var oldVal = Get(type);
        baseValues[type] = value;
        isDirty = true;

        NotifyIfChanged(type, oldVal);
    }

    public StatModifier AddModifier(StatType type, float flat = 0f, float percent = 0f, object source = null)
    {
        Init();

        if (!modifiers.ContainsKey(type))
            modifiers[type] = new List<StatModifier>();

        var oldVal = Get(type);
        var mod = new StatModifier(flat, percent, source);
        modifiers[type].Add(mod);
        isDirty = true;

        NotifyIfChanged(type, oldVal);

        return mod;
    }

    public bool RemoveModifier(StatType type, StatModifier modifier)
    {
        Init();

        if (!modifiers.TryGetValue(type, out var list))
            return false;

        var oldVal = Get(type);
        var removed = list.Remove(modifier);

        if (removed)
        {
            isDirty = true;
            NotifyIfChanged(type, oldVal);
        }

        return removed;
    }

    public int RemoveModifiersFrom(object source)
    {
        Init();

        int total = 0;

        foreach (var (type, list) in modifiers)
        {
            var oldVal = Get(type);
            var removed = list.RemoveAll(m => ReferenceEquals(m.Source, source));

            if (removed > 0)
            {
                total += removed;
                isDirty = true;
                NotifyIfChanged(type, oldVal);
            }
        }

        return total;
    }

    public void ClearModifiers()
    {
        Init();

        foreach (var (type, list) in modifiers)
        {
            if (list.Count > 0)
            {
                var oldVal = Get(type);
                list.Clear();
                isDirty = true;
                NotifyIfChanged(type, oldVal);
            }
        }
    }

    void Init()
    {
        if (initialized) return;

        if (entityEffectsSystem == null)
            Debug.LogError("EntityEffectsSystem reference is missing in EntityStats!");

        baseValues = new Dictionary<StatType, float>
        {
            [StatType.Speed] = speed,
            [StatType.AttackPower] = attackPower,
            [StatType.Defense] = defense,
            [StatType.MaxHealth] = maxHealth,
            [StatType.HealthRegenRate] = healthRegenRate,
            [StatType.CriticalHitChance] = critChance,
            [StatType.CriticalHitDamageMultiplier] = critMultiplier,
            [StatType.KnockbackMultiplier] = knockbackMultiplier,
            [StatType.KnockbackResistance] = knockbackResistance,
            [StatType.DamageBlockChance] = damageBlockChance,
            [StatType.ActiveAbilityCooldown] = cooldownReduction,
            [StatType.LuckStat] = luck
        };

        modifiers = new Dictionary<StatType, List<StatModifier>>();
        cache = new Dictionary<StatType, float>();

        initialized = true;
    }

    void Recalculate()
    {
        foreach (var (type, baseVal) in baseValues)
        {
            float flat = 0f;
            float percent = 0f;

            if (modifiers.TryGetValue(type, out var list))
            {
                foreach (var mod in list)
                {
                    flat += mod.Flat;
                    percent += mod.Percent;
                }
            }

            float final = (baseVal + flat) * (1f + percent / 100f);

            cache[type] = type switch
            {
                StatType.Speed => Mathf.Max(0f, final),
                StatType.MaxHealth => Mathf.Max(1f, final),
                StatType.CriticalHitChance => Mathf.Clamp(final, 0f, 100f),
                StatType.DamageBlockChance => Mathf.Clamp(final, 0f, 100f),
                StatType.KnockbackResistance => Mathf.Clamp(final, 0f, 100f),
                StatType.ActiveAbilityCooldown => Mathf.Clamp(final, 0f, 90f),
                StatType.CriticalHitDamageMultiplier => Mathf.Max(1f, final),
                _ => Mathf.Max(0f, final)
            };
        }

        isDirty = false;
    }

    void NotifyIfChanged(StatType type, float oldVal)
    {
        if (isDirty) Recalculate();

        if (cache.TryGetValue(type, out var newVal) && Mathf.Abs(oldVal - newVal) > 0.0001f)
            OnChanged?.Invoke(type, oldVal, newVal);
    }
}