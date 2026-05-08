using UnityEngine;

public class BossPhaseTransitionState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    bool _active;
    bool _done;

    public BossPhaseTransitionState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.IsVulnerable = false;
        _boss.rb.linearVelocity = Vector2.zero;
        _boss.rb.simulated = false;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        _active = true;
        _done = false;
        _ = RunTransition();
    }

    async Awaitable RunTransition()
    {
        _boss.StartShake();
        await Awaitable.WaitForSecondsAsync(_boss.transitionDuration);
        if (!_active) return;

        _boss.StopShake();

        if (_boss.debrisTexture != null)
            VisualEffectsManager.SpawnDebris(_boss.debrisTexture, _boss.transform.position, 40);

        _boss.EnterPhase2();
        _done = true;
    }

    public void Update()
    {
        if (!_done) return;
        _done = false;
        _boss.GoToIdle();
    }

    public void Exit()
    {
        _active = false;
        _boss.rb.simulated = true;
        _boss.StopShake();
    }
}
