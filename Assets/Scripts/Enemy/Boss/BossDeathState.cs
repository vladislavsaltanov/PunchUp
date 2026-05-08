using UnityEngine;

public class BossDeathState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    bool _active;

    public BossDeathState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.IsVulnerable = false;
        _boss.IsParryable = false;
        _boss.CanDealDamage = false;
        
        _boss.rb.linearVelocity = Vector2.zero;
        _boss.rb.bodyType = RigidbodyType2D.Kinematic;
        
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        
        _active = true;
        _ = RunDeathSequence();
    }

    async Awaitable RunDeathSequence()
    {
        // 1. Start of death event
        _boss.OnDeathStarted?.Invoke();
        
        // 2. Shake during the whole death sequence
        _boss.StartShake();

        // 3. Wait for death duration
        await Awaitable.WaitForSecondsAsync(_boss.deathDuration);
        
        if (!_active) return;

        // 4. End of death
        _boss.StopShake();
        _boss.OnDeathEnded?.Invoke();

        // Final Explosion and cleanup
        VisualEffectsManager.SpawnExplosion(_boss.spriteRenderer.sprite.texture, _boss.transform.position, Color.white, 50);
    }

    public void Update()
    {
        // Death is a cinematic sequence, no per-frame logic needed here
    }

    public void Exit()
    {
        _active = false;
    }
}
