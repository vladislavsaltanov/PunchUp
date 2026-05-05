using UnityEngine;

public class BossComboState : IEnemyState
{
    readonly EnemyAIBoss _boss;
    bool _active;
    bool _done;

    const int HitCount = 3;
    const float DashSpeed = 22f;
    const float DashFraction = 0.5f;

    public BossComboState(EnemyAIBoss boss) => _boss = boss;

    public void Enter()
    {
        _boss.IsVulnerable = false;
        _boss.IsParryable = true;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
        _active = true;
        _done = false;
        _ = RunCombo();
    }

    async Awaitable RunCombo()
    {
        float dir = _boss.Player != null
            ? Mathf.Sign(_boss.Player.transform.position.x - _boss.transform.position.x)
            : _boss.direction;

        for (int i = 0; i < HitCount; i++)
        {
            if (!_active) return;

            await Awaitable.WaitForSecondsAsync(_boss.comboPauseBetweenHits);
            if (!_active) return;

            float targetX = Mathf.Clamp(
                _boss.transform.position.x + dir * _boss.arenaHalfWidth * DashFraction,
                _boss.arenaCenterX - _boss.arenaHalfWidth,
                _boss.arenaCenterX + _boss.arenaHalfWidth);

            _boss.direction = (sbyte)dir;
            _boss.UpdateVisualDirection();

            if (_boss.hitbox != null)
            {
                _boss.hitbox.damage = _boss.comboDamage;
                _boss.hitbox.gameObject.SetActive(true);
            }

            _boss.rb.linearVelocity = new Vector2(dir * DashSpeed, 0f);

            while (_active)
            {
                bool reached = dir > 0
                    ? _boss.transform.position.x >= targetX
                    : _boss.transform.position.x <= targetX;
                if (reached) break;
                await Awaitable.NextFrameAsync();
            }

            _boss.rb.linearVelocityX = 0f;
            if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);

            dir = -dir;
        }

        if (!_active) return;
        _done = true;
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
        _boss.rb.linearVelocityX = 0f;
        _boss.IsParryable = false;
        if (_boss.hitbox != null) _boss.hitbox.gameObject.SetActive(false);
    }
}
