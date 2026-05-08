using UnityEngine;

public class BossVulnerableState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    float _timer;

    public BossVulnerableState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.ConsecutiveJumps = 0;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        //_boss.rb.linearVelocity = Vector2.zero;
        _boss.IsVulnerable = true;
        _boss.StartShake();
        _timer = _boss.GetVulnerableDuration();
    }

    public void Update()
    {
        _timer -= Time.deltaTime;
        _boss.rb.linearVelocity = Vector2.Lerp(_boss.rb.linearVelocity, Vector2.zero, Time.deltaTime * 25f);
        if (_timer <= 0f)
        {
            if (_boss.CurrentPhase == 2 && _boss.ShouldJumpAfterVulnerable)
            {
                _boss.ShouldJumpAfterVulnerable = false;
                _boss.GoToJumpSlam();
            }
            else
            {
                _boss.GoToIdle();
            }
        }
    } 

    public void Exit()
    {
        _boss.IsVulnerable = false;
        _boss.StopShake();
    }
}
