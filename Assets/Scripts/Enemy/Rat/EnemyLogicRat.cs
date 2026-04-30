using UnityEngine;
using System.Threading;

public class EnemyLogicRat : EnemyLogic
{
    enum RatAiState
    {
        Normal,
        Panic
    }

    [System.Serializable]
    class RatContextState
    {
        public float nextJumpTime;
        public float panicEndTime;
        public float currentPanicDirection;
        public bool isPanicking;
    }

    [Header("Rat: Jump Settings")]
    [SerializeField] float jumpForce = 6f;
    [SerializeField] float forwardForce = 4f;
    [SerializeField] float jumpInterval = 1.2f;

    [Header("Rat: Panic Settings")]
    [SerializeField] float panicSpeed = 8f;
    [SerializeField] float panicDuration = 5f;
    [SerializeField] float lowHealthThreshold = 0.3f;
    [SerializeField] float wallCheckDistance = 0.3f;

    [Header("Rat: Damage")]
    [SerializeField] ushort damage = 5;
    [SerializeField] float attackRadius = 0.5f;

    RatContextState rat = new RatContextState();
    RatAiState ratAiState = RatAiState.Normal;

    protected override void Awake()
    {
        base.Awake();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (entityCollider == null) entityCollider = GetComponent<Collider2D>();
        
        rat.nextJumpTime = Time.time + Random.value;
    }

    protected override void Update()
    {
        if (CurrentHealth <= 0) return;

        UpdateVisualDirection();

        if (HasVelocityOverride) return;

        // Проверка на переход в состояние паники
        if (ratAiState == RatAiState.Normal && (float)CurrentHealth / maxHealth < lowHealthThreshold)
        {
            StartPanic();
        }

        switch (ratAiState)
        {
            case RatAiState.Normal:
                HandleNormalBehavior();
                break;
            case RatAiState.Panic:
                HandlePanicBehavior();
                break;
        }
    }

    void HandleNormalBehavior()
    {
        // Движение к игроку (базовая логика из EnemyLogic)
        context.directionToPlayer = detection.IsPlayerDetected(this, out context.playerDistance);
        direction = context.directionToPlayer != 0 ? context.directionToPlayer : direction;

        if (Time.time >= rat.nextJumpTime)
        {
            PerformJump();
            rat.nextJumpTime = Time.time + jumpInterval;
        }

        // Мелкая корректировка горизонтальной скорости, чтобы не замирать
        float targetVx = direction * (Stats[StatType.Speed] * 0.5f);
        rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, targetVx, Time.deltaTime * 5f), rb.linearVelocity.y);
    }

    void PerformJump()
    {
        // Прыгаем в сторону игрока
        float jumpDir = direction;
        rb.linearVelocity = new Vector2(jumpDir * forwardForce, jumpForce);
    }

    void StartPanic()
    {
        ratAiState = RatAiState.Panic;
        rat.isPanicking = true;
        rat.panicEndTime = Time.time + panicDuration;
        
        // Начальное направление — от игрока
        if (context.directionToPlayer != 0)
        {
            rat.currentPanicDirection = -context.directionToPlayer;
        }
        else
        {
            rat.currentPanicDirection = Random.value > 0.5f ? 1 : -1;
        }

        // Возвращаемся в нормальное состояние через 5 секунд
        _ = EndPanicAfterDelay();
    }

    async Awaitable EndPanicAfterDelay()
    {
        await Awaitable.WaitForSecondsAsync(panicDuration);
        ratAiState = RatAiState.Normal;
        rat.isPanicking = false;
    }

    void HandlePanicBehavior()
    {
        // Быстрое движение в текущем направлении паники
        rb.linearVelocity = new Vector2(rat.currentPanicDirection * panicSpeed, rb.linearVelocity.y);

        // Проверка стены перед собой
        if (IsWallAhead(rat.currentPanicDirection))
        {
            rat.currentPanicDirection *= -1;
            // Небольшой импульс от стены
            rb.linearVelocity = new Vector2(rat.currentPanicDirection * panicSpeed, rb.linearVelocity.y + 2f);
        }
        
        // В панике крыса тоже может случайно подпрыгивать
        if (UnityEngine.Random.value < 0.02f && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.7f);
        }
    }

    bool IsWallAhead(float dir)
    {
        if (entityCollider == null) return false;

        Bounds b = entityCollider.bounds;
        float frontX = (dir > 0) ? b.max.x : b.min.x;
        Vector2 wallOrigin = new Vector2(frontX, b.center.y);
        Vector2 wallDir = new Vector2(dir, 0f);

        RaycastHit2D hit = Physics2D.Raycast(wallOrigin, wallDir, wallCheckDistance, detection.obstacleLayer);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    // Атака при столкновении или в радиусе во время прыжка
    void OnCollisionEnter2D(Collision2D col)
    {
        if (CurrentHealth <= 0) return;

        if (col.gameObject.CompareTag("Player"))
        {
            var player = col.gameObject.GetComponent<BaseEntity>();
            if (player != null)
            {
                player.TakeDamage(damage, transform, _name);
                
                // Отлетаем назад при ударе
                float knockDir = (transform.position.x - col.transform.position.x) > 0 ? 1 : -1;
                rb.linearVelocity = new Vector2(knockDir * 5f, jumpForce * 0.5f);
            }
        }
    }

    protected override void OnDeath()
    {
        rat.isPanicking = false;
        base.OnDeath();
    }
}
