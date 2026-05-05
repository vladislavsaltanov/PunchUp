using System.Threading;
using UnityEngine;

public class EnemyAIBoss : EnemyAI
{
    [Header("References")]
    public BossHitbox hitbox;
    public Transform bodyTransform;
    public LayerMask groundLayer;
    public GameObject projectilePrefab;

    [Header("Arena")]
    public float arenaHalfWidth = 12f;
    public float arenaCenterX = 0f;

    [Header("Shake")]
    public float shakeMagnitude = 0.05f;
    public float shakeFrequency = 20f;
    public Texture2D debrisTexture;

    [Header("Phase 1")]
    public float phase1Cooldown = 2f;
    public float phase1VulnerableDuration = 1.5f;

    [Header("Phase 2")]
    public float phase2Cooldown = 1f;
    public float phase2VulnerableDuration = 0.8f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public ushort dashDamage = 20;

    [Header("Jump Slam")]
    public float jumpHeight = 8f;
    public float apexHangDuration = 0f;
    public ushort slamDamage = 25;

    [Header("Projectile")]
    public float projectileSpeed = 8f;
    public ushort projectileDamage = 15;

    [Header("Combo (Phase 2)")]
    public float comboPauseBetweenHits = 0.35f;
    public ushort comboDamage = 12;

    [Header("Phase Transition")]
    public float transitionDuration = 3f;

    [Header("Bounce on hit")]
    public float bounceForceX = 6f;
    public float bounceForceY = 4f;
    public float counterAttackDistance = 3.5f;
    public ushort counterAttackDamage = 15;

    [Header("Parry Window")]
    public float parryWindowDuration = 0.5f;

    public BossIdleState IdleState { get; private set; }
    public BossDashState DashState { get; private set; }
    public BossJumpSlamState JumpSlamState { get; private set; }
    public BossProjectileState ProjectileState { get; private set; }
    public BossComboState ComboState { get; private set; }
    public BossVulnerableState VulnerableState { get; private set; }
    public BossPhaseTransitionState PhaseTransitionState { get; private set; }

    public PlayerController Player { get; private set; }
    public int CurrentPhase { get; private set; } = 1;
    public bool IsVulnerable { get; set; }
    bool _isParryable;
    public bool IsParryable
    {
        get => _isParryable;
        set
        {
            _isParryable = value;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = _isParryable ? new Color(1f, 0.8f, 0f) : Color.white;
            }
        }
    }
    public bool CanDealDamage { get; set; } = true;
    bool _phaseTransitionTriggered;
    public CancellationTokenSource ShakeCts { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Player = PlayerController.instance;

        IdleState = new BossIdleState(this);
        DashState = new BossDashState(this);
        JumpSlamState = new BossJumpSlamState(this);
        ProjectileState = new BossProjectileState(this);
        ComboState = new BossComboState(this);
        VulnerableState = new BossVulnerableState(this);
        PhaseTransitionState = new BossPhaseTransitionState(this);

        if (hitbox != null)
        {
            hitbox.Init(this);
            hitbox.gameObject.SetActive(false);
        }
        ChangeState(IdleState);
    }

    protected override void Update()
    {
        if (Player == null) Player = PlayerController.instance;
        if (CurrentHealth <= 0) return;
        base.Update();
    }

    public override void TakeDamage(ushort amount, Transform attacker = null, string cause = null)
    {
        bool isParrying = IsParryable && attacker != null && attacker.GetComponent<PlayerController>() != null;

        // Entirely ignore incoming attacks if not vulnerable and not parrying
        if (!IsVulnerable && !isParrying) return;

        if (isParrying)
        {
            // PARRY SUCCESS
            try
            {
                VisualEffectsManager.SpawnDebris(spriteRenderer.sprite.texture, transform.position, Color.lightSlateGray, 5, 0.5f);
            }
            catch { }

            IsParryable = false;
            CanDealDamage = false;

            // Interrupt current attack and transition into vulnerable state
            GoToVulnerable();
            return; // Return early, don't take damage from the parrying blow
        }

        // REGULAR DAMAGE ON VULNERABLE BOSS
        base.TakeDamage(amount, attacker, cause);
        try
        {
            VisualEffectsManager.SpawnDebris(spriteRenderer.sprite.texture, transform.position, Color.darkRed, 10, 0.75f);
        }
        catch { }

        if (CurrentHealth <= 0) return;

        // Reset states
        IsVulnerable = false;
        IsParryable = false;

        // Check for phase transition
        if (!_phaseTransitionTriggered && CurrentPhase == 1 && CurrentHealth <= Stats[StatType.MaxHealth] * 0.5f)
        {
            _phaseTransitionTriggered = true;
            GoToPhaseTransition();
            _ = HandlePhysicsDelayed(attacker, bounceBoss: false);
        }
        else
        {
            // Boss retreats and waits for Cooldown internally inside IdleState
            GoToIdle();
            _ = HandlePhysicsDelayed(attacker, bounceBoss: true);
        }
    }

    public float GetCurrentGracePeriod()
    {
        if (!IsParryable) return 0f;

        if (_currentState == JumpSlamState) return 0.1f;

        return 0.15f;
    }

    public void OnPlayerHitByAttack()
    {
        if (_currentState == DashState || _currentState == ComboState || _currentState == JumpSlamState)
        {
            IsParryable = false;
            IsVulnerable = false;
            CanDealDamage = false;

            GoToIdle();
            _ = HandlePhysicsDelayed(Player != null ? Player.transform : null, bounceBoss: true);
        }
    }

    async Awaitable HandlePhysicsDelayed(Transform attacker, bool bounceBoss)
    {
        await Awaitable.NextFrameAsync();
        if (this == null || CurrentHealth <= 0) return;

        ClearVelocityOverride();

        if (attacker != null)
        {
            var p = attacker.GetComponent<PlayerController>();
            if (p != null)
            {
                float pushDir = Mathf.Sign(p.transform.position.x - transform.position.x);
                p.ApplyVelocityOverride(new Vector2(pushDir * 8f, 3f), 0.2f);
            }
        }

        if (bounceBoss)
        {
            float dx = arenaCenterX - transform.position.x;
            float dir = Mathf.Abs(dx) > 0.5f
                ? Mathf.Sign(dx)
                : (attacker != null ? Mathf.Sign(transform.position.x - attacker.position.x) : direction);

            rb.linearVelocity = new Vector2(dir * bounceForceX, bounceForceY);
        }

        await Awaitable.WaitForSecondsAsync(0.45f);

        if (this == null || Player == null || CurrentHealth <= 0 || IsVulnerable) return;

        float distanceToPlayer = Vector2.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer <= counterAttackDistance)
        {
            float repelDir = Mathf.Sign(Player.transform.position.x - transform.position.x);

            // Only powerfully repel the player to prevent spam, no damage dealt
            Player.ApplyVelocityOverride(new Vector2(repelDir * 20f, 6f), 0.4f);

            direction = (sbyte)repelDir;
            UpdateVisualDirection();
        }
    }

    public void StartShake()
    {
        StopShake();
        ShakeCts = new CancellationTokenSource();
        _ = ShakeLoop(ShakeCts.Token);
    }

    public void StopShake()
    {
        ShakeCts?.Cancel();
        ShakeCts?.Dispose();
        ShakeCts = null;
        if (bodyTransform != null)
            bodyTransform.localPosition = Vector3.zero;
    }

    async Awaitable ShakeLoop(CancellationToken token)
    {
        float t = 0f;
        while (!token.IsCancellationRequested)
        {
            t += Time.deltaTime;
            float x = Mathf.Sin(t * shakeFrequency) * shakeMagnitude;
            if (bodyTransform != null)
                bodyTransform.localPosition = new Vector3(x, 0f, 0f);
            await Awaitable.NextFrameAsync(token);
        }
    }

    public float GetCooldown() => CurrentPhase == 1 ? phase1Cooldown : phase2Cooldown;
    public float GetVulnerableDuration() => CurrentPhase == 1 ? phase1VulnerableDuration : phase2VulnerableDuration;
    public void EnterPhase2() => CurrentPhase = 2;

    public void GoToIdle() => ChangeState(IdleState);
    public void GoToDash() => ChangeState(DashState);
    public void GoToJumpSlam() => ChangeState(JumpSlamState);
    public void GoToProjectile() => ChangeState(ProjectileState);
    public void GoToCombo() => ChangeState(ComboState);
    public void GoToVulnerable() => ChangeState(VulnerableState);
    public void GoToPhaseTransition() => ChangeState(PhaseTransitionState);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(arenaHalfWidth * 2f, 1f, 0f));
    }
}
