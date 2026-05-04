using UnityEngine;

public class BossVulnerableState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    float _timer;

    public BossVulnerableState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        _boss.rb.linearVelocity = Vector2.zero;
        _boss.IsVulnerable = true;
        _boss.StartShake();
        _timer = _boss.GetVulnerableDuration();
    }

    public void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f) _boss.GoToIdle();
    }

    public void Exit()
    {
        _boss.IsVulnerable = false;
        _boss.StopShake();
    }
}
