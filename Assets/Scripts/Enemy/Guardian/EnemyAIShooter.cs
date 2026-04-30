using UnityEngine;

public class EnemyAIShooter : EnemyAI
{
    [Header("Shooter: Detection")]
    public float detectionRange = 8f;
    public LayerMask losBlockerLayer;
    public LayerMask groundLayer;

    [Header("Shooter: Movement")]
    public float patrolSpeed = 3f;
    public float retreatSpeed = 2f;
    public float retreatDistance = 3f;
    public float ledgeCheckDistance = 0.5f;

    [Header("Shooter: Firing")]
    public float fireRate = 1.5f;
    public float lostTargetFireDuration = 2f;
    public GameObject bulletPrefab;
    public Transform muzzlePoint;
    public float bulletSpeed = 10f;
    public ushort bulletDamage = 8;

    [Header("Shooter: Aggro")]
    public float aggroSearchDuration = 3f;
    public Vector2 stunDurationRange = new Vector2(0.5f, 1.5f);

    // Cached states
    public ShooterPatrolState PatrolState { get; private set; }
    public ShooterAggroState AggroState { get; private set; }
    public ShooterStunState StunState { get; private set; }

    public PlayerController Player { get; private set; }
    public bool IsGrounded { get; private set; }

    static readonly int HashPatrol = Animator.StringToHash("isPatrol");
    static readonly int HashAggro = Animator.StringToHash("isAggro");
    static readonly int HashStun = Animator.StringToHash("isStun");

    public enum ShooterAnimState { Patrol, Aggro, Stun }

    protected override void Awake()
    {
        base.Awake();
        Player = PlayerController.instance;
        direction = Random.value < 0.5f ? (sbyte)-1 : (sbyte)1;
        PatrolState = new ShooterPatrolState(this);
        AggroState = new ShooterAggroState(this);
        StunState = new ShooterStunState(this);

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
        if (dy > 1f) return false; 

        Vector2 dir = ((Vector2)Player.transform.position - (Vector2)transform.position).normalized;
        return Physics2D.Raycast(transform.position, dir, dist, losBlockerLayer).collider == null;
    }

    public bool IsWallAhead(sbyte dir)
    {
        if (entityCollider == null) return false;
        Bounds b = entityCollider.bounds;
        float frontX = dir > 0 ? b.max.x : b.min.x;
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(frontX, b.center.y), new Vector2(dir, 0f), 0.15f, groundLayer);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    public bool HasGroundAhead(sbyte dir)
    {
        if (entityCollider == null) return true;
        float checkX = transform.position.x + dir * ledgeCheckDistance;
        float checkY = entityCollider.bounds.min.y;
        return Physics2D.Raycast(new Vector2(checkX, checkY), Vector2.down, 2f, groundLayer).collider != null;
    }

    public void Shoot()
    {
        if (bulletPrefab == null || muzzlePoint == null) return;
        var go = Object.Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.identity);
        var bullet = go.GetComponent<TurretBullet>();
        if (bullet != null)
            bullet.Init(direction, bulletSpeed, bulletDamage, this);
    }

    public void SetAnimation(ShooterAnimState anim)
    {
        if (animator == null) return;
        animator.SetBool(HashPatrol, anim == ShooterAnimState.Patrol);
        animator.SetBool(HashAggro, anim == ShooterAnimState.Aggro);
        animator.SetBool(HashStun, anim == ShooterAnimState.Stun);
    }

    public void GoToPatrol() => ChangeState(PatrolState);
    public void GoToAggro() => ChangeState(AggroState);

    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        if (attacker != null)
            direction = (sbyte)Mathf.Sign(attacker.position.x - transform.position.x);

        float duration = Random.Range(stunDurationRange.x, stunDurationRange.y);
        ((ShooterStunState)StunState).SetDuration(duration);
        ChangeState(StunState);
    }

    protected override void OnDeath() => base.OnDeath();
}
