using UnityEngine;

public class BossDashState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    float _targetX;
    bool _done;

    public BossDashState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.IsVulnerable = false;
        _done = false;

        float dir = _boss.Player != null
            ? Mathf.Sign(_boss.Player.transform.position.x - _boss.transform.position.x)
            : _boss.direction;

        _targetX = _boss.arenaCenterX + dir * _boss.arenaHalfWidth;
        _boss.direction = (sbyte)dir;
        _boss.UpdateVisualDirection();

        if (_boss.hitbox != null)
        {
            _boss.hitbox.damage = _boss.dashDamage;
            _boss.hitbox.gameObject.SetActive(true);
        }

        _boss.rb.linearVelocity = new Vector2(dir * _boss.dashSpeed, 0f);
    }

    public void Update()
    {
        if (_done) return;

        bool reached = _boss.rb.linearVelocityX > 0f
            ? _boss.transform.position.x >= _targetX
            : _boss.transform.position.x <= _targetX;

        if (reached)
        {
            _done = true;
            _boss.GoToVulnerable();
        }
    }

    public void Exit()
    {
        _boss.rb.linearVelocityX = 0f;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
    }
}
