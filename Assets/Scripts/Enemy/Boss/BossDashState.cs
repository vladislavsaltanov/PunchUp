using UnityEngine;

public class BossDashState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    float _targetX;
    bool _active;
    bool _done;

    public BossDashState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.IsVulnerable = true;
        _boss.IsParryable = false;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        _active = true;
        _done = false;

        float dir = _boss.Player != null
            ? Mathf.Sign(_boss.Player.transform.position.x - _boss.transform.position.x)
            : _boss.direction;

        _targetX = _boss.arenaCenterX + dir * _boss.arenaHalfWidth;
        _boss.direction = (sbyte)dir;
        _boss.UpdateVisualDirection();

        _boss.rb.linearVelocity = Vector2.zero;

        _ = RunDashSequence(dir);
    }

    async Awaitable RunDashSequence(float dir)
    {
        _boss.IsParryable = true;
        await Awaitable.WaitForSecondsAsync(_boss.parryWindowDuration);
        if (!_active) return;

        if (_boss.hitbox != null)
        {
            _boss.hitbox.damage = _boss.dashDamage;
            _boss.hitbox.gameObject.SetActive(true);
        }

        _boss.rb.linearVelocity = new Vector2(dir * _boss.dashSpeed, 0f);

        float maxDashTime = (_boss.arenaHalfWidth * 2f) / _boss.dashSpeed + 0.5f;
        float elapsed = 0f;

        while (_active)
        {
            elapsed += Time.deltaTime;

            bool reached = dir > 0
                ? _boss.transform.position.x >= _targetX
                : _boss.transform.position.x <= _targetX;

            if (reached || elapsed >= maxDashTime) break;
            await Awaitable.NextFrameAsync();
        }

        if (!_active) return;

        _done = true;
    }

    public void Exit()
    {
        _active = false;
        _boss.rb.linearVelocityX = 0f;
        _boss.IsParryable = false;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (!_done) return;
        _done = false;

        _boss.GoToVulnerable();
    }
}
