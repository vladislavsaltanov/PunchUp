using System;
using UnityEngine;

[Serializable]
public struct StatModifierData
{
    [Tooltip("Какой стат модифицируем")]
    public StatType statType;

    [Header("Value Settings")]
    [Tooltip("Flat = +5 единиц\nPercent = +25%")]
    public ModifierValueType valueType;

    [Tooltip("Linear: значение за 1 стак\nHyperbolic: максимум эффекта\nExponential: шанс за 1 стак")]
    public float value;

    [Header("Stacking")]
    [Tooltip("Тип стакания эффекта")]
    public StackType stackType;

    [Tooltip("Только для Hyperbolic: скорость роста")]
    [Range(0.05f, 0.5f)]
    public float coefficient;

    public float CalculateValue(int stacks)
    {
        if (stacks <= 0) return 0f;

        return stackType switch
        {
            StackType.Linear => Linear(stacks),
            StackType.Hyperbolic => Hyperbolic(stacks),
            StackType.Exponential => Exponential(stacks),
            _ => Linear(stacks)
        };
    }

    public bool IsFlat => valueType == ModifierValueType.Flat;
    public bool IsPercent => valueType == ModifierValueType.Percent;

    float Linear(int stacks)
    {
        return value * stacks;
    }

    float Hyperbolic(int stacks)
    {
        float c = coefficient > 0f ? coefficient : 0.15f;
        float cx = c * stacks;
        float ratio = cx / (cx + 1f);
        return value * ratio;
    }

    float Exponential(int stacks)
    {
        float chancePerStack = Mathf.Clamp(value, 0f, 100f) / 100f;
        float missChance = Mathf.Pow(1f - chancePerStack, stacks);
        return (1f - missChance) * 100f;
    }
}