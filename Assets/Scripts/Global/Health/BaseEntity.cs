using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EntityEffectsSystem))]
public abstract class BaseEntity : MonoBehaviour, IHealth
{
    [Header("Stats")]
    public EntityStats stats = new EntityStats();

    [Space(10)]
    [Header("Health")]
    [SerializeField] protected ushort maxHealth = 100;
    public ushort CurrentHealth { get; protected set; }

    [Space(10)]
    [Header("Movement")]
    public sbyte direction;

    [Space(10)]
    [Header("Combat Actions")]
    public ActionSO primaryAttack;
    public ActionSO specialAbility;

    // Методы для смены способностей (подбор предметов)
    public void SetPrimaryAttack(ActionSO action) => primaryAttack = action;
    public void SetSpecialAbility(ActionSO action) => specialAbility = action;

    [Space(10)]
    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Collider2D entityCollider;

    [Space(10)]
    [Header("States")]
    public bool isWaiting;

    #region Runtime
    protected float attackCooldown;
    protected float abilityCooldown;
    protected bool isAttacking;
    protected bool isUsingAbility;

    public EntityStats Stats => stats;
    public bool HasAbility => specialAbility != null;
    #endregion 

    #region Velocity Override
    public float? overrideVelocityX { get; private set; }
    public float? overrideVelocityY { get; private set; }
    public bool HasVelocityOverride => overrideVelocityX.HasValue || overrideVelocityY.HasValue;

    Coroutine velocityOverrideCoroutine;
    #endregion

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(ushort amount, Transform attacker = null)
    {
        if (CurrentHealth == 0) return;

        float reduced = Mathf.Max(1, amount - Stats[StatType.Defense]);
        ushort finalDamage = (ushort)reduced;

        CurrentHealth = finalDamage >= CurrentHealth ? (ushort)0 : (ushort)(CurrentHealth - finalDamage);

        OnDamageReceived(amount, attacker);

        if (CurrentHealth == 0)
            OnDeath();
    }

    public void Heal(ushort amount)
    {
        if (CurrentHealth == 0) return;

        CurrentHealth = (ushort)Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    protected virtual void OnDamageReceived(ushort amount, Transform attacker = null) 
    {
    }

    protected virtual void OnDeath()
    {
        this.enabled = false;
    }

    #region Velocity Override
    public void ApplyVelocityOverride(Vector2 velocity, float duration)
    {
        if (velocityOverrideCoroutine != null)
            StopCoroutine(velocityOverrideCoroutine);

        if (CurrentHealth != 0)
            velocityOverrideCoroutine = StartCoroutine(VelocityOverrideRoutine(velocity, duration));
    }

    public void ClearVelocityOverride()
    {
        if (velocityOverrideCoroutine != null)
        {
            StopCoroutine(velocityOverrideCoroutine);
            velocityOverrideCoroutine = null;
        }

        overrideVelocityX = null;
        overrideVelocityY = null;
    }

    IEnumerator VelocityOverrideRoutine(Vector2 velocity, float duration)
    {
        overrideVelocityX = velocity.x;
        overrideVelocityY = velocity.y;
        rb.linearVelocity = velocity;

        yield return new WaitForSeconds(duration);

        overrideVelocityX = null;
        overrideVelocityY = null;
        velocityOverrideCoroutine = null;
    }
    #endregion

    #region Helpers
    public void FaceDirection(float targetX)
    {
        direction = (sbyte)(targetX > transform.position.x ? 1 : -1);
        UpdateVisualDirection();
    }

    public virtual void UpdateVisualDirection()
    {
        if (spriteRenderer != null && direction != 0)
            spriteRenderer.flipX = (direction < 0);
    }
    #endregion
}