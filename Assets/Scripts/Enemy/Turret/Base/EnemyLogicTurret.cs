using UnityEngine;

public class EnemyLogicTurret : EnemyLogic
{
    enum TurretState { Idle, Detected, Firing, LostTarget, Stunned }

    [Header("Turret: Detection")]
    [SerializeField] float detectionRange = 12f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask losBlockerLayer;
    [SerializeField] LayerMask groundLayer;

    [Header("Turret: Firing")]
    [SerializeField] float detectionDelay = 2f;
    [SerializeField] float fireRate = 2f;
    [SerializeField] float lostTargetFireDuration = 3f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform muzzlePoint;
    [SerializeField] float bulletSpeed = 10f;
    [SerializeField] ushort bulletDamage = 10;

    [Header("Turret: Stun")]
    [SerializeField] Vector2 stunDurationRange = new Vector2(1.5f, 2.5f);

    TurretState turretState = TurretState.Idle;
    float detectionTimer;
    float fireTimer;
    float lostTargetTimer;
    float stunnedEndTime;
    bool isGrounded;

    PlayerController player;

    protected override void Awake()
    {
        base.Awake();
        player = PlayerController.instance;
        PickInitialDirection();
    }

    void PickInitialDirection()
    {
        sbyte first = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;
        if (!IsWallTooClose(first))
            direction = first;
        else if (!IsWallTooClose((sbyte)-first))
            direction = (sbyte)-first;
        else
            direction = first;

        UpdateVisualDirection();
    }

    bool IsWallTooClose(sbyte dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(dir, 0f), 15f, losBlockerLayer);
        return hit.collider != null && hit.distance < 15f;
    }

    void Update()
    {
        if (player == null) player = PlayerController.instance;
        if (CurrentHealth <= 0) return;
        if (!isGrounded) return;

        if (turretState == TurretState.Stunned)
        {
            if (Time.time >= stunnedEndTime)
                turretState = TurretState.Idle;
            return;
        }

        bool sees = CanSeePlayer();

        switch (turretState)
        {
            case TurretState.Idle:
                if (sees)
                {
                    turretState = TurretState.Detected;
                    detectionTimer = detectionDelay;
                }
                break;

            case TurretState.Detected:
                if (!sees) { turretState = TurretState.Idle; break; }
                detectionTimer -= Time.deltaTime;
                if (detectionTimer <= 0f)
                {
                    turretState = TurretState.Firing;
                    fireTimer = 0f;
                }
                break;

            case TurretState.Firing:
                fireTimer -= Time.deltaTime;
                if (fireTimer <= 0f)
                {
                    Shoot();
                    fireTimer = 1f / fireRate;
                }
                if (!sees)
                {
                    turretState = TurretState.LostTarget;
                    lostTargetTimer = lostTargetFireDuration;
                }
                break;

            case TurretState.LostTarget:
                fireTimer -= Time.deltaTime;
                if (fireTimer <= 0f)
                {
                    Shoot();
                    fireTimer = 1f / fireRate;
                }
                lostTargetTimer -= Time.deltaTime;
                if (lostTargetTimer <= 0f)
                    turretState = TurretState.Idle;
                if (sees)
                    turretState = TurretState.Firing;
                break;
        }
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (!isGrounded && ((1 << col.gameObject.layer) & groundLayer.value) != 0)
        {
            isGrounded = true;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
    bool CanSeePlayer()
    {
        if (player == null) return false;
        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist > detectionRange) return false;

        float dx = player.transform.position.x - transform.position.x;
        if (Mathf.Sign(dx) != direction) return false;

        Vector2 dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, losBlockerLayer);
        return hit.collider == null;
    }

    void FacePlayer()
    {
        if (player == null) return;
        direction = (sbyte)(player.transform.position.x > transform.position.x ? 1 : -1);
        UpdateVisualDirection();
    }

    void Shoot()
    {
        if (bulletPrefab == null || muzzlePoint == null) return;

        var go = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.identity);
        var bullet = go.GetComponent<TurretBullet>();
        if (bullet != null)
            bullet.Init(direction, bulletSpeed, bulletDamage, this);
    }

    protected override void OnDamageReceived(ushort amount, Transform attacker = null)
    {
        turretState = TurretState.Stunned;
        stunnedEndTime = Time.time + Random.Range(stunDurationRange.x, stunDurationRange.y);
    }

    protected override void OnDeath()
    {
        VisualEffectsManager.SpawnDebris(spriteRenderer.sprite.texture, transform.position + new Vector3(0f, 0.5f, 0f), 10);
        base.OnDeath();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up, new Vector3(direction, 0, 0f) * 3f);
    }
}
