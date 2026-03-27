using System;
public readonly struct StatModifier : IEquatable<StatModifier>
{
    public readonly float Flat;
    public readonly float Percent;
    public readonly object Source;

    readonly Guid _id;

    public StatModifier(float flat = 0f, float percent = 0f, object source = null)
    {
        Flat = flat;
        Percent = percent;
        Source = source;
        _id = Guid.NewGuid();
    }

    public bool IsValid => _id != Guid.Empty;

    public bool Equals(StatModifier other) => _id == other._id;
    public override bool Equals(object obj) => obj is StatModifier m && Equals(m);
    public override int GetHashCode() => _id.GetHashCode();
}