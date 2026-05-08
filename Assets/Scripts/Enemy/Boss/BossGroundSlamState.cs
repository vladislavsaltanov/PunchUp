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

        var indicator = _boss.groundSlamIndicator;
        SpriteRenderer sr = indicator.GetComponent<SpriteRenderer>();
        Collider2D col = indicator.GetComponent<Collider2D>();

        try
        {
            // 1. Warning Phase: Flashing Indicator
            indicator.SetActive(true);
            float warningDuration = 2.5f;
            float elapsed = 0f;
            while (elapsed < warningDuration)
            {
                if (!_active) return;
                if (sr != null)
                {
                    float alpha = 0.4f + Mathf.Sin(Time.time * 10f) * 0.2f;
                    sr.color = new Color(1f, 0f, 0f, alpha);
                }
                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }

            if (!_active) return;

            // 2. Attack Phase: Solid Red + Damage
            if (sr != null) sr.color = Color.red;
            
            bool hitPlayer = false;
            if (col != null && _boss.Player != null)
            {
                var playerCol = _boss.Player.entityCollider;
                if (playerCol != null && col.IsTouching(playerCol))
                {
                    if (Mathf.Abs(_boss.Player.rb.linearVelocityY) < 0.5f)
                    {
                        _boss.Player.ApplyVelocityOverride(new Vector2(0, 15f), 0.2f);
 
                        _boss.Player.TakeDamage(_boss.slamDamage, _boss.transform, "ярость Тесея");
                        hitPlayer = true;
                    }
                }
            }

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
                _boss.GoToJumpSlam();
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
            _done = true;
        }
        finally
        {
            indicator.SetActive(false);
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
