using UnityEngine;

public class EnemyAIRat : EnemyAI
{
    [Header("Rat: Detection")]
    public float detectionRange = 6f;
    public LayerMask playerLayer;
    public LayerMask losBlockerLayer;

    [Header("Rat: Jump")]
    public float jumpForceX = 5f;
    public float jumpForceY = 7f;
    public float jumpInterval = 0.6f;
    public float aggroJumpInterval = 0.3f;
    public float panicSpeedMultiplier = 2f;

    [Header("Rat: Damage")]
    public ushort contactDamage = 8;
    public float damageCooldown = 0.5f;
    public LayerMask hitTargetLayer;

    [Header("Rat: Stun")]
    public Vector2 stunDurationRange = new Vector2(0.5f, 1f);

    [Header("Rat: Panic")]
    [Range(0f, 1f)] public float panicHealthThreshold = 0.3f;
    public float aggroSearchDuration = 3f;

    [Header("Rat: Ground")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;
    public float ledgeCheckDistance = 1.5f;

    public RatPatrolState PatrolState { get; private set; }
    public RatAggroState AggroState { get; private set; }
    public RatStunState StunState { get; private set; }
    public RatPanicState PanicState { get; private set; }

    public PlayerController Player { get; private set; }
    public bool IsGrounded { get; private set; }
    public float LastDamageDealtTime { get; set; }

    protected override void Awake()
    {
        base.Awake();

        direction = Random.value < 0.5f ? (sbyte)-1 : (sbyte)1;

        Player = PlayerController.instance;

        PatrolState = new RatPatrolState(this);
        AggroState = new RatAggroState(this);
        StunState = new RatStunState(this);
        PanicState = new RatPanicState(this);

        ChangeState(PatrolState);
    }

    protected override void Update()
    {
        if (Player == null) Player = PlayerController.instance;
        if (CurrentHealth <= 0) return;

        UpdateVisualDirection();
        CheckGround();
        base.Update();
    }

    void CheckGround()
    {
        Vector2 origin = entityCollider != null
            ? (Vector2)entityCollider.bounds.center
            : (Vector2)transform.position;
        origin.y -= entityCollider != null ? entityCollider.bounds.extents.y : 0f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        IsGrounded = hit.collider != null;
    }
    public bool HasGroundAhead(sbyte dir)
    {
        if (entityCollider == null) return true;

        float checkX = transform.position.x + dir * ledgeCheckDistance;
        float checkY = entityCollider.bounds.min.y;

        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(checkX, checkY),
            Vector2.down, 2f, groundLayer);

        return hit.collider != null;
    }

    public bool IsWallAhead(sbyte dir)
    {
        if (entityCollider == null) return false;
        Bounds b = entityCollider.bounds;
        float frontX = dir > 0 ? b.max.x : b.min.x;
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(frontX, b.center.y),
            new Vector2(dir, 0f), 0.15f, groundLayer);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    public bool CanSeePlayer()
    {
        if (Player == null) return false;
        float dist = Vector2.Distance(transform.position, Player.transform.position);
        if (dist > detectionRange) return false;

        float dx = Player.transform.position.x - transform.position.x;
        if (Mathf.Sign(dx) != direction) return false;

        Vector2 dir = ((Vector2)Player.transform.position - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, losBlockerLayer);
        return hit.collider == null;
    }

    public bool IsPanic => (float)CurrentHealth / Stats[StatType.MaxHealth] < panicHealthThreshold;

    public void Jump(float vx, float vy) => rb.linearVelocity = new Vector2(vx, vy);

    public void GoToPatrol() => ChangeState(PatrolState);
    public void GoToAggro() => ChangeState(AggroState);
    public void GoToPanic() => ChangeState(PanicState);

    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        if (attacker != null)
            direction = (sbyte)Mathf.Sign(attacker.position.x - transform.position.x);

        if (IsPanic)
        {
            ChangeState(PanicState);
            return;
        }

        float duration = Random.Range(stunDurationRange.x, stunDurationRange.y);
        ((RatStunState)StunState).SetDuration(duration);
        ChangeState(StunState);
    }

    protected override void OnDeath() => base.OnDeath();
}
