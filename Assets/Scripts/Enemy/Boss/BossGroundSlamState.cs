using UnityEngine;

public class BossGroundSlamState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    bool _active;
    bool _done;

    public BossGroundSlamState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.IsVulnerable = false;
        _boss.IsParryable = false;
        _active = true;
        _done = false;
        _ = RunSlam();
    }

    async Awaitable RunSlam()
    {
        // 1. Warning Phase: Red "!"
        _ = _boss.ShowHint(Color.red, 2.5f);
        await Awaitable.WaitForSecondsAsync(2.5f);
        if (!_active) return;

        // 2. Attack Phase: Ground Slam
        bool hitPlayer = false;
        
        // Check for player on ground across the arena
        // Creating a large area check at ground level
        Vector2 slamAreaCenter = new Vector2(_boss.arenaCenterX, _boss.transform.position.y - 1f);
        Vector2 slamAreaSize = new Vector2(_boss.arenaHalfWidth * 2f, 2.5f);
        
        Collider2D playerCol = Physics2D.OverlapBox(slamAreaCenter, slamAreaSize, 0f, 1 << _boss.Player.gameObject.layer);
        
        if (playerCol != null)
        {
            // Only damage if player is NOT jumping (simplified check)
            if (Mathf.Abs(_boss.Player.rb.linearVelocityY) < 0.1f)
            {
                _boss.Player.TakeDamage(_boss.slamDamage, _boss.transform, "Тесей");
                _boss.Player.ApplyVelocityOverride(new Vector2(0, 10f), 0.2f);
                hitPlayer = true;
            }
        }

        // Visual shake for the slam
        _boss.StartShake();
        await Awaitable.WaitForSecondsAsync(0.5f);
        _boss.StopShake();

        if (!_active) return;

        // 3. Outcome
        if (hitPlayer)
        {
            _done = true;
        }
        else
        {
            // Immediately jump if missed
            _boss.GoToJumpSlam();
        }
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
    }
}
