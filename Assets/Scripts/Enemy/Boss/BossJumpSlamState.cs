using UnityEngine;

public class BossJumpSlamState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    bool _active;
    bool _done;
    float _savedGravityScale;
    bool _hitRegistered;

    public BossJumpSlamState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.IsParryable = false;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        _active = true;
        _done = false;
        _hitRegistered = false;
        _savedGravityScale = _boss.rb.gravityScale;
        _ = RunSequence();
    }

    public void RegisterHit() => _hitRegistered = true;


    async Awaitable RunSequence()
    {
        if (!_active) return;

        _boss.IsParryable = false;

        // Jump calculation: Beautiful Curve
        if (_boss.Player != null)
        {
            float dx = _boss.Player.transform.position.x - _boss.transform.position.x;
            float g = Mathf.Abs(Physics2D.gravity.y) * _boss.rb.gravityScale;
            
            // Vertical velocity to reach jumpHeight
            float vy = Mathf.Sqrt(2f * g * _boss.jumpHeight);
            
            // Total time in air: t_up + t_down. 
            // Assuming landing height approx equals starting height.
            float tApex = vy / g;
            float tTotal = tApex * 2f; 
            
            // Horizontal velocity to reach player at the moment of impact
            float vx = dx / tTotal;

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

        // Start descent: Open parry window
        _boss.IsParryable = true;
        _boss.rb.gravityScale = _savedGravityScale;
        
        // PRESERVE Vx: Add downward boost instead of resetting to zero
        float currentVx = _boss.rb.linearVelocityX;
        _boss.rb.linearVelocity = new Vector2(currentVx, -_boss.jumpHeight * 2f);

        // Wait for landing
        float fallTimeout = 2.0f; 
        float fallTimer = 0f;
        while (_active && fallTimer < fallTimeout)
        {
            fallTimer += Time.deltaTime;
            Vector2 origin = _boss.entityCollider != null
                ? (Vector2)_boss.entityCollider.bounds.center
                  - new Vector2(0f, _boss.entityCollider.bounds.extents.y)
                : (Vector2)_boss.transform.position;
             
            if (Physics2D.Raycast(origin, Vector2.down, 0.3f, _boss.groundLayer).collider != null)
            {
                _boss.rb.linearVelocity = Vector2.zero;
                break;
            }
            await Awaitable.NextFrameAsync();
        }

        // CRITICAL: Check if we were parried/interrupted during the fall
        if (!_active) return;

        // Execute slam: Keep IsParryable = true to allow parrying the impact itself
        if (_boss.hitbox != null)
        {
            _boss.hitbox.damage = _boss.slamDamage;
            _boss.hitbox.gameObject.SetActive(true);

            // Allow the hitbox to persist briefly
            await Awaitable.WaitForSecondsAsync(0.15f);
            
            // Ensure we cleanup even if the state changed during the 0.15s wait
            if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        }

        if (!_active) return;
        _boss.IsParryable = false;
        _done = true;
    }

    public void Update()
    {
        if (!_done) return;
        _done = false;

        // Phase 2: Aggressive chase - jump again if missed
        if (_boss.CurrentPhase == 2 && !_hitRegistered && _boss.ConsecutiveJumps < 3)
        {
            _boss.ConsecutiveJumps++;
            _boss.GoToJumpSlam();
            return;
        }

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
