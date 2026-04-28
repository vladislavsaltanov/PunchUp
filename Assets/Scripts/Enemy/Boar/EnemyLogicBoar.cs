using UnityEngine;
using System.Threading;

public class EnemyLogicBoar : EnemyLogic
{
    enum BoarState { Patrol, Charging, Recovering, Stunned }

    [Header("Boar: Detection")]
    [SerializeField] float detectionRange = 8f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask losBlockerLayer;
    [SerializeField] LayerMask groundLayer;

    [Header("Boar: Charge")]
    [SerializeField] float chargeSpeed = 14f;
    [SerializeField] float chargeAcceleration = 30f;
    [SerializeField] float chargeDuration = 2f;
    [SerializeField] ushort chargeDamage = 15;
    [SerializeField] float chargeKnockbackX = 10f;
    [SerializeField] float chargeKnockbackY = 4f;
    [SerializeField] float chargeKnockbackDuration = 0.3f;
    [SerializeField] LayerMask hitTargetLayer;

    [Header("Boar: Recovery")]
    [SerializeField] float recoveryDuration = 1.5f;

    [Header("Boar: Stun on damage")]
    [SerializeField] Vector2 stunDurationRange = new Vector2(0.5f, 1.5f);

    BoarState boarState = BoarState.Patrol;
    float stateTimer;
    float currentChargeSpeed;
    sbyte chargeDirection;
    bool hitPlayerThisCharge;

    PlayerController player;

    static readonly Collider2D[] hitBuffer = new Collider2D[8];
    float lastHitTime;
    [SerializeField] float hitCooldown = 0.5f;
    float lastChargeTime;
    [SerializeField] float chargeCooldown = 3f;
    protected override void Awake()
    {
        base.Awake();
        player = PlayerController.instance;
    }

    protected override void Update()
    {
        if (player == null) player = PlayerController.instance;
        if (CurrentHealth <= 0) return; 
        UpdateVisualDirection();
        if (HasVelocityOverride) return;

        switch (boarState)
        {
            case BoarState.Patrol:
                PatrolTick();
                break;

            case BoarState.Charging:
                ChargeTick();
                break;

            case BoarState.Recovering:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, 0f, chargeAcceleration * Time.deltaTime);
                break;

            case BoarState.Stunned:
                rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, 0f, chargeAcceleration * Time.deltaTime);
                break;
        }
    }

    void PatrolTick()
    {
        if (CanSeePlayer() && Time.time >= lastChargeTime + chargeCooldown)
        {
            StartCharge();
            return; 
        }

        if (movement != null)
        {
            var ctx = context;
            movement.Movement(this, ctx);
        }
    }

    void ChargeTick()
    {
        stateTimer -= Time.deltaTime;

        currentChargeSpeed = Mathf.MoveTowards(
            currentChargeSpeed, chargeSpeed, chargeAcceleration * Time.deltaTime);

        rb.linearVelocityX = chargeDirection * currentChargeSpeed;

        if (!hitPlayerThisCharge)
        {
            int count = Physics2D.OverlapBoxNonAlloc(
                entityCollider.bounds.center,
                entityCollider.bounds.size,
                0f, hitBuffer, hitTargetLayer);

            BaseEntity hitEntity = null;
            for (int i = 0; i < count; i++)
            {
                var entity = hitBuffer[i]?.GetComponentInParent<BaseEntity>();
                if (entity == null || entity == this) continue;
                hitEntity = entity;
                break;
            }

            if (hitEntity != null && Time.time >= lastHitTime + hitCooldown)
            {
                lastHitTime = Time.time;
                hitPlayerThisCharge = true;
                EnterRecovery();

                hitEntity.ApplyVelocityOverride(
                    new Vector2(chargeDirection * chargeKnockbackX, chargeKnockbackY),
                    chargeKnockbackDuration);
                hitEntity.TakeDamage(chargeDamage, transform, _name);
                
                return;
            }
        }

        bool wallHit = movement != null && movement.IsBlocked(this, chargeDirection);
        if (wallHit || stateTimer <= 0f)
            EnterRecovery();
    }

    void StartCharge()
    {
        lastChargeTime = Time.time;
        boarState = BoarState.Charging;
        stateTimer = chargeDuration;
        currentChargeSpeed = 0f;
        hitPlayerThisCharge = false;
        chargeDirection = (sbyte)(player.transform.position.x > transform.position.x ? 1 : -1);
        direction = chargeDirection;
    }

    async void EnterRecovery()
    {
        boarState = BoarState.Recovering;
        rb.linearVelocityX = 0f;

        // Используем actionCts из базового класса для отмены, если кабан умрет или будет оглушен
        actionCts?.Cancel();
        actionCts = new CancellationTokenSource();

        try 
        {
            await Awaitable.WaitForSecondsAsync(recoveryDuration, actionCts.Token);
            if (boarState == BoarState.Recovering)
                boarState = BoarState.Patrol;
        }
        catch (System.OperationCanceledException) { }
    }

    bool CanSeePlayer()
    { 
        if (player == null) return false;
        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist > detectionRange) return false;

        Vector2 dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, losBlockerLayer);
        return hit.collider == null;
    }

    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        boarState = BoarState.Stunned;
        rb.linearVelocityX = 0f;

        _ = PerformStun();
    }

    async Awaitable PerformStun()
    {
        actionCts?.Cancel();
        actionCts = new CancellationTokenSource();

        try
        {
            await Awaitable.WaitForSecondsAsync(Random.Range(stunDurationRange.x, stunDurationRange.y), actionCts.Token);
            if (boarState == BoarState.Stunned)
                boarState = BoarState.Patrol;
        }
        catch (System.OperationCanceledException) { }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
