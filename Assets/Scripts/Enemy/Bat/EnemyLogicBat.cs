using UnityEngine;

public class EnemyLogicBat : EnemyLogic
{
    enum BatAiState
    {
        None,
        PreparingAttack
    }

    [System.Serializable]
    class BatContextState
    {
        public Vector2 targetPoint;
        public bool hasTarget;

        public float originalY;

        public float nextScanTime;
        public float attackEndTime;
        public float waitOnGroundEndTime;
        public float riseEndTime;
        public float repeatAfterHitTime;

        public float bobPhase;

        public float lastX;
        public float stuckTimer;

        public float prepareAttackEndTime;

        public float nextDamageTime;
        public bool damagedThisDive;

        // Patrol constraints runtime
        public float patrolAltitudeAboveGround;
        public float requiredHeadClearance;
    }

    [Header("Bat: Scan (down to ground)")]
    [SerializeField] Vector2 scanBoxSize = new Vector2(3f, 6f);
    [SerializeField] Vector2 scanBoxOffset = new Vector2(0f, -0.25f);
    [SerializeField] LayerMask scanTargetLayer;
    [SerializeField] LayerMask groundLayer;

    [Header("Bat: Ceiling")]
    [Tooltip("Layers that count as ceiling above the bat. Often same as groundLayer.")]
    [SerializeField] LayerMask ceilingLayer;

    [Header("Ground Probe")]
    [SerializeField] float groundProbeDistance = 50f;

    [Header("Ceiling Probe")]
    [SerializeField] float ceilingProbeDistance = 20f;

    [Header("Bat: Scan Runtime")]
    [SerializeField] float scanTickInterval = 0.15f;

    [Header("Bat: Attack Delay")]
    [SerializeField] Vector2 prepareAttackDelayRange = new Vector2(1f, 2f);

    [Header("Bat: Patrol Height Constraints (above ground)")]
    [Tooltip("Bat will never choose a patrol altitude above this range (above ground).")]
    [SerializeField] Vector2 maxAltitudeAboveGroundRange = new Vector2(5f, 6f);

    [Tooltip("Required free space above bat (to ceiling). If there is less, bat will go lower.")]
    [SerializeField] Vector2 requiredHeadClearanceRange = new Vector2(1f, 2f);

    [Tooltip("If bat falls below this altitude above ground, it recalculates patrol height.")]
    [SerializeField] float minAltitudeAboveGround = 2.5f;

    [Header("Bat: Bobbing (vertical oscillation)")]
    [SerializeField] float bobAmplitude = 0.35f;
    [SerializeField] float bobFrequency = 1.25f;

    [Header("Bat: Movement")]
    [SerializeField] float patrolSpeedMultiplier = 1.0f;
    [SerializeField] float chaseSpeedMultiplier = 2.0f;
    [SerializeField] float diveSpeed = 10f;
    [SerializeField] float riseSpeed = 6f;
    [SerializeField] float groundCheckDistance = 0.2f;

    [Header("Bat: Vertical Control")]
    [SerializeField] float verticalFollowSpeed = 6f;
    [SerializeField] float maxVerticalSpeed = 6f;

    [Header("Bat: Stuck Detection")]
    [SerializeField] float stuckMoveThresholdX = 0.005f;
    [SerializeField] float stuckTimeThreshold = 0.25f;
    [SerializeField] float unstuckBoostX = 1.5f;

    [Header("Bat: Attack/Timers")]
    [SerializeField] float attackChaseSecondsN = 2f;
    [SerializeField] float attackRadiusK = 1.2f;
    [SerializeField] float attackHitCooldown = 0.35f;
    [SerializeField] float groundWaitSecondsK = 1.5f;
    [SerializeField] float riseSecondsP = 1.0f;
    [SerializeField] float reattackDelayAfterHit = 1.0f;

    [Header("Bat: Damage")]
    [SerializeField] ushort damage = 10;

    [Header("Bat: Sprites")]
    [SerializeField] Sprite spriteA_Idle;
    [SerializeField] Sprite spriteB_Attack;
    [SerializeField] Sprite spriteC_MissWait;

    BatContextState bat = new BatContextState();
    BatAiState batAiState = BatAiState.None;

    float patrolBaseY;

    float PatrolTargetY =>
        patrolBaseY + Mathf.Sin((Time.time + bat.bobPhase) * (bobFrequency * Mathf.PI * 2f)) * bobAmplitude;

    protected override void Awake()
    {
        base.Awake();

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (entityCollider == null) entityCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (ceilingLayer.value == 0)
            ceilingLayer = groundLayer; // safe default

        bat.originalY = transform.position.y;
        bat.bobPhase = Random.value * 10f;

        // pick constraints once (stable “personality”)
        bat.patrolAltitudeAboveGround = Random.Range(maxAltitudeAboveGroundRange.x, maxAltitudeAboveGroundRange.y);
        bat.requiredHeadClearance = Random.Range(requiredHeadClearanceRange.x, requiredHeadClearanceRange.y);

        patrolBaseY = PickInitialPatrolY();

        bat.nextScanTime = Time.time;
        bat.lastX = transform.position.x;

        currentState = EnemyState.FlyingPatrol;
    }

    float PickInitialPatrolY()
    {
        if (!TryGetGroundPointBelowProbed(out Vector2 ground))
            return transform.position.y;

        float allowedAltitude = GetAllowedAltitudeAboveGround(ground.y);
        return ground.y + allowedAltitude;
    }

    void Update()
    {
        if (CurrentHealth <= 0) return;
        if (HasVelocityOverride) return;

        UpdateVisualDirection();

        if (Time.time < bat.repeatAfterHitTime)
        {
            SetSprite(spriteA_Idle);
            PatrolTick();
            return;
        }

        switch (currentState)
        {
            case EnemyState.FlyingPatrol:
                SetSprite(spriteA_Idle);

                EnsureMinAltitudeAndCeilingClearance();

                if (Time.time >= bat.nextScanTime && batAiState != BatAiState.PreparingAttack)
                {
                    bat.nextScanTime = Time.time + scanTickInterval;
                    TryAcquireTargetFromDownScan();
                }

                if (batAiState == BatAiState.PreparingAttack)
                {
                    rb.linearVelocity = new Vector2(0f, VerticalSeek(PatrolTargetY, verticalFollowSpeed));
                    if (Time.time >= bat.prepareAttackEndTime)
                    {
                        batAiState = BatAiState.None;
                        bat.attackEndTime = Time.time + attackChaseSecondsN;
                        bat.damagedThisDive = false;
                        currentState = EnemyState.DivingToPoint;
                    }
                }
                else
                {
                    PatrolTick();
                }
                break;

            case EnemyState.DivingToPoint:
                SetSprite(spriteB_Attack);
                DiveTick();
                break;

            case EnemyState.WaitingOnGround:
                SetSprite(spriteC_MissWait);
                rb.linearVelocity = Vector2.zero;

                if (Time.time >= bat.waitOnGroundEndTime)
                {
                    bat.riseEndTime = Time.time + riseSecondsP;
                    currentState = EnemyState.RisingBack;
                }
                break;

            case EnemyState.RisingBack:
                SetSprite(spriteA_Idle);
                EnsureMinAltitudeAndCeilingClearance();
                RiseTick();
                break;

            default:
                currentState = EnemyState.FlyingPatrol;
                break;
        }
    }

    void EnsureMinAltitudeAndCeilingClearance()
    {
        if (!TryGetGroundPointBelowProbed(out Vector2 ground))
            return;

        float groundY = ground.y;
        float altitude = transform.position.y - groundY;

        // If too low: re-anchor baseline using allowed altitude
        if (altitude < minAltitudeAboveGround)
        {
            patrolBaseY = groundY + GetAllowedAltitudeAboveGround(groundY);
            return;
        }

        // If current target violates ceiling clearance: push baseline down
        float allowed = GetAllowedAltitudeAboveGround(groundY);
        float desiredY = groundY + allowed;

        // only adjust down (don’t jerk upwards constantly)
        if (desiredY < patrolBaseY)
            patrolBaseY = desiredY;
    }

    float GetAllowedAltitudeAboveGround(float groundY)
    {
        float desired = Mathf.Clamp(bat.patrolAltitudeAboveGround, 0f, maxAltitudeAboveGroundRange.y);

        // Limit by ceiling distance so that there’s head clearance
        if (TryGetCeilingDistance(out float ceilingDist))
        {
            float maxAltitudeByCeiling = Mathf.Max(0f, ceilingDist - bat.requiredHeadClearance);
            desired = Mathf.Min(desired, maxAltitudeByCeiling);
        }

        // Always cap by max range (5–6)
        desired = Mathf.Min(desired, maxAltitudeAboveGroundRange.y);

        return desired;
    }

    bool TryGetCeilingDistance(out float ceilingDistance)
    {
        Vector2 origin = transform.position;

        // if we have collider, start from top to make it intuitive
        if (entityCollider != null)
            origin.y = entityCollider.bounds.max.y;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, ceilingProbeDistance, ceilingLayer);
        if (hit.collider == null)
        {
            ceilingDistance = float.PositiveInfinity;
            return false;
        }

        ceilingDistance = hit.distance;
        return true;
    }

    void PatrolTick()
    {
        float speedX = Stats[StatType.Speed] * patrolSpeedMultiplier;
        float vx = direction * speedX;

        float vy = VerticalSeek(PatrolTargetY, verticalFollowSpeed);

        rb.linearVelocity = new Vector2(vx, vy);

        if (IsWallAhead(direction))
        {
            direction *= -1;
            rb.linearVelocity = new Vector2(direction * speedX, vy);
        }

        HandleStuckInPatrol(speedX);
    }

    void HandleStuckInPatrol(float intendedSpeedX)
    {
        float x = transform.position.x;
        float movedX = Mathf.Abs(x - bat.lastX);
        bat.lastX = x;

        bool tryingToMove = Mathf.Abs(intendedSpeedX) > 0.01f;

        if (tryingToMove && movedX < stuckMoveThresholdX)
        {
            bat.stuckTimer += Time.deltaTime;
            if (bat.stuckTimer >= stuckTimeThreshold)
            {
                bat.stuckTimer = 0f;

                direction *= -1;

                float vy = rb.linearVelocity.y;
                float boostX = direction * (Stats[StatType.Speed] * patrolSpeedMultiplier + unstuckBoostX);
                rb.linearVelocity = new Vector2(boostX, vy);
            }
        }
        else
        {
            bat.stuckTimer = 0f;
        }
    }

    void TryAcquireTargetFromDownScan()
    {
        if (!TryGetGroundPointBelowProbed(out Vector2 groundPoint))
            return;

        Vector2 top = (Vector2)transform.position + scanBoxOffset;
        float height = Mathf.Max(0.1f, top.y - groundPoint.y);

        Vector2 size = new Vector2(scanBoxSize.x, height);
        Vector2 center = new Vector2(top.x, groundPoint.y + height * 0.5f);

        var hits = Physics2D.OverlapBoxAll(center, size, 0f, scanTargetLayer);
        if (hits == null || hits.Length == 0)
            return;

        Transform best = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h == null) continue;
            if (h.transform == transform) continue;

            float sqr = ((Vector2)h.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = h.transform;
            }
        }

        if (best == null) return;

        bat.hasTarget = true;
        bat.targetPoint = best.position;
        FaceDirection(bat.targetPoint.x);

        batAiState = BatAiState.PreparingAttack;
        bat.prepareAttackEndTime = Time.time + Random.Range(prepareAttackDelayRange.x, prepareAttackDelayRange.y);
    }

    void DiveTick()
    {
        if (Time.time >= bat.attackEndTime)
        {
            BeginMissWait();
            return;
        }

        Vector2 pos = transform.position;
        Vector2 to = bat.targetPoint - pos;

        float vx = Mathf.Clamp(to.x, -1f, 1f) * Stats[StatType.Speed] * chaseSpeedMultiplier;
        float vy = -Mathf.Abs(diveSpeed);

        rb.linearVelocity = new Vector2(vx, vy);

        if (!bat.damagedThisDive && Time.time >= bat.nextDamageTime && TryDamagePlayerInRadiusK())
        {
            bat.damagedThisDive = true;
            bat.nextDamageTime = Time.time + attackHitCooldown;

            rb.linearVelocity = new Vector2(0f, riseSpeed * 0.75f);

            bat.hasTarget = false;
            bat.repeatAfterHitTime = Time.time + reattackDelayAfterHit;

            currentState = EnemyState.FlyingPatrol;
            return;
        }

        if (!IsGroundBelow())
            return;

        rb.linearVelocity = Vector2.zero;
        BeginMissWait();
    }

    bool TryDamagePlayerInRadiusK()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, attackRadiusK, scanTargetLayer);
        if (hits == null || hits.Length == 0)
            return false;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h == null) continue;

            var entity = h.GetComponentInParent<BaseEntity>();
            if (entity == null) continue;
            if (entity == this) continue;

            entity.TakeDamage(damage, transform, _name);
            return true;
        }

        return false;
    }

    void BeginMissWait()
    {
        batAiState = BatAiState.None;
        bat.hasTarget = false;

        bat.waitOnGroundEndTime = Time.time + groundWaitSecondsK;
        currentState = EnemyState.WaitingOnGround;
    }

    void RiseTick()
    {
        float targetY = PatrolTargetY;
        float vy = VerticalSeek(targetY, verticalFollowSpeed);

        float speedX = direction * Stats[StatType.Speed] * patrolSpeedMultiplier * 0.5f;
        rb.linearVelocity = new Vector2(speedX, vy);

        float dy = Mathf.Abs(targetY - transform.position.y);
        if (dy <= 0.1f || Time.time >= bat.riseEndTime)
        {
            currentState = EnemyState.FlyingPatrol;
        }

        if (IsWallAhead(direction))
            direction *= -1;
    }

    float VerticalSeek(float targetY, float followSpeed)
    {
        float dy = targetY - transform.position.y;
        float vy = dy * followSpeed;
        return Mathf.Clamp(vy, -maxVerticalSpeed, maxVerticalSpeed);
    }

    bool TryGetGroundPointBelowProbed(out Vector2 groundPoint)
    {
        Vector2 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundProbeDistance, groundLayer);

        if (hit.collider == null)
        {
            groundPoint = default;
            return false;
        }

        groundPoint = hit.point;
        return true;
    }

    bool IsGroundBelow()
    {
        Vector2 origin = transform.position;

        if (entityCollider != null)
            origin.y = entityCollider.bounds.min.y;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    bool IsWallAhead(sbyte dir)
    {
        if (entityCollider == null) return false;

        Bounds b = entityCollider.bounds;
        float frontX = (dir > 0) ? b.max.x : b.min.x;

        Vector2 wallOrigin = new Vector2(frontX, b.center.y);
        Vector2 wallDir = new Vector2(dir, 0f);

        RaycastHit2D hit = Physics2D.Raycast(wallOrigin, wallDir, 0.25f, groundLayer);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null) return;
        if (sprite == null) return;

        if (spriteRenderer.sprite != sprite)
            spriteRenderer.sprite = sprite;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector2 top = (Vector2)transform.position + scanBoxOffset;
        Vector2 center = top + Vector2.down * (scanBoxSize.y * 0.5f);
        Gizmos.DrawWireCube(center, scanBoxSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadiusK);
    }
}