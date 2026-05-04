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
        _boss.IsVulnerable = false;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        _active = true;
        _done = false;
        _savedGravityScale = _boss.rb.gravityScale;
        _ = RunSequence();
    }

    async Awaitable RunSequence()
    {
        if (_boss.Player != null)
        {
            float dx = _boss.Player.transform.position.x - _boss.transform.position.x;
            float g = Mathf.Abs(Physics2D.gravity.y) * _boss.rb.gravityScale;
            float vy = Mathf.Sqrt(2f * g * _boss.jumpHeight);
            float vx = dx / (2f * vy / g);

            _boss.rb.linearVelocity = new Vector2(vx, vy);
            _boss.direction = (sbyte)Mathf.Sign(dx);
            _boss.UpdateVisualDirection();
        }

        while (_active && _boss.rb.linearVelocityY > 0f)
            await Awaitable.NextFrameAsync();
        if (!_active) return;

        _boss.rb.gravityScale = 0f;
        _boss.rb.linearVelocity = Vector2.zero;
        await Awaitable.WaitForSecondsAsync(_boss.slamWindupDuration);
        if (!_active) return;

        _boss.rb.gravityScale = _savedGravityScale;

        if (_boss.hitbox != null)
        {
            _boss.hitbox.damage = _boss.slamDamage;
            _boss.hitbox.gameObject.SetActive(true);
        }
        _boss.rb.linearVelocity = new Vector2(0f, -_boss.jumpHeight * 3f);

        while (_active)
        {
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
        _boss.rb.gravityScale = _savedGravityScale;
        _boss.rb.linearVelocityX = 0f;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
    }
}
