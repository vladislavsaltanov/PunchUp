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
    public float slamWindupDuration = 0.7f;
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

    // States
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

        if (hitbox != null) hitbox.gameObject.SetActive(false);
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
        if (!IsVulnerable) return;

        base.TakeDamage(amount, attacker, cause);

        if (attacker != null)
        {
            float dir = Mathf.Sign(transform.position.x - attacker.position.x);
            rb.linearVelocity = new Vector2(dir * bounceForceX, bounceForceY);
        }

        if (CurrentHealth <= 0) return;

        if (!_phaseTransitionTriggered && CurrentPhase == 1 &&
            CurrentHealth <= Stats[StatType.MaxHealth] * 0.5f)
        {
            _phaseTransitionTriggered = true;
            GoToPhaseTransition();
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
