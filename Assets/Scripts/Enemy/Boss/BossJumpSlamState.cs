using UnityEngine;

public class BossJumpSlamState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    bool _active;
    bool _done;
    float _savedGravityScale;

    public BossJumpSlamState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.IsVulnerable = true;
        _boss.IsParryable = false;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        _active = true;
        _done = false;
        _savedGravityScale = _boss.rb.gravityScale;
        _ = RunSequence();
    }

    async Awaitable RunSequence()
    {
        if (!_active) return;

        _boss.IsParryable = false;

        // Jump calculation
        if (_boss.Player != null)
        {
            float dx = _boss.Player.transform.position.x - _boss.transform.position.x;
            float g = Mathf.Abs(Physics2D.gravity.y) * _boss.rb.gravityScale;
            float vy = Mathf.Sqrt(2f * g * _boss.jumpHeight);
            float tApex = vy / g;
            float vx = dx / tApex;

            _boss.rb.linearVelocity = new Vector2(vx, vy);
            _boss.direction = (sbyte)Mathf.Sign(dx);
            _boss.UpdateVisualDirection();
        }

        // Wait for apex
        while (_active && _boss.rb.linearVelocityY > 0f)
            await Awaitable.NextFrameAsync();
        if (!_active) return;

        // Optional hang midair (can be set to 0 to remove apex pause entirely)
        if (_boss.apexHangDuration > 0f)
        {
            _boss.rb.gravityScale = 0f;
            _boss.rb.linearVelocity = Vector2.zero;
            await Awaitable.WaitForSecondsAsync(_boss.apexHangDuration);
            if (!_active) return;
        }

        _boss.IsParryable = true;

        _boss.rb.gravityScale = _savedGravityScale;
        _boss.rb.linearVelocity = new Vector2(0f, -_boss.jumpHeight * 5f);

        // Wait for landing
        while (_active)
        {
            _boss.IsParryable = true;

            Vector2 origin = _boss.entityCollider != null
                ? (Vector2)_boss.entityCollider.bounds.center
                  - new Vector2(0f, _boss.entityCollider.bounds.extents.y)
                : (Vector2)_boss.transform.position;
             
            if (Physics2D.Raycast(origin, Vector2.down, 0.25f, _boss.groundLayer).collider != null)
            {
                _boss.rb.linearVelocity = Vector2.zero;
                break;
            }
            await Awaitable.NextFrameAsync();
        }

        if (!_active) return;
        _boss.IsParryable = false;

        // Execute slam
        if (_boss.hitbox != null)
        {
            _boss.hitbox.damage = _boss.slamDamage;
            _boss.hitbox.gameObject.SetActive(true);

            // Allow the hitbox to persist briefly
            await Awaitable.WaitForSecondsAsync(0.15f);
            if (!_active) return;

            _boss.hitbox.gameObject.SetActive(false);
        }

        _done = true;
    }

    public void Update()
    {
        if (!_done) return;
        _done = false;
        _boss.GoToVulnerable();
    } 

    public void Exit()
    {
        _active = false;
        _boss.IsParryable = false;
        _boss.rb.gravityScale = _savedGravityScale;
        _boss.rb.linearVelocityX = 0f;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
    }
}
