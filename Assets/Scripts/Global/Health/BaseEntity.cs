using System;
using System.Collections;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(EntityEffectsSystem))]
public abstract class BaseEntity : MonoBehaviour, IHealth
{
    [Header("Stats")]
    public string _name = "Entity";
    public EntityStats stats = new EntityStats();

    [Space(10)]
    [Header("Health")]
    [SerializeField] protected ushort maxHealth = 100;
    public ushort CurrentHealth { get; protected set; }
    protected string lastDamageCause;

    [Space(10)]
    [Header("Movement")]
    public sbyte direction;

    [Space(10)]
    [Header("Combat Actions")]
    public ActionSO primaryAttack;
    public ActionSO specialAbility;

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

    #region Shader FX (impact/death dissolve)
    [Header("Shader FX")]
    [SerializeField, Min(0.001f)] float impactSeconds = 0.1f;
    [SerializeField, Min(0.01f)] float deathProgressSeconds = 1.5f;

    [Tooltip("If true, object will be deactivated at the end of death dissolve. Disable for Player.")]
    [SerializeField] bool deactivateOnDeath = true;

    // ВАЖНО: Reference из ShaderGraph
    static readonly int ImpactFrameId = Shader.PropertyToID("_impactFrame");
    static readonly int ProgressId = Shader.PropertyToID("_Progress");

    MaterialPropertyBlock mpb;
    SpriteRenderer[] shaderTargets;

    CancellationTokenSource impactCts;
    CancellationTokenSource deathCts;
    bool isDying;
    #endregion

    #region Velocity Override
    public float? overrideVelocityX { get; private set; }
    public float? overrideVelocityY { get; private set; }
    public bool HasVelocityOverride => overrideVelocityX.HasValue || overrideVelocityY.HasValue;

    CancellationTokenSource velocityOverrideCts;
    #endregion

    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
        mpb ??= new MaterialPropertyBlock();

        // initial gather
        RefreshShaderTargets();

        SetMaterialFloat(ImpactFrameId, 0f);
        SetMaterialFloat(ProgressId, 0f);
    }

    void RefreshShaderTargets()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        shaderTargets = GetComponentsInChildren<SpriteRenderer>(true);
        if (shaderTargets == null)
            shaderTargets = Array.Empty<SpriteRenderer>();
    }

    public void TakeDamage(ushort amount, Transform attacker = null, string cause = null)
    {
        if (CurrentHealth == 0) return;

        lastDamageCause = cause ?? "unknown";

        float reduced = Mathf.Max(1, amount - Stats[StatType.Defense]);
        ushort finalDamage = (ushort)reduced;

        CurrentHealth = finalDamage >= CurrentHealth ? (ushort)0 : (ushort)(CurrentHealth - finalDamage);

        if (finalDamage > 0 && CurrentHealth > 0 && !isDying)
            _ = ImpactRoutine(impactSeconds);

        OnDamageReceived(amount, attacker);

        if (CurrentHealth == 0)
            OnDeath();

        if (this is PlayerController)
            PlayerHealthBarUIManager.Instance.UpdateHealth(CurrentHealth, maxHealth);
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
        if (isDying) return;
        isDying = true;

        impactCts?.Cancel();
        impactCts?.Dispose();
        impactCts = null;

        ClearVelocityOverride();

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        var cols = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < cols.Length; i++)
            if (cols[i] != null) cols[i].enabled = false;

        RefreshShaderTargets();

        _ = DeathProgressRoutine(deathProgressSeconds);
    }

    void SetMaterialFloat(int id, float value)
    {
        if (shaderTargets == null || shaderTargets.Length == 0) return;

        mpb ??= new MaterialPropertyBlock();

        for (int i = 0; i < shaderTargets.Length; i++)
        {
            var sr = shaderTargets[i];
            if (sr == null) continue;

            sr.GetPropertyBlock(mpb);
            mpb.SetFloat(id, value);
            sr.SetPropertyBlock(mpb);
        }
    }

    async Awaitable ImpactRoutine(float seconds)
    {
        impactCts?.Cancel();
        impactCts?.Dispose();
        impactCts = new CancellationTokenSource();
        CancellationToken token = impactCts.Token;

        SetMaterialFloat(ImpactFrameId, 1f);

        await Awaitable.WaitForSecondsAsync(Mathf.Max(0.001f, seconds), token);
        if (token.IsCancellationRequested) return;

        SetMaterialFloat(ImpactFrameId, 0f);

        impactCts?.Dispose();
        impactCts = null;
    }

    async Awaitable DeathProgressRoutine(float seconds)
    {
        deathCts?.Cancel();
        deathCts?.Dispose();
        deathCts = new CancellationTokenSource();
        CancellationToken token = deathCts.Token;

        SetMaterialFloat(ImpactFrameId, 0f);
        SetMaterialFloat(ProgressId, 0f);

        float start = Time.time;
        float end = start + Mathf.Max(0.01f, seconds);

        Debug.Log("Starting material");

        while (Time.time < end)
        {
            float t = Mathf.InverseLerp(start, end, Time.time);
            SetMaterialFloat(ProgressId, t);

            await Awaitable.NextFrameAsync(token);
            if (token.IsCancellationRequested) return;
        }

        SetMaterialFloat(ProgressId, 1f);

        if (deactivateOnDeath)
            gameObject.SetActive(false);

        deathCts?.Dispose();
        deathCts = null;
    }

    #region Velocity Override
    public void ApplyVelocityOverride(Vector2 velocity, float duration)
    {
        ClearVelocityOverride();

        if (CurrentHealth != 0)
        {
            velocityOverrideCts = new CancellationTokenSource();
            _ = VelocityOverrideRoutine(velocity, duration, velocityOverrideCts.Token);
        }
    }

    public void ClearVelocityOverride()
    {
        if (velocityOverrideCts != null)
        {
            velocityOverrideCts.Cancel();
            velocityOverrideCts.Dispose();
            velocityOverrideCts = null;
        }

        overrideVelocityX = null;
        overrideVelocityY = null;
    }

    async Awaitable VelocityOverrideRoutine(Vector2 velocity, float duration, CancellationToken token)
    {
        overrideVelocityX = velocity.x;
        overrideVelocityY = velocity.y;
        rb.linearVelocity = velocity;

        await Awaitable.WaitForSecondsAsync(duration, token);

        if (token.IsCancellationRequested) return;

        overrideVelocityX = null;
        overrideVelocityY = null;
        velocityOverrideCts = null;
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