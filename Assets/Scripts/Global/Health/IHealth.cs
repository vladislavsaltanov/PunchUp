using UnityEngine;

public interface IHealth
{
    ushort CurrentHealth { get; }
    void TakeDamage(ushort amount, Transform attacker = null);
    void Heal(ushort amount);
}