using System;

[Serializable]
public struct StatModifierData
{
    public StatType statType;
    public float flat;
    public float percent;

    public StatModifier CreateModifier(object source = null)
    {
        return new StatModifier(flat, percent, source);
    }
}