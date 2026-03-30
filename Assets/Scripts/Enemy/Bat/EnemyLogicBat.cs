using UnityEngine;

public class EnemyLogicBat : EnemyLogic
{
    enum BatAiState
    {
        None,
        PreparingAttack,
        RisingBeforeDive,
        RisingToUncramp
    }

    [System.Serializable]
    class BatContextState
    {
        public Vector2 targetPoint;
        public bool hasTarget;

        public float nextScanTime;
        public float attackEndTime;
        public float waitOnGroundEndTime;
        public float riseEndTime;

        public float bobPhase;

        public float lastX;
        public float stuckTimer;

        public float prepareAttackEndTime;

        public float nextDamageTime;
        public bool damagedThisDive;

        public float requiredHeadClearance;

        public float desiredAltitudeAboveGround;
        public float postHitCooldownEndTime;

        public float repeatAfterHitTime;

        public float preDiveRiseTargetY;
        public float preDiveRiseEndTime;

        public float stunnedEndTime;
    }

    [Header("Bat: Scan (down to ground)")]
    [SerializeField] Vector2 scanBoxSize = new Vector2(3f, 6f);
    [SerializeField] Vector2 scanBoxOffset = new Vector2(0f, -0.25f);
    [SerializeField] LayerMask scanTargetLayer;
    [SerializeField] LayerMask groundLayer;

    [Header("Bat: Ceiling")]
    [SerializeField] LayerMask ceilingLayer;
    [SerializeField] float ceilingProbeDistance = 20f;

    [Header("Ground Probe")]
    [SerializeField] float groundProbeDistance = 50f;

    [Header("Bat: Scan Runtime")]
    [SerializeField] float scanTickInterval = 0.15f;

    [Header("Bat: Attack Delay")]
    [SerializeField] Vector2 prepareAttackDelayRange = new Vector2(1f, 2f);

    [Header("Bat: Patrol Height Constraints (above ground)")]
    [SerializeField] Vector2 desiredAltitudeAboveGroundRange = new Vector2(5f, 6f);
    [SerializeField] Vector2 requiredHeadClearanceRange = new Vector2(1f, 2f);
    [SerializeField] float minAltitudeAboveGround = 5f;

    [Header("Bat: Post-hit cooldown (cruise only)")]
    [SerializeField] Vector2 postHitCooldownRange = new Vector2(3f, 5f);

    [Header("Bat: Stun/Contusion after taking damage")]
    [SerializeField] Vector2 stunDurationRange = new Vector2(2f, 3f);

    [Header("Bat: Pre-dive rise")]
    [SerializeField] Vector2 preDiveRiseUnitsRange = new Vector2(1f, 3f);
    [SerializeField] float preDiveRiseTimeout = 0.75f;
    [SerializeField] float preDiveRiseTolerance = 0.05f;

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
    [SerializeField] float missWaitSeconds = 1.5f;
    [SerializeField] float riseSecondsP = 1.0f;
    [SerializeField] float reattackDelayAfterHit = 1.0f;

    [Header("Bat: Damage")]
    [SerializeField] ushort damage = 10;

    [Header("Bat: Stomp (������ ������)")]
    [SerializeField] LayerMask playerLayer;
    [SerializeField] ushort stompDamage = 5;
    [SerializeField] float stompKnockbackX = 12f;
    [SerializeField] float stompKnockbackY = 4f;

    [Header("Bat: Sprites")]
    [SerializeField] Sprite spriteA_Idle;
    [SerializeField] Sprite spriteB_Attack;
    [SerializeField] Sprite spriteC_MissWait;

    [Header("Bat: Separation (anti-clumping)")]
    [SerializeField] LayerMask batLayer;
    [SerializeField] float separationRadius = 0.75f;
    [SerializeField] float separationStrength = 3.0f;

    [Header("Bat: Stun Drift (anti-stuck)")]
    [SerializeField] float stunDriftUpSpeed = 2.5f;
    [SerializeField] float stunDriftSideSpeed = 1.5f;

    [Header("Bat: Side clearance (anti-cramped)")]
    [SerializeField] LayerMask sideObstacleLayer;
    [SerializeField] float sideProbeDistance = 15f;
    [SerializeField] float minSideClearance = 10f;
    [SerializeField] float crampedRiseUnits = 2.0f;
    [SerializeField] float crampedRiseCooldown = 1.0f;

    [Header("Bat: Targeting (LoS + leash)")]
    [SerializeField] LayerMask losBlockerLayer;
    [SerializeField, Min(1f)] float maxRoamDistanceFromSpawn = 18f;
    [SerializeField, Min(0.1f)] float leashReturnMargin = 1.0f;

    [Header("Audio manager")]
    [SerializeField] private BatAudio batAudio;

    static readonly Collider2D[] separationHits = new Collider2D[16];
    static readonly Collider2D[] scanHits = new Collider2D[32];

    BatContextState bat = new BatContextState();
    BatAiState batAiState = BatAiState.None;

    float patrolBaseY;
    float nextCrampedCheckTime;
    float nextAltitudeUpdateTime;
    const float altitudeUpdateInterval = 0.2f;

    Vector2 spawnAnchor;

    float PatrolTargetY =>
        patrolBaseY + Mathf.Sin((Time.time + bat.bobPhase) * (bobFrequency * Mathf.PI * 2f)) * bobAmplitude;

    protected override void Awake()
    {
        base.Awake();

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (entityCollider == null) entityCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (ceilingLayer.value == 0) ceilingLayer = groundLayer;

        spawnAnchor = transform.position;

        bat.bobPhase = Random.value * 10f;

        bat.desiredAltitudeAboveGround = Random.Range(desiredAltitudeAboveGroundRange.x, desiredAltitudeAboveGroundRange.y);
        bat.requiredHeadClearance = Random.Range(requiredHeadClearanceRange.x, requiredHeadClearanceRange.y);

        bat.nextScanTime = Time.time;
        bat.lastX = transform.position.x;

        patrolBaseY = PickInitialPatrolY();
        currentState = EnemyState.FlyingPatrol;
    }

    bool IsOutsideLeash()
    {
        return Vector2.Distance(transform.position, spawnAnchor) > maxRoamDistanceFromSpawn;
    }

    bool IsWithinLeash(Vector2 point)
    {
        return Vector2.Distance(point, spawnAnchor) <= maxRoamDistanceFromSpawn;
    }

    bool HasLineOfSightTo(Transform target)
    {
        if (target == null) return false;

        LayerMask mask = (losBlockerLayer.value != 0) ? losBlockerLayer : groundLayer;

        Vector2 from = entityCollider != null ? entityCollider.bounds.center : (Vector2)transform.position;

        Collider2D targetCol = target.GetComponentInChildren<Collider2D>();
        Vector2 to = targetCol != null ? targetCol.bounds.center : (Vector2)target.position;

        Vector2 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;

        RaycastHit2D hit = Physics2D.Raycast(from, dir / dist, dist, mask);
        return hit.collider == null || hit.collider.isTrigger;
    }

    float PickInitialPatrolY()
    {
        if (!TryGetGroundPointBelowProbed(out Vector2 ground))
            return transform.position.y;

        return ground.y + GetAllowedCruisingAltitudeAboveGround();
    }

    void Update()
    {
        if (CurrentHealth <= 0) return;

        UpdateVisualDirection();

        if (Time.time < bat.stunnedEndTime)
        {
            if (HasVelocityOverride) return;

            float vx = direction * stunDriftSideSpeed;
            float vy = stunDriftUpSpeed;

            Vector2 sep = Vector2.ClampMagnitude(ComputeSeparationVector() * separationStrength,
                                                  Stats[StatType.Speed] * 1.5f);
            vx += sep.x;
            vy += sep.y;

            rb.linearVelocity = new Vector2(vx, Mathf.Clamp(vy, -maxVerticalSpeed, maxVerticalSpeed));
            return;
        }

        if (HasVelocityOverride)
            return;

        if (Time.time < bat.postHitCooldownEndTime
            || Time.time < bat.repeatAfterHitTime)
        {
            SetSprite(spriteA_Idle);
            EnsureMinAltitudeAlways();
            PatrolTick();
            return;
        }

        switch (currentState)
        {
            case EnemyState.FlyingPatrol:
                SetSprite(spriteA_Idle);
                EnsureMinAltitudeAlways();

                if (batAiState == BatAiState.None)
                    TryCrampedRise();

                if (Time.time >= bat.nextScanTime && batAiState == BatAiState.None)
                {
                    bat.nextScanTime = Time.time + scanTickInterval;
                    TryAcquireTargetFromDownScan();
                }

                if (batAiState == BatAiState.PreparingAttack)
                {
                    float prepDriftX = 0f;
                    if (bat.hasTarget)
                    {
                        float dx = bat.targetPoint.x - transform.position.x;
                        if (Mathf.Abs(dx) > 0.15f)
                            prepDriftX = Mathf.Sign(dx) * Stats[StatType.Speed] * patrolSpeedMultiplier * 0.4f;
                    }
                    rb.linearVelocity = new Vector2(prepDriftX, VerticalSeek(PatrolTargetY, verticalFollowSpeed));

                    if (Time.time >= bat.prepareAttackEndTime)
                        BeginPreDiveRise();
                    break;
                }

                if (batAiState == BatAiState.RisingBeforeDive)
                {
                    float riseDriftX = 0f;
                    if (bat.hasTarget)
                    {
                        float dx = bat.targetPoint.x - transform.position.x;
                        if (Mathf.Abs(dx) > 0.15f)
                            riseDriftX = Mathf.Sign(dx) * Stats[StatType.Speed] * patrolSpeedMultiplier * 0.3f;
                    }
                    rb.linearVelocity = new Vector2(riseDriftX, VerticalSeek(bat.preDiveRiseTargetY, verticalFollowSpeed));

                    bool reached = Mathf.Abs(transform.position.y - bat.preDiveRiseTargetY) <= preDiveRiseTolerance;
                    bool timedOut = Time.time >= bat.preDiveRiseEndTime;

                    if (reached || timedOut)
                    {
                        batAiState = BatAiState.None;
                        bat.attackEndTime = Time.time + attackChaseSecondsN;
                        bat.damagedThisDive = false;
                        currentState = EnemyState.DivingToPoint;
                    }
                    break;
                }

                if (batAiState == BatAiState.RisingToUncramp)
                {
                    float uncrampVx = direction * Stats[StatType.Speed] * patrolSpeedMultiplier * 0.5f;
                    rb.linearVelocity = new Vector2(
                        uncrampVx,
                        VerticalSeek(bat.preDiveRiseTargetY, verticalFollowSpeed));

                    bool reached = Mathf.Abs(transform.position.y - bat.preDiveRiseTargetY) <= preDiveRiseTolerance;
                    bool timedOut = Time.time >= bat.preDiveRiseEndTime;

                    if (reached || timedOut)
                        batAiState = BatAiState.None;
                    break;
                }

                PatrolTick();
                break;

            case EnemyState.DivingToPoint:
                SetSprite(spriteB_Attack);

                if (IsOutsideLeash())
                {
                    batAiState = BatAiState.None;
                    bat.hasTarget = false;
                    rb.linearVelocity = new Vector2(0f, riseSpeed);
                    currentState = EnemyState.RisingBack;
                    bat.riseEndTime = Time.time + riseSecondsP;
                    break;
                }

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
                EnsureMinAltitudeAlways();
                RiseTick();
                break;

            default:
                currentState = EnemyState.FlyingPatrol;
                break;
        }
    }

    void EnsureMinAltitudeAlways()
    {
        if (Time.time < nextAltitudeUpdateTime) return;
        nextAltitudeUpdateTime = Time.time + altitudeUpdateInterval;

        if (!TryGetGroundPointBelowProbed(out Vector2 ground)) return;

        float desiredAltitude = Mathf.Max(minAltitudeAboveGround, GetAllowedCruisingAltitudeAboveGround());
        patrolBaseY = ground.y + desiredAltitude;
    }

    float GetAllowedCruisingAltitudeAboveGround()
    {
        float desired = Mathf.Max(minAltitudeAboveGround, bat.desiredAltitudeAboveGround);

        if (TryGetCeilingDistance(out float ceilingDist))
        {
            float maxByCeiling = Mathf.Max(0f, ceilingDist - bat.requiredHeadClearance);
            desired = Mathf.Min(desired, maxByCeiling);
        }

        return desired;
    }

    bool TryGetCeilingDistance(out float ceilingDistance)
    {
        Vector2 origin = transform.position;
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

    void BeginPreDiveRise()
    {
        float riseUnits = Random.Range(preDiveRiseUnitsRange.x, preDiveRiseUnitsRange.y);
        float targetY = transform.position.y + riseUnits;

        if (TryGetCeilingDistance(out float ceilingDist))
        {
            float maxUp = Mathf.Max(0f, ceilingDist - bat.requiredHeadClearance);
            targetY = Mathf.Min(targetY, transform.position.y + maxUp);
        }

        bat.preDiveRiseTargetY = targetY;
        bat.preDiveRiseEndTime = Time.time + preDiveRiseTimeout;
        batAiState = BatAiState.RisingBeforeDive;
    }

    void PatrolTick()
    {
        float speedX = Stats[StatType.Speed] * patrolSpeedMultiplier;
        float vx = direction * speedX;

        float vy = VerticalSeek(PatrolTargetY, verticalFollowSpeed);

        Vector2 sep = Vector2.ClampMagnitude(ComputeSeparationVector() * separationStrength,
                                              Stats[StatType.Speed] * 1.5f);
        vx += sep.x;
        vy += sep.y;

        Vector2 v = new Vector2(vx, vy);
        ApplyLeashSteering(ref v, Stats[StatType.Speed] * patrolSpeedMultiplier);

        rb.linearVelocity = v;

        if (IsWallAhead(direction))
        {
            direction *= -1;
            rb.linearVelocity = new Vector2(direction * speedX, rb.linearVelocity.y);
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

        int count = Physics2D.OverlapBoxNonAlloc(center, size, 0f, scanHits, scanTargetLayer);
        if (count == 0) return;

        Transform best = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            var h = scanHits[i];
            if (h == null) continue;
            if (h.transform == transform) continue;

            if (!IsWithinLeash(h.transform.position)) continue;
            if (!HasLineOfSightTo(h.transform)) continue;

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

        if (!IsWithinLeash(bat.targetPoint))
        {
            bat.hasTarget = false;
            bat.riseEndTime = Time.time + riseSecondsP;
            currentState = EnemyState.RisingBack;
            return;
        }

        Vector2 pos = transform.position;
        Vector2 to = bat.targetPoint - pos;

        float targetVx = Mathf.Abs(to.x) > 0.1f
            ? Mathf.Sign(to.x) * Stats[StatType.Speed] * chaseSpeedMultiplier
            : 0f;

        float vx = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetVx,
            Stats[StatType.Speed] * chaseSpeedMultiplier * 8f * Time.deltaTime);

        float vy = -Mathf.Abs(diveSpeed);

        rb.linearVelocity = new Vector2(vx, vy);

        if (!bat.damagedThisDive && Time.time >= bat.nextDamageTime && TryDamagePlayerInRadiusK())
        {
            bat.damagedThisDive = true;
            bat.nextDamageTime = Time.time + attackHitCooldown;

            rb.linearVelocity = new Vector2(0f, riseSpeed * 0.75f);

            bat.hasTarget = false;
            bat.postHitCooldownEndTime = Time.time + Random.Range(postHitCooldownRange.x, postHitCooldownRange.y);
            bat.repeatAfterHitTime = Time.time + reattackDelayAfterHit;

            currentState = EnemyState.FlyingPatrol;
            return;
        }

        float dynamicGroundCheck = Mathf.Max(groundCheckDistance, Mathf.Abs(diveSpeed) * Time.deltaTime * 2f);
        Vector2 groundOrigin = transform.position;
        if (entityCollider != null)
            groundOrigin.y = entityCollider.bounds.min.y;

        RaycastHit2D groundHit = Physics2D.Raycast(groundOrigin, Vector2.down, dynamicGroundCheck, groundLayer);
        if (groundHit.collider == null) return;

        rb.linearVelocity = Vector2.zero;
        BeginMissWait();
    }

    void BeginMissWait()
    {
        batAiState = BatAiState.None;
        bat.hasTarget = false;
        bat.waitOnGroundEndTime = Time.time + missWaitSeconds;
        currentState = EnemyState.WaitingOnGround;
    }

    bool TryDamagePlayerInRadiusK()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, attackRadiusK, separationHits, scanTargetLayer);
        if (count == 0) return false;

        for (int i = 0; i < count; i++)
        {
            var h = separationHits[i];
            if (h == null) continue;

            var entity = h.GetComponentInParent<BaseEntity>();
            if (entity == null || entity == this) continue;

            entity.TakeDamage(damage, transform, _name);
            return true;
        }

        return false;
    }

    void RiseTick()
    {
        float targetY = PatrolTargetY;
        float vy = VerticalSeek(targetY, verticalFollowSpeed);

        float speedX = direction * Stats[StatType.Speed] * patrolSpeedMultiplier * 0.5f;

        Vector2 sep = Vector2.ClampMagnitude(ComputeSeparationVector() * separationStrength, Stats[StatType.Speed] * 1.5f);
        speedX += sep.x;
        vy += sep.y;

        Vector2 v = new Vector2(speedX, vy);
        ApplyLeashSteering(ref v, Stats[StatType.Speed] * patrolSpeedMultiplier);
        rb.linearVelocity = v;

        float dy = Mathf.Abs(patrolBaseY - transform.position.y);
        if (dy <= 0.15f || Time.time >= bat.riseEndTime)
            currentState = EnemyState.FlyingPatrol;

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

    bool IsCrampedSideways()
    {
        LayerMask mask = (sideObstacleLayer.value != 0) ? sideObstacleLayer : groundLayer;

        Vector2 origin = transform.position;
        if (entityCollider != null)
            origin = entityCollider.bounds.center;

        float left = ProbeSide(origin, Vector2.left, sideProbeDistance, mask);
        float right = ProbeSide(origin, Vector2.right, sideProbeDistance, mask);

        return left < minSideClearance && right < minSideClearance;
    }

    float ProbeSide(Vector2 origin, Vector2 dir, float dist, LayerMask mask)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, mask);
        if (hit.collider == null) return dist;
        if (hit.collider.isTrigger) return dist;
        return hit.distance;
    }

    void TryCrampedRise()
    {
        if (Time.time < nextCrampedCheckTime) return;
        nextCrampedCheckTime = Time.time + crampedRiseCooldown;

        if (batAiState != BatAiState.None) return;
        if (!IsCrampedSideways()) return;

        float targetY = transform.position.y + crampedRiseUnits;

        if (TryGetCeilingDistance(out float ceilingDist))
        {
            float maxUp = Mathf.Max(0f, ceilingDist - bat.requiredHeadClearance);
            targetY = Mathf.Min(targetY, transform.position.y + maxUp);
        }

        bat.preDiveRiseTargetY = targetY;
        bat.preDiveRiseEndTime = Time.time + preDiveRiseTimeout;
        batAiState = BatAiState.RisingToUncramp;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (CurrentHealth <= 0) return;
        if ((playerLayer.value & (1 << col.gameObject.layer)) == 0) return;

        Vector2 normal = col.contacts[0].normal;
        if (normal.y > -0.6f) return;

        TakeDamage(stompDamage, col.transform, "stomp");

        LayerMask obstacleMask = sideObstacleLayer.value != 0 ? sideObstacleLayer : groundLayer;
        Vector2 origin = entityCollider != null ? entityCollider.bounds.center : (Vector2)transform.position;

        float leftDist = ProbeSide(origin, Vector2.left, sideProbeDistance, obstacleMask);
        float rightDist = ProbeSide(origin, Vector2.right, sideProbeDistance, obstacleMask);

        float knockDir = leftDist > rightDist ? -1f : 1f;
        rb.linearVelocity = new Vector2(knockDir * stompKnockbackX, stompKnockbackY);
        direction = (sbyte)(knockDir > 0 ? 1 : -1);
    }

    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        bat.stunnedEndTime = Time.time + Random.Range(stunDurationRange.x, stunDurationRange.y);

        batAiState = BatAiState.None;
        bat.hasTarget = false;

        currentState = EnemyState.RisingBack;
        bat.riseEndTime = bat.stunnedEndTime + riseSecondsP;
    }

    protected override void OnDeath()
    {
        batAiState = BatAiState.None;
        bat.hasTarget = false;

        base.OnDeath();
    }

    void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null || sprite == null) return;
        if (spriteRenderer.sprite != sprite)
            spriteRenderer.sprite = sprite;
    }

    Vector2 ComputeSeparationVector()
    {
        if (batLayer.value == 0) return Vector2.zero;

        int count = Physics2D.OverlapCircleNonAlloc(transform.position, separationRadius, separationHits, batLayer);
        if (count <= 0) return Vector2.zero;

        Vector2 push = Vector2.zero;
        Vector2 myPos = transform.position;

        for (int i = 0; i < count; i++)
        {
            var col = separationHits[i];
            if (col == null) continue;
            if (col.transform == transform || col.transform.IsChildOf(transform)) continue;

            var otherBat = col.GetComponentInParent<EnemyLogicBat>();
            if (otherBat == null || otherBat == this) continue;

            Vector2 toMe = myPos - (Vector2)otherBat.transform.position;
            float dist = toMe.magnitude;
            if (dist <= 0.0001f) continue;

            float t = 1f - Mathf.Clamp01(dist / separationRadius);
            push += (toMe / dist) * t;
        }

        return push;
    }

    void ApplyLeashSteering(ref Vector2 velocity, float strength)
    {
        Vector2 pos = transform.position;
        Vector2 toCenter = spawnAnchor - pos;

        float dist = toCenter.magnitude;
        if (dist <= 0.001f) return;

        float over = dist - maxRoamDistanceFromSpawn;
        if (over <= 0f) return;

        Vector2 dir = toCenter / dist;
        float k = Mathf.Clamp01(over / Mathf.Max(0.01f, leashReturnMargin));
        velocity += dir * (strength * k);

        float awayDot = Vector2.Dot(velocity, -dir);
        if (awayDot > 0f)
            velocity += dir * awayDot;
    }

    Vector2 GetLeashCenter()
    {
        return spawnAnchor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnAnchor, maxRoamDistanceFromSpawn);
    }
}