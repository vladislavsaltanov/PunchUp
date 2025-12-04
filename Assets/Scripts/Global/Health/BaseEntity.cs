using UnityEditor.Animations;
using UnityEngine;

public abstract class BaseEntity : MonoBehaviour, IHealth
{
    [Header("Health")]
    [SerializeField] protected ushort maxHealth = 100;
    public ushort CurrentHealth { get; protected set; }

    [Space(20)]
    [Header("Movement")]
    public sbyte direction;

    [Space(20)]
    [Header("Components")]
    public Rigidbody2D rb;
    public AnimatorController animations;

    [Space(20)]
    [Header("States")]
    public bool isWaiting;

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(ushort amount, Transform attacker = null)
    {
        if (CurrentHealth == 0) return;

        if (amount >= CurrentHealth)
            CurrentHealth = 0;
        else
            CurrentHealth -= amount;

        OnDamageReceived(amount, attacker);

        if (CurrentHealth == 0)
            OnDeath();
    }

    public void Heal(ushort amount)
    {
        if (CurrentHealth == 0) return;

        CurrentHealth += amount;
        if (CurrentHealth > maxHealth)
            CurrentHealth = maxHealth;
    }

    protected virtual void OnDamageReceived(ushort amount, Transform attacker = null) 
    {
    }

    protected virtual void OnDeath()
    {
    }
}