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
        if (_boss.groundSlamIndicator == null)
        {
            UnityEngine.Debug.LogWarning("Ground Slam Indicator not assigned in EnemyAIBoss!");
            _done = true;
            return;
        }

        // 1. Warning Phase: Flashing Indicator
        var indicator = _boss.groundSlamIndicator;
        indicator.SetActive(true);
        
        SpriteRenderer sr = indicator.GetComponent<SpriteRenderer>();
        Collider2D col = indicator.GetComponent<Collider2D>();

        float warningDuration = 2.5f;
        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            if (!_active) break;
            
            // Flash effect: Sine wave for alpha
            if (sr != null)
            {
                float alpha = 0.4f + Mathf.Sin(Time.time * 10f) * 0.2f;
                sr.color = new Color(1f, 0f, 0f, alpha);
            }
            
            elapsed += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

        if (!_active) 
        {
            indicator.SetActive(false);
            return;
        }

        // 2. Attack Phase: Solid Red + Damage
        if (sr != null) sr.color = Color.red;
        
        bool hitPlayer = false;
        if (col != null)
        {
            // Check all colliders inside the trigger
            Collider2D[] hits = Physics2D.OverlapBoxAll(col.bounds.center, col.bounds.size, 0f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var player = hit.GetComponent<PlayerController>();
                    if (player != null && Mathf.Abs(player.rb.linearVelocityY) < 0.1f)
                    {
                        player.TakeDamage(_boss.slamDamage, _boss.transform, "Тесей");
                        player.ApplyVelocityOverride(new Vector2(0, 10f), 0.2f);
                        hitPlayer = true;
                    }
                }
            }
        }

        // Visual shake for the slam
        _boss.StartShake();
        await Awaitable.WaitForSecondsAsync(0.5f);
        _boss.StopShake();

        indicator.SetActive(false);

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
