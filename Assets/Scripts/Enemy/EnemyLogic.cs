using System.Collections;
using System.Threading;
using UnityEngine;

public class EnemyLogic : BaseEntity
{
    [Header("Modules")]
    [SerializeField] EnemyMovementBaseSO movement;
    [SerializeField] EnemyPlayerDetectionSO detection;
    [SerializeField] CombatHandler combatHandler;
    [SerializeField] DoctorAudio doctorAudio;

    [Header("AI Settings")]
    [SerializeField] float abilityChance = 0.3f;
    [SerializeField] float agroTimeout = 5f;
    [SerializeField] float searchDuration = 2f;

    CancellationTokenSource actionCts;
    CancellationTokenSource waitCts;

    // --- НОВОЕ СВОЙСТВО ---
    // Считает реальную дистанцию удара: от центра врага до кончика меча
    public float EffectiveAttackReach
    {
        get
        {
            float weaponRange = (primaryAttack != null) ? primaryAttack.range : 1.0f;

            // Если есть коллайдер, добавляем его половину ширины (extents.x)
            if (entityCollider != null)
            {
                return entityCollider.bounds.extents.x + weaponRange;
            }

            return weaponRange;
        }
    }

    public EnemyState currentState;
    public EnemyContextState context = new EnemyContextState();

    PlayerController playerController;
    GameObject player => playerController?.gameObject ?? this.gameObject;

    protected override void Awake()
    {
        base.Awake();
        actionCts = new CancellationTokenSource();
        waitCts = new CancellationTokenSource();
        if (combatHandler == null) combatHandler = GetComponent<CombatHandler>();
        playerController = PlayerController.instance;

        if (entityCollider == null) entityCollider = GetComponent<Collider2D>();

        if (doctorAudio == null) doctorAudio = GetComponent<DoctorAudio>();

        direction = (sbyte)(UnityEngine.Random.value > 0.5f ? 1 : -1);
        EnterWait(1f);
    }

    private void Update()
    {
        if (CurrentHealth <= 0) return;

        UpdateVisualDirection();

        if (HasVelocityOverride) return;

        context.directionToPlayer = detection.IsPlayerDetected(this, out context.playerDistance);
        context.isPlayerDetected = context.directionToPlayer != 0;

        if (context.isPlayerDetected)
        {
            context.lastTimeSeenPlayer = Time.time;
            context.lastKnownDirection = context.directionToPlayer;
        }

        if (currentState == EnemyState.Attacking || currentState == EnemyState.UsingAbility)
            return;

        EvaluateSituation();
        ExecuteState();
    }

    void EvaluateSituation()
    {
        if (context.wasHit)
        {
            if (Time.time > context.lastHitTime + agroTimeout)
            {
                context.wasHit = false;
            }
            else
            {
                if (context.directionToPlayer != 0) direction = context.directionToPlayer;
                ChooseCombatOrChase();
                return;
            }
        }

        if (context.isPlayerDetected)
        {
            direction = context.directionToPlayer;
            ChooseCombatOrChase();
            return;
        }

        if (Time.time < context.lastTimeSeenPlayer + searchDuration)
        {
            if (currentState != EnemyState.Waiting)
            {
                direction = context.lastKnownDirection;
                currentState = EnemyState.WalkingTowardsPlayer;
            }
            return;
        }

        if (currentState != EnemyState.Waiting)
        {
            currentState = EnemyState.Walking;
        }
    }

    void ChooseCombatOrChase()
    {
        // ИСПОЛЬЗУЕМ EffectiveAttackReach (Ширина + Оружие)
        if (context.playerDistance <= EffectiveAttackReach)
        {
            currentState = EnemyState.Combat;
        }
        else
        {
            currentState = EnemyState.WalkingTowardsPlayer;
        }
    }

    void ExecuteState()
    {
        switch (currentState)
        {
            case EnemyState.Walking:
                movement.Movement(this, context);
                break;
            case EnemyState.WalkingTowardsPlayer:
                movement.MovementTowardsPlayer(this, context, detection, direction);
                break;
            case EnemyState.Combat:
                HandleCombat();
                break;
            case EnemyState.Waiting:
                movement.Stop(this);
                break;
        }
    }

    void HandleCombat()
    {
        if (context.isPlayerDetected)
        {
            float dirToPlayer = player.transform.position.x - transform.position.x;
            if (Mathf.Abs(dirToPlayer) > 0.1f) direction = (sbyte)Mathf.Sign(dirToPlayer);
        }

        movement.Stop(this);

        // Также учитываем размеры коллайдера при выходе из боя
        // Умножаем на 1.2 только саму дальность оружия, а не ширину тела, чтобы было точнее
        float weaponRange = (primaryAttack != null) ? primaryAttack.range : 1.0f;
        float bodySize = (entityCollider != null) ? entityCollider.bounds.extents.x : 0f;

        float exitDistance = bodySize + (weaponRange * 1.2f);

        if (context.playerDistance > exitDistance)
        {
            return;
        }

        if (specialAbility != null && UnityEngine.Random.value < abilityChance)
        {
            combatHandler.TrySpecialAbility();
            return;
        }

        combatHandler.TryPrimaryAttack();
    }

    public void EnterWait(float duration)
    {
        if (context.isPlayerDetected || context.wasHit) return;
        if (currentState == EnemyState.Waiting) return;

        currentState = EnemyState.Waiting;

        if (movement != null)
            movement.Stop(this);
        else if (rb != null)
            rb.linearVelocity = Vector2.zero;

        _ = WaitFor(duration);
    }

    public async Awaitable WaitFor(float time)
    {
        waitCts?.Cancel();
        waitCts?.Dispose();
        waitCts = new CancellationTokenSource();
        await Awaitable.WaitForSecondsAsync(time, waitCts.Token);
        currentState = EnemyState.Walking;
    }

    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        if (attacker != null && ((1 << attacker.gameObject.layer) & detection.playerLayer.value) != 0)
        {
            waitCts?.Cancel();
            waitCts?.Dispose();
            waitCts = new CancellationTokenSource();
            actionCts?.Cancel();
            actionCts?.Dispose();
            actionCts = new CancellationTokenSource();

            context.directionToPlayer = (sbyte)(attacker.position.x - transform.position.x > 0 ? 1 : -1);
            direction = context.directionToPlayer;

            context.lastHitTime = Time.time;
            context.wasHit = true;

            currentState = EnemyState.WalkingTowardsPlayer;

            doctorAudio.HandleDamage();
        }
    }

    protected override void OnDeath()
    {
        combatHandler.CancelAll();
        waitCts?.Cancel();
        waitCts?.Dispose();
        actionCts?.Cancel();
        actionCts?.Dispose();

        StatisticsHandler.Instance.statisticData.kills++;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        entityCollider.enabled = false;

        Destroy(gameObject, 0.1f);
    }
    private void OnDrawGizmosSelected()
    {
        if (primaryAttack == null && entityCollider == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, EffectiveAttackReach);
    }
}

[System.Serializable]
public class EnemyContextState
{
    public bool isPlayerDetected = false;
    public bool wasHit = false;
    public float playerDistance = Mathf.Infinity;
    public float lastHitTime = -100f;

    // Для логики "потерял из виду"
    public float lastTimeSeenPlayer = -100f;
    public sbyte lastKnownDirection = 0;

    public sbyte directionToPlayer = 0;
    public float lastXPosition = 0f;
    public float stuckTimer = 0f;
}