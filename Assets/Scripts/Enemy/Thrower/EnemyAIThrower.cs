using UnityEngine;

public class EnemyAIThrower : EnemyAI
{
    [Header("Thrower: Detection")]
    public float detectionRange = 8f;
    public float sameFloorThreshold = 1.5f;
    public LayerMask losBlockerLayer;
    public LayerMask groundLayer;

    [Header("Thrower: Movement")]
    public float patrolSpeed = 2.5f;
    public float fleeSpeed = 5f;
    public float fleeDuration = 2.5f;
    public float tooCloseDistance = 3f;
    public float ledgeCheckDistance = 0.5f;

    [Header("Thrower: Throw")]
    public float jumpBackForce = 4f;
    public float jumpBackForceY = 5f;
    public float flightTime = 1.2f;
    public float throwCooldown = 3f;
    public ushort vialDamage = 12;
    public GameObject vialPrefab;
    public Transform throwPoint;

    [Header("Thrower: Stun")]
    public Vector2 stunDurationRange = new Vector2(0.5f, 1.5f);

    public ThrowerPatrolState PatrolState { get; private set; }
    public ThrowerAggroState AggroState { get; private set; }
    public ThrowerFleeState FleeState { get; private set; }
    public ThrowerStunState StunState { get; private set; }

    public PlayerController Player { get; private set; }
    public bool IsGrounded { get; private set; }
    public float LastThrowTime { get; set; } = -999f;

    static readonly int HashPatrol = Animator.StringToHash("isPatrol");
    static readonly int HashAggro = Animator.StringToHash("isAggro");
    static readonly int HashFlee = Animator.StringToHash("isFlee");
    static readonly int HashStun = Animator.StringToHash("isStun");

    public enum ThrowerAnimState { Patrol, Aggro, Flee, Stun }

    protected override void Awake()
    {
        base.Awake();
        Player = PlayerController.instance;
        direction = Random.value < 0.5f ? (sbyte)-1 : (sbyte)1;

        PatrolState = new ThrowerPatrolState(this);
        AggroState = new ThrowerAggroState(this);
        FleeState = new ThrowerFleeState(this);
        StunState = new ThrowerStunState(this);

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
        IsGrounded = Physics2D.Raycast(origin, Vector2.down, 0.2f, groundLayer).collider != null;
    }

    public bool CanSeePlayer()
    {
        if (Player == null) return false;
        float dist = Vector2.Distance(transform.position, Player.transform.position);
        if (dist > detectionRange) return false;

        float dy = Mathf.Abs(Player.transform.position.y - transform.position.y);
        if (dy > sameFloorThreshold) return false;

        Vector2 dir = ((Vector2)Player.transform.position - (Vector2)transform.position).normalized;
        return Physics2D.Raycast(transform.position, dir, dist, losBlockerLayer).collider == null;
    }

    public bool IsPlayerTooClose()
    {
        if (Player == null) return false;
        return Vector2.Distance(transform.position, Player.transform.position) < tooCloseDistance;
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

    public bool HasGroundAhead(sbyte dir)
    {
        if (entityCollider == null) return true;
        float checkX = transform.position.x + dir * ledgeCheckDistance;
        float checkY = entityCollider.bounds.min.y;
        return Physics2D.Raycast(new Vector2(checkX, checkY), Vector2.down, 2f, groundLayer).collider != null;
    }

    public void Throw()
    {
        if (vialPrefab == null || Player == null) return;

        Vector2 from = throwPoint != null ? (Vector2)throwPoint.position : (Vector2)transform.position;
        Vector2 to = Player.transform.position;

        float g = Mathf.Abs(Physics2D.gravity.y);
        float dx = to.x - from.x;
        float dy = to.y - from.y;
        float vx = dx / flightTime;
        float vy = (dy + 0.5f * g * flightTime * flightTime) / flightTime;

        var go = Instantiate(vialPrefab, from, Quaternion.identity);
        var vial = go.GetComponent<Vial>();
        if (vial != null)
            vial.Init(new Vector2(vx, vy), vialDamage, this);

        LastThrowTime = Time.time;
    }

    public void JumpBack()
    {
        float jumpDir = -direction;
        rb.linearVelocity = new Vector2(jumpDir * jumpBackForce, jumpBackForceY);
    }

    public void SetAnimation(ThrowerAnimState anim)
    {
        if (animator == null) return;
        animator.SetBool(HashPatrol, anim == ThrowerAnimState.Patrol);
        animator.SetBool(HashAggro, anim == ThrowerAnimState.Aggro);
        animator.SetBool(HashFlee, anim == ThrowerAnimState.Flee);
        animator.SetBool(HashStun, anim == ThrowerAnimState.Stun);
    }

    public void GoToPatrol() => ChangeState(PatrolState);
    public void GoToAggro() => ChangeState(AggroState);
    public void GoToFlee() => ChangeState(FleeState);

    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        if (attacker != null)
            direction = (sbyte)Mathf.Sign(attacker.position.x - transform.position.x);

        float duration = Random.Range(stunDurationRange.x, stunDurationRange.y);
        ((ThrowerStunState)StunState).SetDuration(duration);
        ChangeState(StunState);
    }

    protected override void OnDeath() => base.OnDeath();
}
