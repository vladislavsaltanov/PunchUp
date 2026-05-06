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
    public CanvasGroup hintCanvasGroup;
    public UnityEngine.UI.Image hintBackground;

    [Header("Poise System")]
    public float maxPoise = 300f;
    public float currentPoise;
    public float poiseDamageFromAttack = 100f;
    public float poiseDamageFromParry = 300f;
    public float poiseRegenRate = 30f;
    [Range(0f, 1f)]
    public float nonVulnerableDamageMultiplier = 0f;

    public BossIdleState IdleState { get; private set; }
    public BossDashState DashState { get; private set; }
    public BossJumpSlamState JumpSlamState { get; private set; }
    public BossProjectileState ProjectileState { get; private set; }
    public BossComboState ComboState { get; private set; }
    public BossVulnerableState VulnerableState { get; private set; }
    public BossPhaseTransitionState PhaseTransitionState { get; private set; }
    public BossGroundSlamState GroundSlamState { get; private set; }

    public PlayerController Player { get; private set; }
    public int CurrentPhase { get; private set; } = 1;
    public bool IsVulnerable { get; set; }
    bool _isParryable;
    public bool IsParryable
    {
        get => _isParryable;
        set
        {
            if (_isParryable == value) return;
            _isParryable = value;
            
            if (_isParryable)
            {
                _ = ShowHint(Color.yellow, parryWindowDuration);
            }
            else
            {
                // Cancel the hint immediately when parry window closes
                _hintCts?.Cancel();
                _hintCts?.Dispose();
                _hintCts = null;
            }
        }
    }
    public bool CanDealDamage { get; set; } = true;
    public bool ShouldJumpAfterVulnerable { get; set; }
    public int hitsSinceLastVulnerable;
    bool _phaseTransitionTriggered;
    public CancellationTokenSource ShakeCts { get; private set; }
    CancellationTokenSource _hintCts;

    protected override void Awake()
    {
        base.Awake();
        currentPoise = maxPoise;
        Player = PlayerController.instance;

        IdleState = new BossIdleState(this);
        DashState = new BossDashState(this);
        JumpSlamState = new BossJumpSlamState(this);
        ProjectileState = new BossProjectileState(this);
        ComboState = new BossComboState(this);
        VulnerableState = new BossVulnerableState(this);
        PhaseTransitionState = new BossPhaseTransitionState(this);
        GroundSlamState = new BossGroundSlamState(this);

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
        
        // Poise regeneration when not vulnerable
        if (!IsVulnerable)
        {
            currentPoise = Mathf.MoveTowards(currentPoise, maxPoise, poiseRegenRate * Time.deltaTime);
        }

        base.Update();
    }

    public void RepelPlayer(Transform attacker)
    {
        if (Player == null) return;
        
        float pushDir = Mathf.Sign(Player.transform.position.x - transform.position.x);
        // Strong, short impulse to break attack sequences
        Player.ApplyVelocityOverride(new Vector2(pushDir * 12f, 2f), 0.1f);
    }

    public override void TakeDamage(ushort amount, Transform attacker = null, string cause = null)
    {
        bool isParrying = IsParryable && attacker != null && attacker.GetComponent<PlayerController>() != null;

        if (isParrying)
        {
            currentPoise -= poiseDamageFromParry;
            
            try { VisualEffectsManager.SpawnDebris(spriteRenderer.sprite.texture, transform.position, Color.lightSlateGray, 5, 0.5f); }
            catch { }

            IsParryable = false;
            CanDealDamage = false;

            if (CurrentPhase == 2 && _currentState == JumpSlamState && ConsecutiveJumps < 3)
            {
                ConsecutiveJumps++;
                ShouldJumpAfterVulnerable = true;
            }

            if (currentPoise <= 0) GoToVulnerable();
            
            // SUCCESSFUL PARRY: 200% Damage
            base.TakeDamage((ushort)(amount * 2.0f), attacker, cause);
            return; 
        }

        if (!IsVulnerable)
        {
            hitsSinceLastVulnerable++;
            currentPoise -= poiseDamageFromAttack;
            RepelPlayer(attacker);
            
            float multiplier = 0f;
            if (hitsSinceLastVulnerable <= 10) multiplier = 0.1f;
            else if (hitsSinceLastVulnerable % 5 == 0) multiplier = 0.5f;

            base.TakeDamage((ushort)(amount * multiplier), attacker, cause);

            if (currentPoise <= 0) GoToVulnerable();
            return;
        }

        base.TakeDamage(amount, attacker, cause);
        try { VisualEffectsManager.SpawnDebris(spriteRenderer.sprite.texture, transform.position, Color.darkRed, 10, 0.75f); }
        catch { }

        if (CurrentHealth <= 0) return;

        hitsSinceLastVulnerable = 0;
        IsVulnerable = false;
        IsParryable = false;

        if (!_phaseTransitionTriggered && CurrentPhase == 1 && CurrentHealth <= Stats[StatType.MaxHealth] * 0.5f)
        {
            _phaseTransitionTriggered = true;
            GoToPhaseTransition();
            _ = HandlePhysicsDelayed(attacker, bounceBoss: false);
        }
        else
        {
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

    public int ConsecutiveJumps { get; set; }

    public void OnPlayerHitByAttack()
    {
        ConsecutiveJumps = 0; // Reset sequence on hit

        if (_currentState == DashState || _currentState == ComboState || _currentState == JumpSlamState)
        {
            // Register hit in the state if possible
            if (_currentState is BossDashState dash) dash.RegisterHit();
            if (_currentState is BossJumpSlamState jump) jump.RegisterHit();
            if (_currentState is BossComboState combo) combo.RegisterHit();

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
    public void EnterPhase2()
    {
        stats.AddModifier(StatType.HealthRegenRate, percent: -100f, source: this);
        CurrentPhase = 2;
    }

    public void GoToIdle() => ChangeState(IdleState);
    public void GoToDash() => ChangeState(DashState);
    public void GoToJumpSlam() => ChangeState(JumpSlamState);
    public void GoToProjectile() => ChangeState(ProjectileState);
    public void GoToCombo() => ChangeState(ComboState);
    public void GoToVulnerable() => ChangeState(VulnerableState);
    public void GoToPhaseTransition() => ChangeState(PhaseTransitionState);

    public async Awaitable ShowHint(Color color, float duration)
    {
        if (hintCanvasGroup == null || hintBackground == null) return;
        
        // Cancel previous hint task
        _hintCts?.Cancel();
        _hintCts?.Dispose();
        _hintCts = new CancellationTokenSource();
        var token = _hintCts.Token;

        try
        {
            hintBackground.color = color;
            
            // Smooth Fade In
            float fadeTime = 0.15f;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                if (token.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                hintCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeTime);
                await Awaitable.NextFrameAsync();
            }
            hintCanvasGroup.alpha = 1f;

            // Hold
            await Awaitable.WaitForSecondsAsync(duration, token);

            // Smooth Fade Out
            elapsed = 0f;
            while (elapsed < fadeTime)
            {
                if (token.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                hintCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                await Awaitable.NextFrameAsync();
            }
        }
        catch (System.OperationCanceledException) { }
        catch (System.Exception) { }
        finally
        {
            // Guaranteed reset regardless of how the method ended
            hintCanvasGroup.alpha = 0f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(arenaHalfWidth * 2f, 1f, 0f));
    }
}
